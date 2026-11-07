using System;

namespace Chords.Core.Tuning
{
    /// <summary>In-place radix-2 complex FFT (forward, unscaled) of one power-of-two size.</summary>
    internal sealed class Fft
    {
        private readonly int _size;
        private readonly double[] _cos;
        private readonly double[] _sin;
        private readonly int[] _reversed;

        public Fft(int size)
        {
            if (size < 2 || (size & (size - 1)) != 0)
            {
                throw new ArgumentException("The size must be a power of two.", nameof(size));
            }
            _size = size;
            _cos = new double[size / 2];
            _sin = new double[size / 2];
            for (var i = 0; i < size / 2; i++)
            {
                _cos[i] = Math.Cos(2 * Math.PI * i / size);
                _sin[i] = -Math.Sin(2 * Math.PI * i / size);
            }
            _reversed = new int[size];

            var bits = 0;

            while (1 << bits < size)
            {
                bits++;
            }
            for (var i = 0; i < size; i++)
            {
                var r = 0;

                for (var b = 0; b < bits; b++)
                {
                    r |= ((i >> b) & 1) << (bits - 1 - b);
                }
                _reversed[i] = r;
            }
        }

        public void Transform(double[] re, double[] im)
        {
            for (var i = 0; i < _size; i++)
            {
                var j = _reversed[i];

                if (j > i)
                {
                    (re[i], re[j]) = (re[j], re[i]);
                    (im[i], im[j]) = (im[j], im[i]);
                }
            }
            for (var length = 2; length <= _size; length *= 2)
            {
                var half = length / 2;
                var step = _size / length;

                for (var start = 0; start < _size; start += length)
                {
                    for (var k = 0; k < half; k++)
                    {
                        double c = _cos[k * step], s = _sin[k * step];
                        var a = start + k;
                        var b = a + half;
                        var tr = re[b] * c - im[b] * s;
                        var ti = re[b] * s + im[b] * c;

                        re[b] = re[a] - tr;
                        im[b] = im[a] - ti;
                        re[a] += tr;
                        im[a] += ti;
                    }
                }
            }
        }
    }
}
