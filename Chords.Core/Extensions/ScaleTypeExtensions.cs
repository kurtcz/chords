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

        private static Dictionary<ScaleType, Interval[]> ScaleTypeToIntervals = new Dictionary<ScaleType, Interval[]>
        {
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
