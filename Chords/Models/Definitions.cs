using System;
using System.Collections.Generic;

namespace Chords.Models
{
	public static class Definitions
	{
		public static readonly Dictionary<NamingConvention, string[]> Notes = new Dictionary<NamingConvention, string[]>
		{
			{ NamingConvention.English, new[] { "C", "D", "E", "F", "G", "A", "B" }},
            { NamingConvention.German, new[] { "C", "D", "E", "F", "G", "A", "H" }},
			{ NamingConvention.Latin, new[] { "Do", "Re", "Mi", "Fa", "Sol", "La", "Si" }}
		};
		public static readonly Dictionary<NamingConvention, string[]> SharpNotes = new Dictionary<NamingConvention, string[]>
		{
			{ NamingConvention.English, new[] { "C#", "D#", "E#", "F#", "G#", "A#", "B#" }},
			{ NamingConvention.German, new[] { "C#", "D#", "E#", "F#", "G#", "A#", "H#" }},
			{ NamingConvention.Latin, new[] { "Do#", "Re#", "Mi#", "Fa#", "Sol#", "La#", "Si#" }}
		};
		public static readonly Dictionary<NamingConvention, string[]> FlatNotes = new Dictionary<NamingConvention, string[]>
		{
			{ NamingConvention.English, new[] { "Cb", "Db", "Eb", "Fb", "Gb", "Ab", "Bb" }},
			{ NamingConvention.German, new[] { "Cb", "Db", "Eb", "Fb", "Gb", "Ab", "B" }},
			{ NamingConvention.Latin, new[] { "Dob", "Reb", "Mib", "Fab", "Solb", "Lab", "Sib" }}
		};
		public static readonly Dictionary<NamingConvention, string[]> Notes1 = new Dictionary<NamingConvention, string[]>
		{
			{ NamingConvention.English, new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" }},
			{ NamingConvention.German, new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "H" }},
			{ NamingConvention.Latin, new[] { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si" }}
		};

		public static readonly Dictionary<NamingConvention, string[]> Notes2 = new Dictionary<NamingConvention, string[]>
		{
			{ NamingConvention.English, new[] { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" }},
			{ NamingConvention.German, new[] { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "B", "H" }},
			{ NamingConvention.Latin, new[] { "Do", "Reb", "Re", "Mib", "Mi", "Fa", "Solb", "Sol", "Lab", "La", "Sib", "Si" }}
		};

		public static readonly Dictionary<Interval, int> IntervalToHalftones = new Dictionary<Interval, int>
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

		public static readonly Dictionary<ChordType, Interval[]> ChordIntervals = new Dictionary<ChordType, Interval[]>
		{ { ChordType.Major,                  new[] {Interval.P1, Interval.M3, Interval.P5} },
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
		  { ChordType.ThirteenthSus4,         new[] {Interval.P1, Interval.P4, Interval.P5, Interval.m7, Interval.M9, Interval.P11, Interval.M13} }};

		public static readonly Dictionary<ChordType, Interval[]> NonMandatoryChordIntervals = new Dictionary<ChordType, Interval[]>
        { { ChordType.DominantSeventh,        new[] {Interval.P5, Interval.P1} },
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
		  { ChordType.ThirteenthSus4,         new[] {Interval.P11, Interval.M9, Interval.P5, Interval.P1} }};

		//public static readonly Dictionary<ScaleType, int[]> Scales = new Dictionary<ScaleType, int[]>
		//{ { ScaleType.Major, new[] {0, 2, 4, 5, 7, 9, 11, 12} },
		//  { ScaleType.Minor, new[] {0, 2, 3, 5, 7, 8, 11, 12} }};
	}
}
