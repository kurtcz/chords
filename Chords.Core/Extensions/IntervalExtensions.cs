using System;
using System.Collections.Generic;
using Chords.Core.Models;

namespace Chords.Core.Extensions
{
    public static class IntervalExtensions
    {
        public static int ToSteps(this Interval interval)
        {
            return IntervalToSteps[interval];
        }

        public static int ToHalftones(this Interval interval)
        {
            return IntervalToHalftones[interval];
        }

        public static Interval ToInvertedInterval(this Interval interval)
        {
            return InvertedIntervals[interval];
        }

        private static readonly Dictionary<Interval, int> IntervalToSteps = new Dictionary<Interval, int>
        {
            { Interval.P1, 0 }, { Interval.A1, 0 },
            { Interval.d2, 1 }, { Interval.m2, 1 }, { Interval.M2, 1 }, { Interval.A2, 1 },
            { Interval.d3, 2 }, { Interval.m3, 2 }, { Interval.M3, 2 }, { Interval.A3, 2 },
            { Interval.d4, 3 }, { Interval.P4, 3 }, { Interval.A4, 3 },
            { Interval.d5, 4 }, { Interval.P5, 4 }, { Interval.A5, 4 },
            { Interval.d6, 5 }, { Interval.m6, 5 }, { Interval.M6, 5 }, { Interval.A6, 5 },
            { Interval.d7, 6 }, { Interval.m7, 6 }, { Interval.M7, 6 }, { Interval.A7, 6 },
            { Interval.d8, 7 }, { Interval.P8, 7 }, { Interval.A8, 7 },
            { Interval.d9, 8 }, { Interval.m9, 8 }, { Interval.M9, 8 }, { Interval.A9, 8 },
            { Interval.d10, 9 }, { Interval.m10, 9 }, { Interval.M10, 9 }, { Interval.A10, 9 },
            { Interval.d11, 10 }, { Interval.P11, 10 }, { Interval.A11, 10 },
            { Interval.d12, 11 }, { Interval.P12, 11 }, { Interval.A12, 11 },
            { Interval.d13, 12 }, { Interval.m13, 12 }, { Interval.M13, 12 }, { Interval.A13, 12 },
            { Interval.d14, 13 }, { Interval.m14, 13 }, { Interval.M14, 13 }, { Interval.A14, 13 },
            { Interval.d15, 14 }, { Interval.P15, 14 }, { Interval.A15, 14 }
        };

        private static readonly Dictionary<Interval, int> IntervalToHalftones = new Dictionary<Interval, int>
        {
            { Interval.P1, 0 }, { Interval.d2, 0 },
            { Interval.m2, 1 }, { Interval.A1, 1 },
            { Interval.M2, 2 }, { Interval.d3, 2 },
            { Interval.m3, 3 }, { Interval.A2, 3 },
            { Interval.M3, 4 }, { Interval.d4, 4 },
            { Interval.P4, 5 }, { Interval.A3, 5 },
            { Interval.d5, 6 }, { Interval.A4, 6 },
            { Interval.P5, 7 }, { Interval.d6, 7 },
            { Interval.m6, 8 }, { Interval.A5, 8 },
            { Interval.M6, 9 }, { Interval.d7, 9 },
            { Interval.m7, 10 }, { Interval.A6, 10 },
            { Interval.M7, 11 }, { Interval.d8, 11 },
            { Interval.P8, 12 }, { Interval.A7, 12 }, { Interval.d9, 12 },
            { Interval.m9, 13 }, { Interval.A8, 13 },
            { Interval.M9, 14 }, { Interval.d10, 14 },
            { Interval.m10, 15 }, { Interval.A9, 15 },
            { Interval.M10, 16 }, { Interval.d11, 16 },
            { Interval.P11, 17 }, { Interval.A10, 17 },
            { Interval.d12, 18 }, { Interval.A11, 18 },
            { Interval.P12, 19 }, { Interval.d13, 19 },
            { Interval.m13, 20 }, { Interval.A12, 20 },
            { Interval.M13, 21 }, { Interval.d14, 21 },
            { Interval.m14, 22 }, { Interval.A13, 22 },
            { Interval.M14, 23 }, { Interval.d15, 23 },
            { Interval.P15, 24 }, { Interval.A14, 24 },
            { Interval.A15, 25 }
        };

        private static readonly Dictionary<Interval, Interval> InvertedIntervals = new Dictionary<Interval, Interval>
        {
            { Interval.P1, Interval.P1 }, { Interval.d2, Interval.A7 },
            { Interval.m2, Interval.M7 }, { Interval.A1, Interval.d8 },
            { Interval.M2, Interval.m7 }, { Interval.d3, Interval.A6 },
            { Interval.m3, Interval.M6 }, { Interval.A2, Interval.d7 },
            { Interval.M3, Interval.m6 }, { Interval.d4, Interval.A5 },
            { Interval.P4, Interval.P5 }, { Interval.A3, Interval.d6 },
            { Interval.d5, Interval.A4 }, { Interval.A4, Interval.d5 },
            { Interval.P5, Interval.P4 }, { Interval.d6, Interval.A3 },
            { Interval.m6, Interval.M3 }, { Interval.A5, Interval.d4 },
            { Interval.M6, Interval.m3 }, { Interval.d7, Interval.A2 },
            { Interval.m7, Interval.M2 }, { Interval.A6, Interval.d3 },
            { Interval.M7, Interval.m2 }, { Interval.d8, Interval.A1 },
            { Interval.P8, Interval.P1 }, { Interval.A7, Interval.d2 }, { Interval.d9, Interval.P1 },
            { Interval.m9, Interval.M7 }, { Interval.A8, Interval.d8 },
            { Interval.M9, Interval.m7 }, { Interval.d10, Interval.A6 },
            { Interval.m10, Interval.M6 }, { Interval.A9, Interval.d7 },
            { Interval.M10, Interval.m6 }, { Interval.d11, Interval.A5 },
            { Interval.P11, Interval.P5 }, { Interval.A10, Interval.d6 },
            { Interval.d12, Interval.A4 }, { Interval.A11, Interval.d5 },
            { Interval.P12, Interval.P4 }, { Interval.d13, Interval.A3 },
            { Interval.m13, Interval.M3 }, { Interval.A12, Interval.d4 },
            { Interval.M13, Interval.m3 }, { Interval.d14, Interval.A2 },
            { Interval.m14, Interval.M2 }, { Interval.A13, Interval.d3 },
            { Interval.M14, Interval.m7 }, { Interval.d15, Interval.A1 },
            { Interval.P15, Interval.P1 }, { Interval.A14, Interval.d2 },
            { Interval.A15, Interval.d8 }
        };
    }
}
