using System;
using System.Collections.Generic;
using Chords.Core.Models;

namespace Chords.Core.Extensions
{
    public static class ChordTypeExtensions
    {
        public static Interval[] ToIntervals(this ChordType chordType)
        {
            return ChordIntervals[chordType];
        }

		public static Interval[] ToNonMandatoryIntervals(this ChordType chordType)
		{
            return NonMandatoryChordIntervals.ContainsKey(chordType)
                                             ? NonMandatoryChordIntervals[chordType]
                                             : new Interval[0];
		}

        private static readonly Dictionary<ChordType, Interval[]> ChordIntervals = new Dictionary<ChordType, Interval[]>
        {
            { ChordType.Major,                  new[] {Interval.P1, Interval.M3, Interval.P5} },
			{ ChordType.Minor,                  new[] {Interval.P1, Interval.m3, Interval.P5} },
			{ ChordType.Diminished,             new[] {Interval.P1, Interval.m3, Interval.d5} },
			{ ChordType.Augmented,              new[] {Interval.P1, Interval.M3, Interval.A5} },
			{ ChordType.DominantSeventh,        new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7} },
			{ ChordType.MajorSeventh,           new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M7} },
			{ ChordType.MinorSeventh,           new[] {Interval.P1, Interval.m3, Interval.P5, Interval.m7} },
			{ ChordType.MinorMajorSeventh,      new[] {Interval.P1, Interval.m3, Interval.P5, Interval.M7} },
			{ ChordType.AugmentedSeventh,       new[] {Interval.P1, Interval.M3, Interval.A5, Interval.m7} },
			{ ChordType.AugmentedSeventhFlat9,  new[] {Interval.P1, Interval.M3, Interval.A5, Interval.m7, Interval.m9} },
            { ChordType.AugmentedSeventhSharp9, new[] {Interval.P1, Interval.M3, Interval.A5, Interval.m7, Interval.A9} },
            { ChordType.DominantSeventhFlat5,   new[] {Interval.P1, Interval.M3, Interval.d5, Interval.M7} },
            { ChordType.DominantSeventhFlat9,   new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7, Interval.m9} },
            { ChordType.DominantSeventhSharp9,  new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7, Interval.A9} },
            { ChordType.DominantSeventhSharp11, new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7, Interval.M9, Interval.A11} },
            { ChordType.MinorSeventhFlat5,      new[] {Interval.P1, Interval.m3, Interval.d5, Interval.m7} },
            { ChordType.MinorSeventhFlat9,      new[] {Interval.P1, Interval.m3, Interval.P5, Interval.m7, Interval.m9} },
            { ChordType.DiminishedSeventh,      new[] {Interval.P1, Interval.m3, Interval.d5, Interval.d7} },
            { ChordType.Sus2,                   new[] {Interval.P1, Interval.M2, Interval.P5} },
            { ChordType.Sus4,                   new[] {Interval.P1, Interval.P4, Interval.P5} },
            { ChordType.SeventhSus4,            new[] {Interval.P1, Interval.P4, Interval.P5, Interval.m7} },
            { ChordType.Add2,                   new[] {Interval.P1, Interval.M2, Interval.M3, Interval.P5} },
            { ChordType.Add4,                   new[] {Interval.P1, Interval.M3, Interval.P4, Interval.P5} },
            { ChordType.Add9,                   new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M9} },
            { ChordType.MinorAdd9,              new[] {Interval.P1, Interval.m3, Interval.P5, Interval.M9} },
            { ChordType.Fifth,                  new[] {Interval.P1, Interval.P5} },
            { ChordType.Sixth,                  new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M6} },
            { ChordType.MinorSixth,             new[] {Interval.P1, Interval.m3, Interval.P5, Interval.M6} },
            { ChordType.Ninth,                  new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7, Interval.M9} },
            { ChordType.SixthNinth,             new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M6, Interval.M9} },
            { ChordType.MinorSixthNinth,        new[] {Interval.P1, Interval.m3, Interval.P5, Interval.M6, Interval.M9} },
            { ChordType.MajorNinth,             new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M7, Interval.M9} },
            { ChordType.MinorNinth,             new[] {Interval.P1, Interval.m3, Interval.P5, Interval.m7, Interval.M9} },
            { ChordType.MinorMajorNinth,        new[] {Interval.P1, Interval.m3, Interval.P5, Interval.M7, Interval.M9} },
            { ChordType.AugmentedNinth,         new[] {Interval.P1, Interval.M3, Interval.A5, Interval.m7, Interval.M9} },
            { ChordType.MajorNinthFlat5,        new[] {Interval.P1, Interval.M3, Interval.d5, Interval.m7, Interval.M9} },
            { ChordType.MajorNinthSharp11,      new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M7, Interval.M9, Interval.A11} },
            { ChordType.NinthSus4,              new[] {Interval.P1, Interval.P4, Interval.P5, Interval.m7, Interval.M9} },
            { ChordType.MinorNinthFlat5,        new[] {Interval.P1, Interval.m3, Interval.d5, Interval.m7, Interval.M9} },
            { ChordType.MinorNinthMajorSeventh, new[] {Interval.P1, Interval.m3, Interval.P5, Interval.M7, Interval.M9} },
            { ChordType.Eleventh,               new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7, Interval.M9, Interval.P11} },
            { ChordType.MajorEleventh,          new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M7, Interval.M9, Interval.P11} },
            { ChordType.MinorEleventh,          new[] {Interval.P1, Interval.m3, Interval.P5, Interval.m7, Interval.M9, Interval.P11} },
            { ChordType.Thirteenth,             new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7, Interval.M9, Interval.P11, Interval.M13} },
            { ChordType.MajorThirteenth,        new[] {Interval.P1, Interval.M3, Interval.P5, Interval.M7, Interval.M9, Interval.P11, Interval.M13} },
            { ChordType.MinorThirteenth,        new[] {Interval.P1, Interval.m3, Interval.P5, Interval.m7, Interval.M9, Interval.P11, Interval.M13} },
            { ChordType.ThirteenthFlat9,        new[] {Interval.P1, Interval.M3, Interval.P5, Interval.m7, Interval.m9, Interval.P11, Interval.M13} },
            { ChordType.ThirteenthSus4,         new[] {Interval.P1, Interval.P4, Interval.P5, Interval.m7, Interval.M9, Interval.P11, Interval.M13} }
        };

		private static readonly Dictionary<ChordType, Interval[]> NonMandatoryChordIntervals = new Dictionary<ChordType, Interval[]>
		{ 
            { ChordType.DominantSeventh,        new[] {Interval.P5, Interval.P1} },
		    { ChordType.MajorSeventh,           new[] {Interval.P5, Interval.P1} },
		    { ChordType.MinorSeventh,           new[] {Interval.P5, Interval.P1} },
		    { ChordType.MinorMajorSeventh,      new[] {Interval.P5, Interval.P1} },
		    { ChordType.DominantSeventhFlat9,   new[] {Interval.P5, Interval.P1} },
		    { ChordType.DominantSeventhSharp9,  new[] {Interval.P5, Interval.P1} },
		    { ChordType.DominantSeventhSharp11, new[] {Interval.P5, Interval.P1} },
		    { ChordType.SeventhSus4,            new[] {Interval.P5, Interval.P1} },
		    { ChordType.Sixth,                  new[] {Interval.P5, Interval.P1} },
		    { ChordType.Ninth,                  new[] {Interval.P5, Interval.P1} },
		    { ChordType.SixthNinth,             new[] {Interval.P5, Interval.P1} },
		    { ChordType.MajorNinth,             new[] {Interval.P5, Interval.P1} },
		    { ChordType.MinorNinth,             new[] {Interval.P5, Interval.P1} },
		    { ChordType.MinorMajorNinth,        new[] {Interval.P5, Interval.P1} },
		    { ChordType.Eleventh,               new[] {Interval.M9, Interval.P5, Interval.P1} },
		    { ChordType.MajorEleventh,          new[] {Interval.M9, Interval.P5, Interval.P1} },
		    { ChordType.MinorEleventh,          new[] {Interval.M9, Interval.P5, Interval.P1} },
		    { ChordType.Thirteenth,             new[] {Interval.P11, Interval.M9, Interval.P5, Interval.P1} },
		    { ChordType.MajorThirteenth,        new[] {Interval.P11, Interval.M9, Interval.P5, Interval.P1} },
		    { ChordType.MinorThirteenth,        new[] {Interval.P11, Interval.M9, Interval.P5, Interval.P1} },
		    { ChordType.ThirteenthFlat9,        new[] {Interval.P11, Interval.M9, Interval.P5, Interval.P1} },
		    { ChordType.ThirteenthSus4,         new[] {Interval.P11, Interval.M9, Interval.P5, Interval.P1} }
        };
    }
}
