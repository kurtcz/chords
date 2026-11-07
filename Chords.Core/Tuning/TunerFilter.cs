using System;

namespace Chords.Core.Tuning
{
    /// <summary>
    /// Band filter applied to the microphone stream before pitch detection: a 2nd-order Butterworth high-pass well
    /// below the lowest note (rumble, handling noise, DC) and a 4th-order Butterworth low-pass (two cascaded biquads).
    /// Both leave the period of a note alone; they only take away noise that makes quiet notes less clear.
    /// With a string locked, the low-pass also sits low enough to fix the following. A string's upper partials run sharp (inharmonicity: the 12th harmonic of a low E is about
    /// +11 cents, higher ones more). The period YIN finds weights partials by the square of their frequency,
    /// so when the fundamental is weak (a phone speaker or a small microphone barely passes 80 Hz) those
    /// sharp partials pull the reading up by 10–20 cents. Cutting them leaves the fundamental and the lowest
    /// harmonics, which are close to the true pitch.
    /// Not thread-safe; keeps state between blocks, so one instance per audio stream.
    /// </summary>
    public class TunerFilter
    {
        /// <summary>Q of the two sections of a 4th-order Butterworth filter.</summary>
        private static readonly double[] SectionQ = { 0.54119610, 1.30656296 };
        /// <summary>Q of a 2nd-order Butterworth filter.</summary>
        private const double ButterworthQ = 0.70710678;

        private readonly Biquad[] _sections;

        public float HighPass { get; }
        public float LowPass { get; }

        public TunerFilter(int sampleRate, float highPass, float lowPass)
        {
            if (sampleRate <= 0 || highPass <= 0 || lowPass <= highPass)
            {
                throw new ArgumentException("Invalid sample rate or cutoffs.");
            }
            // Never at or above Nyquist, where the design breaks down.
            HighPass = highPass;
            LowPass = Math.Min(lowPass, sampleRate * 0.45f);
            _sections = new Biquad[SectionQ.Length + 1];
            _sections[0] = new Biquad(sampleRate, HighPass, ButterworthQ, highPass: true);
            for (var i = 0; i < SectionQ.Length; i++)
            {
                _sections[i + 1] = new Biquad(sampleRate, LowPass, SectionQ[i], highPass: false);
            }
        }

        public TunerFilter(int sampleRate, (float HighPass, float LowPass) band) : this(sampleRate, band.HighPass, band.LowPass)
        {
        }

        /// <summary>Filters <paramref name="count"/> samples of <paramref name="input"/> from <paramref name="offset"/> into <paramref name="output"/>.</summary>
        public void Process(float[] input, int offset, int count, float[] output)
        {
            for (var i = 0; i < count; i++)
            {
                double x = input[offset + i];

                foreach (var section in _sections)
                {
                    x = section.Next(x);
                }
                output[i] = (float)x;
            }
        }

        /// <summary>Filters a whole signal (a fresh filter state).</summary>
        public static float[] Apply(float[] samples, int sampleRate, (float HighPass, float LowPass) band)
        {
            var output = new float[samples.Length];

            new TunerFilter(sampleRate, band).Process(samples, 0, samples.Length, output);

            return output;
        }

        /// <summary>RBJ cookbook low-pass or high-pass, direct form I.</summary>
        private sealed class Biquad
        {
            private readonly double _b0, _b1, _b2, _a1, _a2;
            private double _x1, _x2, _y1, _y2;

            public Biquad(int sampleRate, double cutoff, double q, bool highPass)
            {
                var w = 2 * Math.PI * cutoff / sampleRate;
                var alpha = Math.Sin(w) / (2 * q);
                var cos = Math.Cos(w);
                var a0 = 1 + alpha;

                _b0 = (highPass ? 1 + cos : 1 - cos) / 2 / a0;
                _b1 = (highPass ? -(1 + cos) : 1 - cos) / a0;
                _b2 = _b0;
                _a1 = -2 * cos / a0;
                _a2 = (1 - alpha) / a0;
            }

            public double Next(double x)
            {
                var y = _b0 * x + _b1 * _x1 + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;

                _x2 = _x1;
                _x1 = x;
                _y2 = _y1;
                _y1 = y;

                return y;
            }
        }
    }
}
