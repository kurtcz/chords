using System;

namespace Chords.Core.Tuning
{
    /// <summary>Pitch of one analysed frame.</summary>
    public readonly struct PitchEstimate
    {
        /// <summary>Fundamental frequency in Hz; 0 when no pitch was found.</summary>
        public float Frequency { get; }
        /// <summary>How periodic the frame is, 0–1 (1 = a perfect periodic signal).</summary>
        public float Clarity { get; }
        /// <summary>Root mean square of the frame (loudness, 1 = full scale).</summary>
        public float Rms { get; }

        public bool HasPitch => Frequency > 0;

        public PitchEstimate(float frequency, float clarity, float rms)
        {
            Frequency = frequency;
            Clarity = clarity;
            Rms = rms;
        }
    }

    /// <summary>
    /// Finds the fundamental frequency of a single note with the YIN algorithm
    /// (de Cheveigné &amp; Kawahara, 2002), searching only between <c>minFrequency</c> and <c>maxFrequency</c>.
    /// Not thread-safe: one instance per audio stream.
    /// </summary>
    public class PitchDetector
    {
        /// <summary>A dip of the normalized difference below this is a candidate period.</summary>
        private const float Threshold = 0.15f;
        /// <summary>
        /// An earlier (shorter) candidate is accepted only when it is about as deep as the deepest dip.
        /// Low strings often have a stronger 2nd harmonic than their fundamental; without this the
        /// half period would win and the note would read an octave too high.
        /// </summary>
        private const float RelativeDepth = 0.1f;
        /// <summary>
        /// Frames quieter than this (−80 dBFS) are not analysed at all. It is far below any room's noise: whether a
        /// frame is loud enough to be a note is decided against the measured noise floor (see <see cref="NoiseFloor"/>).
        /// </summary>
        public const float SilenceRms = 0.0001f;

        private readonly int _sampleRate;
        private readonly int _minLag;
        private readonly int _maxLag;
        private readonly int _window;
        private readonly float[] _difference;
        /// <summary>The difference function before normalization, which the exact period is interpolated on.</summary>
        private readonly double[] _raw;
        private readonly Fft _fft;
        /// <summary>Real part: the window; imaginary part: the whole frame (both zero-padded), transformed in place.</summary>
        private readonly double[] _re, _im;
        /// <summary>Running sums of the squared frame samples, for the energy of every lagged window.</summary>
        private readonly double[] _energy;

        /// <summary>Samples one analysis needs: the integration window plus the longest period.</summary>
        public int FrameSize { get; }

        public PitchDetector(int sampleRate, float minFrequency, float maxFrequency)
        {
            if (sampleRate <= 0 || minFrequency <= 0 || maxFrequency <= minFrequency)
            {
                throw new ArgumentException("Invalid sample rate or frequency range.");
            }
            _sampleRate = sampleRate;
            _minLag = Math.Max(2, (int)Math.Floor(sampleRate / maxFrequency));
            _maxLag = (int)Math.Ceiling(sampleRate / minFrequency) + 1;
            // Two periods of the lowest note keep the estimate stable; never fewer than 1024 samples.
            _window = Math.Max(1024, 2 * _maxLag);
            FrameSize = _window + _maxLag + 1;
            _difference = new float[_maxLag + 2];
            _raw = new double[_maxLag + 2];
            // Large enough that the circular correlation never wraps around.
            var size = 1;

            while (size < _window + FrameSize)
            {
                size *= 2;
            }
            _fft = new Fft(size);
            _re = new double[size];
            _im = new double[size];
            _energy = new double[FrameSize + 1];
        }

        /// <summary>
        /// Analyses the last <see cref="FrameSize"/> samples of <paramref name="samples"/> ending at <paramref name="end"/>.
        /// A frame quieter than <paramref name="minRms"/> gets its loudness only, without the pitch search.
        /// </summary>
        public PitchEstimate Detect(float[] samples, int end, float minRms = SilenceRms)
        {
            var start = end - FrameSize;

            if (start < 0)
            {
                throw new ArgumentException("Not enough samples.", nameof(samples));
            }
            var rms = Rms(samples, start, _window);

            if (rms < Math.Max(SilenceRms, minRms))
            {
                return new PitchEstimate(0, 0, rms);
            }
            ComputeNormalizedDifference(samples, start);

            var lag = PickLag();

            if (lag < 0)
            {
                return new PitchEstimate(0, 0, rms);
            }
            var clarity = 1 - Math.Min(1, _difference[lag]);
            var refined = Interpolate(lag);

            return new PitchEstimate((float)(_sampleRate / refined), clarity, rms);
        }

        /// <summary>Analyses a whole buffer (its last <see cref="FrameSize"/> samples).</summary>
        public PitchEstimate Detect(float[] samples, float minRms = SilenceRms) => Detect(samples, samples.Length, minRms);

        private static float Rms(float[] x, int start, int count)
        {
            double sum = 0;

            for (var i = start; i < start + count; i++)
            {
                sum += x[i] * x[i];
            }

            return (float)Math.Sqrt(sum / count);
        }

        /// <summary>
        /// YIN steps 2 and 3: difference function and its cumulative mean normalization. The difference of the window
        /// and its copy lagged by τ is E(0) + E(τ) − 2·r(τ), with E the windows' energies and r their cross-correlation,
        /// which one FFT gives for every lag at once (O(N log N) instead of O(window · lags)).
        /// </summary>
        private void ComputeNormalizedDifference(float[] x, int start)
        {
            var n = _re.Length;

            _energy[0] = 0;
            for (var i = 0; i < FrameSize; i++)
            {
                double v = x[start + i];

                _energy[i + 1] = _energy[i] + v * v;
                _re[i] = i < _window ? v : 0;
                _im[i] = v;
            }
            Array.Clear(_re, FrameSize, n - FrameSize);
            Array.Clear(_im, FrameSize, n - FrameSize);

            // Both real signals in one complex transform: z = window + i·frame.
            _fft.Transform(_re, _im);

            // Spectrum of the correlation, conj(W)·F, split out of Z; stored conjugated for the inverse below.
            for (var k = 0; k <= n / 2; k++)
            {
                ConjugateCorrelation(k, (n - k) % n, out var cr, out var ci);

                if (k != 0 && k != n / 2)
                {
                    // The other half of the spectrum is its mirror image (the correlation is real).
                    ConjugateCorrelation(n - k, k, out var mr, out var mi);
                    _re[n - k] = mr;
                    _im[n - k] = mi;
                }
                _re[k] = cr;
                _im[k] = ci;
            }
            // Forward transform of the conjugate: its real part over n is the inverse transform.
            _fft.Transform(_re, _im);

            var e0 = _energy[_window];

            _difference[0] = 1;

            double running = 0;

            for (var lag = 1; lag <= _maxLag + 1; lag++)
            {
                var correlation = _re[lag] / n;
                var sum = Math.Max(0, e0 + _energy[lag + _window] - _energy[lag] - 2 * correlation);

                _raw[lag] = sum;
                running += sum;
                _difference[lag] = running > 0 ? (float)(sum * lag / running) : 1;
            }
        }

        /// <summary>conj(conj(W[k])·F[k]) from the transform Z of window + i·frame (<paramref name="mirror"/> = −k mod n).</summary>
        private void ConjugateCorrelation(int k, int mirror, out double re, out double im)
        {
            double zr = _re[k], zi = _im[k], mr = _re[mirror], mi = _im[mirror];
            // W[k] = (Z[k] + conj Z[−k]) / 2, F[k] = (Z[k] − conj Z[−k]) / 2i.
            double wr = (zr + mr) / 2, wi = (zi - mi) / 2;
            double fr = (zi + mi) / 2, fi = -(zr - mr) / 2;

            re = wr * fr + wi * fi;
            im = -(wr * fi - wi * fr);
        }

        /// <summary>
        /// The first dip below the threshold that is about as deep as the deepest one (shortest period wins
        /// among equals, so the true note beats its lower octaves); -1 when the frame is not periodic.
        /// </summary>
        private int PickLag()
        {
            var best = -1;

            for (var lag = _minLag; lag <= _maxLag; lag++)
            {
                if (IsLocalMinimum(lag) && (best < 0 || _difference[lag] < _difference[best]))
                {
                    best = lag;
                }
            }
            if (best < 0 || _difference[best] > Threshold * 2)
            {
                return -1;
            }
            var limit = Math.Max(_difference[best], Math.Min(Threshold, _difference[best] + RelativeDepth));

            for (var lag = _minLag; lag < best; lag++)
            {
                if (IsLocalMinimum(lag) && _difference[lag] <= limit)
                {
                    return lag;
                }
            }

            return best;
        }

        private bool IsLocalMinimum(int lag)
            => _difference[lag] <= _difference[lag - 1] && _difference[lag] <= _difference[lag + 1];

        /// <summary>Half width of the parabola fitted to the dip, as a fraction of the period.</summary>
        private const double FitWidth = 1.0 / 64;

        /// <summary>
        /// Least-squares parabola over the bottom of the dip of the raw difference: sub-sample period for cent accuracy.
        /// In noise, neighbouring lags of a low note differ more by noise than by the shape of its wide dip, so a parabola
        /// through just three of them jitters by many cents; its width follows the period. And the raw difference, as
        /// YIN prescribes: noise lifts the bottom of the dip off zero, and the normalization, which grows with the lag,
        /// then tilts it towards shorter periods (a quiet note read sharp).
        /// </summary>
        private double Interpolate(int lag)
        {
            var m = Math.Max(1, (int)Math.Round(lag * FitWidth));
            // Bottom of the raw dip near the normalized one.
            var from = Math.Max(1, lag - m);
            var to = Math.Min(_maxLag, lag + m);

            for (var i = from; i <= to; i++)
            {
                if (_raw[i] < _raw[lag])
                {
                    lag = i;
                }
            }
            m = Math.Min(m, Math.Min(lag - 1, _maxLag + 1 - lag));
            if (m < 1)
            {
                return lag;
            }
            double sy = 0, sxy = 0, sxxy = 0, s2 = 0, s4 = 0;

            for (var i = -m; i <= m; i++)
            {
                var y = _raw[lag + i];

                sy += y;
                sxy += i * y;
                sxxy += (double)i * i * y;
                s2 += (double)i * i;
                s4 += (double)i * i * i * i;
            }
            var count = 2 * m + 1;
            var curvature = (sxxy - s2 / count * sy) / (s4 - s2 * s2 / count);

            if (curvature <= 1e-30)
            {
                return lag;
            }
            var shift = -(sxy / s2) / (2 * curvature);

            return lag + Math.Max(-m, Math.Min(m, shift));
        }
    }
}
