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
        Flamenco,
        Klezmer
    }

    /// <summary>
    /// A chord progression written in Roman numerals, playable in any key.
    /// </summary>
    /// <remarks>
    /// A numeral is the diatonic chord of its degree unless it says otherwise: a ♭/♯ prefix moves the root,
    /// upper/lower case asks for a major/minor chord and a suffix sets the type
    /// (7 = dominant or minor seventh, maj7, ø7 = half-diminished, ° = diminished).
    /// A slash and an Arabic scale degree put another note in the bass, the way a walking bass line is written:
    /// V/7 is the dominant over the seventh degree (G/B in C major), vi/5 the sixth chord over the fifth (Am/G).
    /// The degree may be altered with ♭/♯ like the numeral.
    /// </remarks>
    public class Progression
    {
        public string[] Numerals { get; }
        public ProgressionStyle Style { get; }
        /// <summary>Written for a major key; otherwise for a minor one.</summary>
        public bool ForMajor { get; }
        /// <summary>The only scale the progression is written for, or null for any major or minor key.</summary>
        public ScaleType? ForScale { get; }

        public Progression(ProgressionStyle style, bool forMajor, string numerals)
        {
            Style = style;
            ForMajor = forMajor;
            Numerals = numerals.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public Progression(ProgressionStyle style, ScaleType forScale, string numerals)
            : this(style, new Key(new Note(Tone.C), forScale).IsMajor, numerals)
        {
            ForScale = forScale;
        }

        /// <summary>
        /// Whether the progression suits a key: one written for a scale only that scale, the others any key
        /// with the same kind of tonic chord. The non-diatonic scales (Phrygian dominant, Hungarian minor…)
        /// get only their own progressions, as their diminished and augmented chords make
        /// ordinary major- and minor-key progressions sound wrong.
        /// </summary>
        public bool Fits(Key key)
        {
            if (ForScale != null)
            {
                return key.ScaleType == ForScale;
            }
            return !ScaleMoves.ContainsKey(key.ScaleType) && ForMajor == key.IsMajor;
        }

        /// <summary>Well-known progressions.</summary>
        public static readonly Progression[] Common =
        {
            new Progression(ProgressionStyle.Pop, true, "I V vi IV"),
            new Progression(ProgressionStyle.Pop, true, "I vi IV V"),
            new Progression(ProgressionStyle.Pop, true, "vi IV I V"),
            new Progression(ProgressionStyle.Pop, true, "IV V iii vi"),
            new Progression(ProgressionStyle.Pop, true, "I V/7 vi IV"),
            new Progression(ProgressionStyle.Pop, true, "I V/7 vi vi/5 IV I/3 ii V"),
            new Progression(ProgressionStyle.Rock, true, "I IV V IV"),
            new Progression(ProgressionStyle.Rock, true, "I bVII IV I"),
            new Progression(ProgressionStyle.Jazz, true, "ii7 V7 Imaj7"),
            new Progression(ProgressionStyle.Jazz, true, "Imaj7 vi7 ii7 V7"),
            new Progression(ProgressionStyle.Blues, true, "I7 I7 I7 I7 IV7 IV7 I7 I7 V7 IV7 I7 V7"),
            new Progression(ProgressionStyle.Classical, true, "I V vi iii IV I IV V"),
            new Progression(ProgressionStyle.Classical, true, "I IV V I"),
            new Progression(ProgressionStyle.Classical, true, "I V/7 vi iii/5 IV I/3 IV V"),
            new Progression(ProgressionStyle.Pop, false, "i VI III VII"),
            new Progression(ProgressionStyle.Pop, false, "i i/7 VI V"),
            new Progression(ProgressionStyle.Rock, false, "i VII VI VII"),
            new Progression(ProgressionStyle.Classical, false, "i iv V i"),
            new Progression(ProgressionStyle.Flamenco, false, "i VII VI V"),
            new Progression(ProgressionStyle.Jazz, false, "iiø7 V7 i7"),
            new Progression(ProgressionStyle.Blues, false, "i7 i7 i7 i7 iv7 iv7 i7 i7 VI7 V7 i7 V7"),
            new Progression(ProgressionStyle.Klezmer, ScaleType.PhrygianDominant, "I I iv I"),
            new Progression(ProgressionStyle.Klezmer, ScaleType.PhrygianDominant, "I iv vii I"),
            new Progression(ProgressionStyle.Klezmer, ScaleType.PhrygianDominant, "I II I iv"),
            new Progression(ProgressionStyle.Flamenco, ScaleType.PhrygianDominant, "iv bIII II I"),
            new Progression(ProgressionStyle.Classical, ScaleType.HungarianMinor, "i V VI V"),
            new Progression(ProgressionStyle.Classical, ScaleType.HungarianMinor, "i VI V i"),
            new Progression(ProgressionStyle.Classical, ScaleType.HungarianMinor, "i II V i"),
            new Progression(ProgressionStyle.Rock, ScaleType.DoubleHarmonicMajor, "I II I II"),
            new Progression(ProgressionStyle.Flamenco, ScaleType.DoubleHarmonicMajor, "iv iii II I"),
            new Progression(ProgressionStyle.Klezmer, ScaleType.DoubleHarmonicMajor, "I iv II I"),
            new Progression(ProgressionStyle.Jazz, ScaleType.LydianDominant, "I7 II7 v7 I7"),
            new Progression(ProgressionStyle.Pop, ScaleType.LydianDominant, "I II vi II"),
            new Progression(ProgressionStyle.Klezmer, ScaleType.UkrainianDorian, "i II i II"),
            new Progression(ProgressionStyle.Klezmer, ScaleType.UkrainianDorian, "i II III II"),
            new Progression(ProgressionStyle.Klezmer, ScaleType.UkrainianDorian, "i v II i"),
            new Progression(ProgressionStyle.Jazz, ScaleType.Altered, "ii V VI i"),
            new Progression(ProgressionStyle.Jazz, ScaleType.Altered, "i ii iii ii")
        };

        /// <summary>The progression's chords in a key.</summary>
        public DegreeChord[] In(Key key, bool sevenths)
        {
            // The common progressions are written in major or natural minor; a chord that is not the mode's own
            // (IV in Lydian, VI7 of a minor blues in Dorian) keeps the root it has there.
            ScaleType? reference = ForScale != null ? (ScaleType?)null : ForMajor ? ScaleType.Major : ScaleType.Minor;

            return Numerals.Select(i => Resolve(key, i, sevenths, reference)).ToArray();
        }

        private static readonly string[] Romans = { "VII", "III", "VI", "IV", "II", "V", "I" };

        /// <summary>The chord a numeral stands for in a key.</summary>
        public static DegreeChord Resolve(Key key, string numeral, bool sevenths, ScaleType? reference = null)
        {
            var slash = numeral.IndexOf('/');

            if (slash > 0)
            {
                var upper = Resolve(key, numeral.Substring(0, slash), sevenths, reference);
                var bass = numeral.Substring(slash + 1);

                return new DegreeChord(upper.Degree, upper.Chord.WithBass(BassNote(key, bass, reference)), upper.Numeral + "/" + bass);
            }
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

            return key.ChordOn(degree, alteration, type, sevenths, reference);
        }

        /// <summary>The note on a scale degree written with an Arabic numeral, optionally altered (♭7).</summary>
        private static Note BassNote(Key key, string degree, ScaleType? reference)
        {
            var alteration = 0;
            var rest = degree;

            while (rest.Length > 0 && (rest[0] == 'b' || rest[0] == '♭' || rest[0] == '#' || rest[0] == '♯'))
            {
                alteration += rest[0] == 'b' || rest[0] == '♭' ? -1 : 1;
                rest = rest.Substring(1);
            }
            var number = int.Parse(rest);
            var note = reference != null ? new Scale(key.Tonic, reference.Value).Notes[(number - 1) % 7] : key.NoteOf(number);

            return alteration == 0
                ? note
                : new Note(note.Tone, (Accidental)Math.Max(-2, Math.Min(2, (int)note.Accidental + alteration)));
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
        /// The same for Phrygian dominant (Freygish), whose harmony circles the major tonic: I–iv, I–II and vii–I.
        /// Its diminished iii° and v° and augmented VI+ are left out.
        /// </summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> PhrygianDominantMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (4, 5), (2, 4), (7, 3) } },
            { 2, new[] { (1, 5), (4, 1), (7, 1) } },
            { 4, new[] { (1, 5), (7, 3), (2, 2) } },
            { 7, new[] { (1, 5), (4, 1), (2, 1) } }
        };

        /// <summary>
        /// Hungarian minor: i, V and VI, with II7♭5 (D F♯ A♭ in C) leading to the dominant;
        /// III+ and the chord on the raised fourth (F♯ A♭ C) are left out.
        /// </summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> HungarianMinorMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (5, 4), (6, 3), (2, 3), (7, 1) } },
            { 2, new[] { (5, 5), (1, 1) } },
            { 5, new[] { (1, 5), (6, 2) } },
            { 6, new[] { (5, 4), (2, 2) } },
            { 7, new[] { (1, 3), (6, 1) } }
        };

        /// <summary>Double harmonic major circles the tonic with ♭II, iii and iv; its ♭5 dominant, VI+ and vii are left out.</summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> DoubleHarmonicMajorMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (2, 5), (4, 3), (3, 1) } },
            { 2, new[] { (1, 5), (3, 2), (4, 1) } },
            { 3, new[] { (4, 3), (2, 3) } },
            { 4, new[] { (1, 4), (2, 3), (3, 1) } }
        };

        /// <summary>Lydian dominant swings between I and II (the raised fourth); its diminished chords and VII+ are left out.</summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> LydianDominantMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (2, 5), (5, 2), (6, 2) } },
            { 2, new[] { (1, 5), (5, 2), (6, 1) } },
            { 5, new[] { (1, 3), (2, 2), (6, 1) } },
            { 6, new[] { (2, 4), (5, 2), (1, 1) } }
        };

        /// <summary>Ukrainian Dorian (Misheberakh): i–II above all, with III and v; vi° and VII+ are left out.</summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> UkrainianDorianMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (2, 5), (5, 2), (3, 2) } },
            { 2, new[] { (1, 5), (3, 2), (5, 1) } },
            { 3, new[] { (2, 4), (1, 2) } },
            { 5, new[] { (1, 4), (2, 2) } }
        };

        /// <summary>
        /// Altered: the chords of the melodic minor a semitone up (ii, iii, V, VI) around the diminished tonic;
        /// IV+ and the other diminished chords are left out.
        /// </summary>
        private static readonly Dictionary<int, (int Degree, int Weight)[]> AlteredMoves = new Dictionary<int, (int, int)[]>
        {
            { 1, new[] { (2, 4), (5, 3), (6, 2) } },
            { 2, new[] { (1, 3), (3, 3), (5, 3) } },
            { 3, new[] { (2, 3), (5, 3) } },
            { 5, new[] { (6, 4), (2, 2), (1, 2) } },
            { 6, new[] { (1, 5), (2, 2) } }
        };

        /// <summary>Scales with harmony of their own; the others use the major or minor moves.</summary>
        private static readonly Dictionary<ScaleType, Dictionary<int, (int Degree, int Weight)[]>> ScaleMoves =
            new Dictionary<ScaleType, Dictionary<int, (int Degree, int Weight)[]>>
            {
                { ScaleType.PhrygianDominant, PhrygianDominantMoves },
                { ScaleType.HungarianMinor, HungarianMinorMoves },
                { ScaleType.DoubleHarmonicMajor, DoubleHarmonicMajorMoves },
                { ScaleType.LydianDominant, LydianDominantMoves },
                { ScaleType.UkrainianDorian, UkrainianDorianMoves },
                { ScaleType.Altered, AlteredMoves }
            };

        /// <summary>
        /// A new progression in a key: starts on the tonic, moves the way harmony usually does
        /// and ends on a chord that leads back to the tonic, so it can be looped.
        /// </summary>
        public static DegreeChord[] Generate(Key key, int length, bool sevenths, Random random)
        {
            var moves = ScaleMoves.TryGetValue(key.ScaleType, out var own) ? own
                      : key.IsMajor ? MajorMoves
                      : MinorMoves;

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
                    // Natural minor borrows the major dominant of harmonic minor; the modes keep their own fifth chord.
                    if (key.ScaleType == ScaleType.Minor && degree == 5)
                    {
                        return Resolve(key, sevenths ? "V7" : "V", sevenths);
                    }
                    return key.DiatonicChord(degree, sevenths);
                }).ToArray();
            }
        }
    }
}
