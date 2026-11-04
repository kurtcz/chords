using System;
using Chords.Core.Attributes;

namespace Chords.Core.Models
{
	public enum ChordType
	{
        [Descriptor("")]
		Major,
        [Descriptor("mi", "m")]
		Minor,
        [Descriptor("dim", "°")]
		Diminished,
        [Descriptor("aug", "+")]
		Augmented,
        [Descriptor("7")]
		DominantSeventh,
        [Descriptor("maj7")]
		MajorSeventh,
        [Descriptor("mi7", "m7")]
        MinorSeventh,
        [Descriptor("mmaj7")]
        MinorMajorSeventh,
		[Descriptor("dim7", "°7")]
		DiminishedSeventh,
		[Descriptor("aug7", "+7")]
		AugmentedSeventh,
		[Descriptor("aug7b9", "+7b9")]
		AugmentedSeventhFlat9,
		[Descriptor("aug7#9", "+7#9")]
		AugmentedSeventhSharp9,
		[Descriptor("7b5")]
		DominantSeventhFlat5,
		[Descriptor("mi7b5", "m7b5")]
		MinorSeventhFlat5,
		[Descriptor("7b9")]
		DominantSeventhFlat9,
		[Descriptor("mi7b9", "m7b9")]
		MinorSeventhFlat9,
		[Descriptor("7#9")]
		DominantSeventhSharp9,
		[Descriptor("7#11")]
		DominantSeventhSharp11,
		[Descriptor("7sus4")]
        SeventhSus4,
        [Descriptor("sus2")]
		Sus2,
        [Descriptor("sus4")]
		Sus4,
        [Descriptor("add2")]
		Add2,
        [Descriptor("add4")]
		Add4,
		[Descriptor("add9")]
		Add9,
        [Descriptor("mi(add9)", "m(add9)")]
		MinorAdd9,
		[Descriptor("5")]
		Fifth,
        [Descriptor("6")]
		Sixth,
		[Descriptor("mi6", "m6")]
		MinorSixth,
		[Descriptor("9")]
        Ninth,
		[Descriptor("6/9")]
		SixthNinth,
		[Descriptor("mi6/9", "m6/9")]
		MinorSixthNinth,
		[Descriptor("maj9", "9maj7")]
        MajorNinth,
        [Descriptor("mi9", "m9")]
        MinorNinth,
		[Descriptor("mmaj9")]
		MinorMajorNinth,
		[Descriptor("aug9", "+9")]
		AugmentedNinth,
		[Descriptor("9b5")]
		MajorNinthFlat5,
		[Descriptor("9#11")]
        MajorNinthSharp11,
		[Descriptor("9sus4")]
		NinthSus4,
		[Descriptor("mi9b5", "m9b5")]
		MinorNinthFlat5,
		[Descriptor("mi9maj7", "m9maj7")]
		MinorNinthMajorSeventh,
		[Descriptor("11")]
        Eleventh,
        [Descriptor("maj11", "11maj7")]
        MajorEleventh,
        [Descriptor("mi11", "m11")]
        MinorEleventh,
		[Descriptor("13")]
		Thirteenth,
		[Descriptor("maj13", "13maj7")]
        MajorThirteenth,
        [Descriptor("mi13", "m13")]
        MinorThirteenth,
        [Descriptor("13b9")]
        ThirteenthFlat9,
		[Descriptor("13sus4")]
		ThirteenthSus4
	}
}
