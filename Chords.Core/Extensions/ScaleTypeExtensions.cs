using System;
using System.Collections.Generic;
using Chords.Core.Models;

namespace Chords.Core.Extensions
{
    public static class ScaleTypeExtensions
    {
        public static Interval[] ToIntervals(this ScaleType scaleType)
        {
            return ScaleTypeToIntervals[scaleType];
        }

        private static Interval[] Heptatonic(Interval second, Interval third, Interval fourth, Interval sixth, Interval seventh)
        {
            return new[] { Interval.P1, second, third, fourth, Interval.P5, sixth, seventh };
        }

        private static Dictionary<ScaleType, Interval[]> ScaleTypeToIntervals = new Dictionary<ScaleType, Interval[]>
        {
            { ScaleType.HarmonicMinor, Heptatonic(Interval.M2, Interval.m3, Interval.P4, Interval.m6, Interval.M7) },
            { ScaleType.MelodicMinor, Heptatonic(Interval.M2, Interval.m3, Interval.P4, Interval.M6, Interval.M7) },
            { ScaleType.Dorian, Heptatonic(Interval.M2, Interval.m3, Interval.P4, Interval.M6, Interval.m7) },
            { ScaleType.Phrygian, Heptatonic(Interval.m2, Interval.m3, Interval.P4, Interval.m6, Interval.m7) },
            { ScaleType.Lydian, Heptatonic(Interval.M2, Interval.M3, Interval.A4, Interval.M6, Interval.M7) },
            { ScaleType.Mixolydian, Heptatonic(Interval.M2, Interval.M3, Interval.P4, Interval.M6, Interval.m7) },
            {
                ScaleType.Chromatic,
                new []
                {
                    Interval.P1,
                    Interval.m2,
                    Interval.M2,
                    Interval.m3,
                    Interval.M3,
                    Interval.P4,
                    Interval.d5,
                    Interval.P5,
                    Interval.m6,
                    Interval.M6,
                    Interval.m7,
                    Interval.M7
                }
            },
            {
                ScaleType.Major,
                new []
                {
                    Interval.P1,
                    Interval.M2,
                    Interval.M3,
                    Interval.P4,
                    Interval.P5,
                    Interval.M6,
                    Interval.M7
                }
            },
            {
                ScaleType.Minor,
                new []
                {
                    Interval.P1,
                    Interval.M2,
                    Interval.m3,
                    Interval.P4,
                    Interval.P5,
                    Interval.m6,
                    Interval.m7
                }
            }
        };
    }
}
