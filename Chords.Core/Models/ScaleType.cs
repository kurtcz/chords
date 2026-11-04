using System;
namespace Chords.Core.Models
{
    public enum ScaleType
    {
        Chromatic,
        Major,
        Minor,
        HarmonicMinor,
        MelodicMinor,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        /// <summary>Fifth mode of harmonic minor (Freygish, Ahava Rabbah): Phrygian with a major third.</summary>
        PhrygianDominant,
        /// <summary>Harmonic minor with a raised fourth (Gypsy minor).</summary>
        HungarianMinor,
        /// <summary>Major with a lowered second and sixth (Gypsy major, Byzantine, Arabic).</summary>
        DoubleHarmonicMajor,
        /// <summary>Fourth mode of melodic minor (acoustic scale): Lydian with a minor seventh.</summary>
        LydianDominant,
        /// <summary>Dorian with a raised fourth (Romanian minor, Misheberakh).</summary>
        UkrainianDorian,
        /// <summary>Seventh mode of melodic minor (super-Locrian): every note above the root lowered.</summary>
        Altered,
        MajorPentatonic,
        MinorPentatonic,
        /// <summary>Minor pentatonic with the flat fifth (six notes).</summary>
        Blues,
        /// <summary>Japanese pentatonic: 1 2 ♭3 5 ♭6.</summary>
        Hirajoshi,
        /// <summary>Japanese pentatonic (Miyako-bushi): 1 ♭2 4 5 ♭6.</summary>
        In,
        /// <summary>Japanese pentatonic: 1 2 4 5 6.</summary>
        Yo,
        /// <summary>Japanese pentatonic: 1 2 ♭3 5 6.</summary>
        Kumoi,
        /// <summary>Suspended pentatonic: 1 2 4 5 ♭7.</summary>
        Egyptian
    }
}
