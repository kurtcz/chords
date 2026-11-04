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

        /// <summary>
        /// The seven-note scale whose chords harmonize a scale: the scale itself when it has seven notes,
        /// otherwise the mode that contains it (Hirajoshi lies in natural minor, In in Phrygian…).
        /// </summary>
        public static ScaleType Harmony(this ScaleType scaleType)
        {
            switch (scaleType)
            {
                case ScaleType.MajorPentatonic: return ScaleType.Major;
                case ScaleType.MinorPentatonic:
                case ScaleType.Blues:
                case ScaleType.Hirajoshi: return ScaleType.Minor;
                case ScaleType.In: return ScaleType.Phrygian;
                case ScaleType.Yo:
                case ScaleType.Kumoi: return ScaleType.Dorian;
                case ScaleType.Egyptian: return ScaleType.Mixolydian;
                default: return scaleType;
            }
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
            { ScaleType.PhrygianDominant, Heptatonic(Interval.m2, Interval.M3, Interval.P4, Interval.m6, Interval.m7) },
            { ScaleType.HungarianMinor, Heptatonic(Interval.M2, Interval.m3, Interval.A4, Interval.m6, Interval.M7) },
            { ScaleType.DoubleHarmonicMajor, Heptatonic(Interval.m2, Interval.M3, Interval.P4, Interval.m6, Interval.M7) },
            { ScaleType.LydianDominant, Heptatonic(Interval.M2, Interval.M3, Interval.A4, Interval.M6, Interval.m7) },
            { ScaleType.UkrainianDorian, Heptatonic(Interval.M2, Interval.m3, Interval.A4, Interval.M6, Interval.m7) },
            // Spelled with all seven letters (C Db Eb Fb Gb Ab Bb), so it has a diminished fifth.
            { ScaleType.Altered, new[] { Interval.P1, Interval.m2, Interval.m3, Interval.d4, Interval.d5, Interval.m6, Interval.m7 } },
            { ScaleType.MajorPentatonic, new[] { Interval.P1, Interval.M2, Interval.M3, Interval.P5, Interval.M6 } },
            { ScaleType.MinorPentatonic, new[] { Interval.P1, Interval.m3, Interval.P4, Interval.P5, Interval.m7 } },
            { ScaleType.Blues, new[] { Interval.P1, Interval.m3, Interval.P4, Interval.d5, Interval.P5, Interval.m7 } },
            { ScaleType.Hirajoshi, new[] { Interval.P1, Interval.M2, Interval.m3, Interval.P5, Interval.m6 } },
            { ScaleType.In, new[] { Interval.P1, Interval.m2, Interval.P4, Interval.P5, Interval.m6 } },
            { ScaleType.Yo, new[] { Interval.P1, Interval.M2, Interval.P4, Interval.P5, Interval.M6 } },
            { ScaleType.Kumoi, new[] { Interval.P1, Interval.M2, Interval.m3, Interval.P5, Interval.M6 } },
            { ScaleType.Egyptian, new[] { Interval.P1, Interval.M2, Interval.P4, Interval.P5, Interval.m7 } },
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
