using System;
using System.Numerics;

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
        /// <summary>Frames quieter than this are silence (about −50 dBFS).</summary>
        public const float SilenceRms = 0.003f;

        private readonly int _sampleRate;
        private readonly int _minLag;
        private readonly int _maxLag;
        private readonly int _window;
        private readonly float[] _difference;

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
        }

        /// <summary>Analyses the last <see cref="FrameSize"/> samples of <paramref name="samples"/> ending at <paramref name="end"/>.</summary>
        public PitchEstimate Detect(float[] samples, int end)
        {
            var start = end - FrameSize;

            if (start < 0)
            {
                throw new ArgumentException("Not enough samples.", nameof(samples));
            }
            var rms = Rms(samples, start, _window);

            if (rms < SilenceRms)
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
        public PitchEstimate Detect(float[] samples) => Detect(samples, samples.Length);

        private static float Rms(float[] x, int start, int count)
        {
            double sum = 0;

            for (var i = start; i < start + count; i++)
            {
                sum += x[i] * x[i];
            }

            return (float)Math.Sqrt(sum / count);
        }

        /// <summary>YIN steps 2 and 3: difference function and its cumulative mean normalization.</summary>
        private void ComputeNormalizedDifference(float[] x, int start)
        {
            _difference[0] = 1;

            double running = 0;

            var width = Vector<float>.Count;
            var vectorEnd = _window - _window % width;

            for (var lag = 1; lag <= _maxLag + 1; lag++)
            {
                // SIMD: the window is summed in float lanes (a few thousand terms; plenty for cent accuracy).
                var lanes = Vector<float>.Zero;
                var a = start;
                var b = start + lag;
                var j = 0;

                for (; j < vectorEnd; j += width)
                {
                    var d = new Vector<float>(x, a + j) - new Vector<float>(x, b + j);
                    lanes += d * d;
                }
                double sum = Vector.Dot(lanes, Vector<float>.One);

                for (; j < _window; j++)
                {
                    var d = x[a + j] - x[b + j];
                    sum += d * d;
                }
                running += sum;
                _difference[lag] = running > 0 ? (float)(sum * lag / running) : 1;
            }
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

        /// <summary>Parabola through the dip and its neighbours: sub-sample period for cent accuracy.</summary>
        private double Interpolate(int lag)
        {
            double s0 = _difference[lag - 1], s1 = _difference[lag], s2 = _difference[lag + 1];
            var denominator = s0 - 2 * s1 + s2;

            if (Math.Abs(denominator) < 1e-12)
            {
                return lag;
            }
            var shift = 0.5 * (s0 - s2) / denominator;

            return lag + Math.Max(-1, Math.Min(1, shift));
        }
    }
}
