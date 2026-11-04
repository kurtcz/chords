using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Extensions;

namespace Chords.Core.Models
{
    /// <summary>A chord built on one degree of a key, with its Roman numeral (I, ii, vii°, V7, iiø7…).</summary>
    public class DegreeChord
    {
        /// <summary>Scale degree 1–7.</summary>
        public int Degree { get; }
        public Chord Chord { get; }
        public string Numeral { get; }

        public DegreeChord(int degree, Chord chord, string numeral)
        {
            Degree = degree;
            Chord = chord;
            Numeral = numeral;
        }
    }

    /// <summary>A key (tonic and scale) and the chords that belong to it.</summary>
    public class Key
    {
        private static readonly string[] Numerals = { "I", "II", "III", "IV", "V", "VI", "VII" };

        /// <summary>Scales a chord progression can be built on (heptatonic).</summary>
        public static readonly ScaleType[] ScaleTypes =
        {
            ScaleType.Major, ScaleType.Minor, ScaleType.HarmonicMinor, ScaleType.MelodicMinor,
            ScaleType.Dorian, ScaleType.Phrygian, ScaleType.Lydian, ScaleType.Mixolydian
        };

        /// <summary>Usual spelling of the tonic of each major key (index = semitones above C).</summary>
        private static readonly Note[] MajorTonics =
        {
            new Note(Tone.C), new Note(Tone.D, Accidental.Flat), new Note(Tone.D), new Note(Tone.E, Accidental.Flat),
            new Note(Tone.E), new Note(Tone.F), new Note(Tone.F, Accidental.Sharp), new Note(Tone.G),
            new Note(Tone.A, Accidental.Flat), new Note(Tone.A), new Note(Tone.B, Accidental.Flat), new Note(Tone.B)
        };

        /// <summary>Usual spelling of the tonic of each minor key (index = semitones above C).</summary>
        private static readonly Note[] MinorTonics =
        {
            new Note(Tone.C), new Note(Tone.C, Accidental.Sharp), new Note(Tone.D), new Note(Tone.E, Accidental.Flat),
            new Note(Tone.E), new Note(Tone.F), new Note(Tone.F, Accidental.Sharp), new Note(Tone.G),
            new Note(Tone.G, Accidental.Sharp), new Note(Tone.A), new Note(Tone.B, Accidental.Flat), new Note(Tone.B)
        };

        public Note Tonic { get; }
        public ScaleType ScaleType { get; }
        public Scale Scale { get; }

        /// <summary>Major, Lydian and Mixolydian have a major tonic chord.</summary>
        public bool IsMajor => Scale.Notes[0].ChromaticDistance(Scale.Notes[2]) == 4;

        public Key(Note tonic, ScaleType scaleType)
        {
            if (scaleType == ScaleType.Chromatic)
            {
                throw new ArgumentException("A key needs a seven-note scale.", nameof(scaleType));
            }
            Tonic = tonic;
            ScaleType = scaleType;
            Scale = new Scale(tonic, scaleType);
        }

        /// <summary>The key with the usual spelling of its tonic (Db major rather than C# major).</summary>
        public static Key Usual(Note tonic, bool major)
        {
            var index = new Note(Tone.C).ChromaticDistance(tonic);

            return new Key(major ? MajorTonics[index] : MinorTonics[index], major ? ScaleType.Major : ScaleType.Minor);
        }

        /// <summary>
        /// Degree (0-based) of the parent major scale the tonic lies on: D Dorian is the second degree of C major.
        /// Harmonic and melodic minor are counted as their natural minor.
        /// </summary>
        private static int ModeOffset(ScaleType scaleType)
        {
            switch (scaleType)
            {
                case ScaleType.Dorian: return 1;
                case ScaleType.Phrygian: return 2;
                case ScaleType.Lydian: return 3;
                case ScaleType.Mixolydian: return 4;
                case ScaleType.Minor:
                case ScaleType.HarmonicMinor:
                case ScaleType.MelodicMinor: return 5;
                default: return 0;
            }
        }

        /// <summary>Tonic of the major key with the same key signature (C for A minor or D Dorian).</summary>
        public Note ParentMajor => Scale.Notes[(7 - ModeOffset(ScaleType)) % 7];

        /// <summary>The key of the same scale type whose parent major is <paramref name="majorTonic"/>.</summary>
        public Key WithParentMajor(Note majorTonic) =>
            new Key(new Scale(majorTonic, ScaleType.Major).Notes[ModeOffset(ScaleType)], ScaleType);

        /// <summary>Number of sharps or flats in the scale.</summary>
        public int Accidentals => Scale.Notes.Sum(i => Math.Abs((int)i.Accidental));

        /// <summary>Note on a degree (1–7) of the scale.</summary>
        public Note NoteOf(int degree) => Scale.Notes[(degree - 1 + 7 * 10) % 7];

        /// <summary>Diatonic triads (or seventh chords) on degrees I–VII, stacked in thirds from the scale.</summary>
        public DegreeChord[] DiatonicChords(bool sevenths)
        {
            return Enumerable.Range(1, 7)
                             .Select(degree => DiatonicChord(degree, sevenths))
                             .ToArray();
        }

        /// <summary>The chord stacked in thirds on a degree of the scale.</summary>
        public DegreeChord DiatonicChord(int degree, bool sevenths)
        {
            var root = NoteOf(degree);
            var third = root.ChromaticDistance(NoteOf(degree + 2));
            var fifth = root.ChromaticDistance(NoteOf(degree + 4));
            var seventh = sevenths ? root.ChromaticDistance(NoteOf(degree + 6)) : (int?)null;
            var type = ChordTypeOf(third, fifth, seventh);

            return new DegreeChord(degree, new Chord(root, type), Numeral(degree, 0, type));
        }

        /// <summary>
        /// A chord on a degree, optionally chromatically altered (e.g. ♭VII in a major key)
        /// and of a given type (e.g. a dominant seventh on every degree of a blues).
        /// </summary>
        public DegreeChord ChordOn(int degree, int alteration, ChordType? type, bool sevenths)
        {
            if (alteration == 0 && type == null)
            {
                return DiatonicChord(degree, sevenths);
            }
            var note = NoteOf(degree);
            var root = new Note(note.Tone, (Accidental)Math.Max(-2, Math.Min(2, (int)note.Accidental + alteration)));
            var chordType = type ?? DiatonicChord(degree, sevenths).Chord.ChordType;

            return new DegreeChord(degree, new Chord(root, chordType), Numeral(degree, alteration, chordType));
        }

        private static ChordType ChordTypeOf(int third, int fifth, int? seventh)
        {
            switch ((third, fifth, seventh))
            {
                case (4, 7, null): return ChordType.Major;
                case (3, 7, null): return ChordType.Minor;
                case (3, 6, null): return ChordType.Diminished;
                case (4, 8, null): return ChordType.Augmented;
                case (4, 7, 11): return ChordType.MajorSeventh;
                case (4, 7, 10): return ChordType.DominantSeventh;
                case (3, 7, 10): return ChordType.MinorSeventh;
                case (3, 7, 11): return ChordType.MinorMajorSeventh;
                case (3, 6, 10): return ChordType.MinorSeventhFlat5;
                case (3, 6, 9): return ChordType.DiminishedSeventh;
                case (4, 8, 10): return ChordType.AugmentedSeventh;
                // An augmented major seventh chord has no symbol of its own; the triad is shown.
                case (4, 8, _): return ChordType.Augmented;
                default: return third == 3 ? ChordType.Minor : ChordType.Major;
            }
        }

        /// <summary>Roman numeral: upper case for major-third chords, lower case for minor ones.</summary>
        public static string Numeral(int degree, int alteration, ChordType type)
        {
            var intervals = type.ToIntervals();
            var minor = intervals.Contains(Interval.m3) && !intervals.Contains(Interval.M3);
            var numeral = Numerals[(degree - 1) % 7];
            var prefix = alteration < 0 ? "♭" : alteration > 0 ? "♯" : string.Empty;

            if (minor)
            {
                numeral = numeral.ToLowerInvariant();
            }
            string suffix;

            switch (type)
            {
                case ChordType.Major:
                case ChordType.Minor: suffix = string.Empty; break;
                case ChordType.Diminished: suffix = "°"; break;
                case ChordType.Augmented: suffix = "+"; break;
                case ChordType.DiminishedSeventh: suffix = "°7"; break;
                case ChordType.MinorSeventhFlat5: suffix = "ø7"; break;
                case ChordType.MajorSeventh:
                case ChordType.MinorMajorSeventh: suffix = "maj7"; break;
                case ChordType.DominantSeventh:
                case ChordType.MinorSeventh: suffix = "7"; break;
                case ChordType.AugmentedSeventh: suffix = "+7"; break;
                default:
                    suffix = type.ToDescription() ?? string.Empty;
                    if (minor && suffix.StartsWith("mi"))
                    {
                        suffix = suffix.Substring(2);
                    }
                    else if (minor && suffix.StartsWith("m") && !suffix.StartsWith("maj"))
                    {
                        suffix = suffix.Substring(1);
                    }
                    break;
            }

            return prefix + numeral + suffix;
        }

        /// <summary>Whether every note of the chord belongs to the scale (enharmonics are equal).</summary>
        public bool Contains(Chord chord)
        {
            var scale = Scale.Notes.Select(Note.Normalize).ToArray();

            return chord.NormalizedNotes.All(i => scale.Contains(i));
        }

        public override bool Equals(object obj) => obj is Key other && other.Tonic == Tonic && other.ScaleType == ScaleType;

        public override int GetHashCode() => Tonic.GetHashCode() * 397 ^ ScaleType.GetHashCode();

        public override string ToString() => $"[Key: {Tonic} {ScaleType}]";
    }

    /// <summary>A key a set of chords fits into.</summary>
    public class KeyMatch
    {
        public Key Key { get; }
        /// <summary>Chords whose notes all belong to the key (in a minor key also its harmonic form).</summary>
        public int Fitting { get; }
        public int Total { get; }
        /// <summary>Roman numeral of each chord in the key, null for a chord outside it.</summary>
        public string[] Numerals { get; }

        public KeyMatch(Key key, int fitting, int total, string[] numerals)
        {
            Key = key;
            Fitting = fitting;
            Total = total;
            Numerals = numerals;
        }

        /// <summary>
        /// Major and minor keys the chords fit best, from the most likely: the most fitting chords,
        /// then a song starting or ending on the tonic chord, then the simplest key signature.
        /// A minor key also accepts the chords of its harmonic form (major V, diminished vii°).
        /// </summary>
        public static KeyMatch[] Find(IReadOnlyList<Chord> chords)
        {
            if (chords.Count == 0)
            {
                return new KeyMatch[0];
            }
            var results = new List<(KeyMatch Match, int TonicBonus)>();

            foreach (var root in Note.AllNotesNormalized)
            {
                foreach (var major in new[] { true, false })
                {
                    var key = Key.Usual(root, major);
                    var harmonic = major ? null : new Key(key.Tonic, ScaleType.HarmonicMinor);
                    var numerals = chords.Select(i => NumeralIn(key, i) ?? (harmonic == null ? null : NumeralIn(harmonic, i)))
                                         .ToArray();
                    var fitting = numerals.Count(i => i != null);

                    if (fitting == 0)
                    {
                        continue;
                    }
                    var tonicChord = key.DiatonicChord(1, false).Chord;
                    var bonus = new[] { chords[0], chords[chords.Count - 1] }
                        .Count(i => Note.Normalize(i.Root) == Note.Normalize(key.Tonic) && IsMinor(i) == IsMinor(tonicChord));

                    results.Add((new KeyMatch(key, fitting, chords.Count, numerals), bonus));
                }
            }

            return results.OrderByDescending(i => i.Match.Fitting)
                          .ThenByDescending(i => i.TonicBonus)
                          .ThenBy(i => i.Match.Key.Accidentals)
                          .ThenBy(i => i.Match.Key.IsMajor ? 0 : 1)
                          .Select(i => i.Match)
                          .ToArray();
        }

        private static bool IsMinor(Chord chord) => chord.Intervals.Contains(Interval.m3) && !chord.Intervals.Contains(Interval.M3);

        /// <summary>Roman numeral of a chord in the key, or null when some of its notes are outside the scale.</summary>
        private static string NumeralIn(Key key, Chord chord)
        {
            if (!key.Contains(chord))
            {
                return null;
            }
            var root = Note.Normalize(chord.Root);
            var degree = Array.FindIndex(key.Scale.Notes, i => Note.Normalize(i) == root) + 1;

            return Key.Numeral(degree, 0, chord.ChordType);
        }
    }
}
