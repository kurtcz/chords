using System;
using System.Collections.Generic;
using System.Linq;

namespace Chords.Core.Models
{
    public enum ProgressionStyle
    {
        Pop,
        Rock,
        Jazz,
        Blues,
        Classical,
        Flamenco
    }

    /// <summary>
    /// A chord progression written in Roman numerals, playable in any key.
    /// </summary>
    /// <remarks>
    /// A numeral is the diatonic chord of its degree unless it says otherwise: a ♭/♯ prefix moves the root,
    /// upper/lower case asks for a major/minor chord and a suffix sets the type
    /// (7 = dominant or minor seventh, maj7, ø7 = half-diminished, ° = diminished).
    /// </remarks>
    public class Progression
    {
        public string[] Numerals { get; }
        public ProgressionStyle Style { get; }
        /// <summary>Written for a major key; otherwise for a minor one.</summary>
        public bool ForMajor { get; }

        public Progression(ProgressionStyle style, bool forMajor, string numerals)
        {
            Style = style;
            ForMajor = forMajor;
            Numerals = numerals.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Well-known progressions.</summary>
        public static readonly Progression[] Common =
        {
            new Progression(ProgressionStyle.Pop, true, "I V vi IV"),
            new Progression(ProgressionStyle.Pop, true, "I vi IV V"),
            new Progression(ProgressionStyle.Pop, true, "vi IV I V"),
            new Progression(ProgressionStyle.Pop, true, "IV V iii vi"),
            new Progression(ProgressionStyle.Rock, true, "I IV V IV"),
            new Progression(ProgressionStyle.Rock, true, "I bVII IV I"),
            new Progression(ProgressionStyle.Jazz, true, "ii7 V7 Imaj7"),
            new Progression(ProgressionStyle.Jazz, true, "Imaj7 vi7 ii7 V7"),
            new Progression(ProgressionStyle.Blues, true, "I7 I7 I7 I7 IV7 IV7 I7 I7 V7 IV7 I7 V7"),
            new Progression(ProgressionStyle.Classical, true, "I V vi iii IV I IV V"),
            new Progression(ProgressionStyle.Classical, true, "I IV V I"),
            new Progression(ProgressionStyle.Pop, false, "i VI III VII"),
            new Progression(ProgressionStyle.Rock, false, "i VII VI VII"),
            new Progression(ProgressionStyle.Classical, false, "i iv V i"),
            new Progression(ProgressionStyle.Flamenco, false, "i VII VI V"),
            new Progression(ProgressionStyle.Jazz, false, "iiø7 V7 i7"),
            new Progression(ProgressionStyle.Blues, false, "i7 i7 i7 i7 iv7 iv7 i7 i7 VI7 V7 i7 V7")
        };

        /// <summary>The progression's chords in a key.</summary>
        public DegreeChord[] In(Key key, bool sevenths)
        {
            return Numerals.Select(i => Resolve(key, i, sevenths)).ToArray();
        }

        private static readonly string[] Romans = { "VII", "III", "VI", "IV", "II", "V", "I" };

        /// <summary>The chord a numeral stands for in a key.</summary>
        public static DegreeChord Resolve(Key key, string numeral, bool sevenths)
        {
            var rest = numeral;
            var alteration = 0;

            while (rest.Length > 0 && (rest[0] == 'b' || rest[0] == '♭' || rest[0] == '#' || rest[0] == '♯'))
            {
                alteration += rest[0] == 'b' || rest[0] == '♭' ? -1 : 1;
                rest = rest.Substring(1);
            }
            var roman = Romans.First(i => rest.StartsWith(i, StringComparison.OrdinalIgnoreCase));
            var degree = Array.IndexOf(new[] { "I", "II", "III", "IV", "V", "VI", "VII" }, roman) + 1;
            var minor = char.IsLower(rest[0]);
            var suffix = rest.Substring(roman.Length);
            var diatonic = key.DiatonicChord(degree, false);
            var diatonicMinor = diatonic.Numeral.Length > 0 && char.IsLower(diatonic.Numeral[0]);

            if (alteration == 0 && suffix.Length == 0 && minor == diatonicMinor)
            {
                return key.DiatonicChord(degree, sevenths);
            }
            ChordType type;

            switch (suffix)
            {
                case "7": type = minor ? ChordType.MinorSeventh : ChordType.DominantSeventh; break;
                case "maj7": type = minor ? ChordType.MinorMajorSeventh : ChordType.MajorSeventh; break;
                case "ø7": type = ChordType.MinorSeventhFlat5; break;
                case "°7": type = ChordType.DiminishedSeventh; break;
                case "°": type = sevenths ? ChordType.MinorSeventhFlat5 : ChordType.Diminished; break;
                default:
                    type = minor
                        ? (sevenths ? ChordType.MinorSeventh : ChordType.Minor)
                        : (sevenths ? (degree == 5 ? ChordType.DominantSeventh : ChordType.MajorSeventh) : ChordType.Major);
                    break;
            }

            return key.ChordOn(degree, alteration, type, sevenths);
        }

        /// <summary>Where each degree of a major key usually moves (functional harmony), with weights.</summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> MajorMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (4, 4), (5, 4), (6, 4), (2, 2), (3, 1) } },
            { 2, new[] { (5, 5), (4, 1), (7, 1) } },
            { 3, new[] { (6, 4), (4, 3) } },
            { 4, new[] { (5, 4), (1, 3), (2, 2), (6, 1) } },
            { 5, new[] { (1, 5), (6, 3), (4, 1) } },
            { 6, new[] { (4, 4), (2, 3), (5, 2), (3, 1) } },
            { 7, new[] { (1, 4), (3, 1) } }
        };

        /// <summary>The same for a minor key; the dominant (5) is major as in harmonic minor.</summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> MinorMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (4, 4), (6, 4), (7, 3), (3, 2), (5, 2) } },
            { 2, new[] { (5, 5) } },
            { 3, new[] { (6, 4), (7, 2), (4, 2) } },
            { 4, new[] { (5, 4), (1, 3), (7, 2) } },
            { 5, new[] { (1, 5), (6, 2) } },
            { 6, new[] { (7, 4), (3, 3), (4, 2), (2, 1) } },
            { 7, new[] { (3, 4), (1, 3), (6, 1) } }
        };

        /// <summary>
        /// A new progression in a key: starts on the tonic, moves the way harmony usually does
        /// and ends on a chord that leads back to the tonic, so it can be looped.
        /// </summary>
        public static DegreeChord[] Generate(Key key, int length, bool sevenths, Random random)
        {
            var moves = key.IsMajor ? MajorMoves : MinorMoves;

            for (var attempt = 0; ; attempt++)
            {
                var degrees = new List<int> { 1 };

                while (degrees.Count < length)
                {
                    var options = moves[degrees[degrees.Count - 1]]
                        .Where(i => i.Degree != 1 || degrees.Count < length - 1)
                        .ToArray();
                    var pick = random.Next(options.Sum(i => i.Weight));
                    var next = options.SkipWhile(i => (pick -= i.Weight) >= 0).First();

                    degrees.Add(next.Degree);
                }
                // The last chord must lead back to the tonic and the chords must not all repeat one another.
                if (attempt < 100 &&
                    (!moves[degrees[degrees.Count - 1]].Any(i => i.Degree == 1) || degrees.Distinct().Count() < Math.Min(3, length)))
                {
                    continue;
                }

                return degrees.Select(degree =>
                {
                    if (!key.IsMajor && degree == 5)
                    {
                        return Resolve(key, sevenths ? "V7" : "V", sevenths);
                    }
                    return key.DiatonicChord(degree, sevenths);
                }).ToArray();
            }
        }
    }
}
