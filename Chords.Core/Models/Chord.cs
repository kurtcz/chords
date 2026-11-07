using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Extensions;
using Newtonsoft.Json;

namespace Chords.Core.Models
{
    public class Chord
    {
        public Note Root { get; private set; }
        public ChordType ChordType { get; private set; }
        /// <summary>
        /// Note to sound lowest when it is not the root – a slash chord such as C/E (an inversion) or D/C
        /// (a bass outside the chord); null for a chord in root position.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Note Bass { get; private set; }
        /// <summary>Whether the bass is one of the chord's own notes (an inversion) rather than a note added below it.</summary>
        [JsonIgnore]
        public bool BassInChord => Bass != null && NormalizedNotes.Contains(Note.Normalize(Bass));
        [JsonIgnore]
        public Interval[] Intervals { get; private set; }
        [JsonIgnore]
        public Note[] Notes { get; private set; }
        [JsonIgnore]
        public Note[] NormalizedNotes { get; private set; }
        [JsonIgnore]
        public Note[] NonMandatoryNotes { get; private set; }

        private Chord()
        {
        }

        /// <param name="bass">Note to sound lowest; null or the root itself for a chord in root position.</param>
        [JsonConstructor]
        public Chord(Note root, ChordType chordType, Note bass = null)
        {
            Root = root;
            ChordType = chordType;
            Bass = bass == null || Note.Normalize(bass) == Note.Normalize(root) ? null : bass;
            Intervals = ChordType.ToIntervals();
            Notes = Intervals.Select(i => Root.NoteAtInterval(i))
                             .ToArray();
            NormalizedNotes = Notes.Select(i => Note.Normalize(i))
                                   .ToArray();
            NonMandatoryNotes = ChordType.ToNonMandatoryIntervals()
                                         .Select(i => Root.NoteAtInterval(i))
                                         .ToArray();
        }

        /// <summary>The note that should sound lowest: the bass of a slash chord, otherwise the root.</summary>
        [JsonIgnore]
        public Note LowestNote => Bass ?? Root;

        /// <summary>
        /// Notes from the lowest one: the bass first, then the chord's other notes in order
        /// (E G C for C/E, C D F♯ A for D/C).
        /// </summary>
        [JsonIgnore]
        public Note[] NotesFromBass
        {
            get
            {
                if (Bass == null)
                {
                    return Notes;
                }
                var bass = Note.Normalize(Bass);
                var index = Array.IndexOf(NormalizedNotes, bass);

                return index < 0
                    ? new[] { Bass }.Concat(Notes).ToArray()
                    : Notes.Skip(index).Concat(Notes.Take(index)).ToArray();
            }
        }

        /// <summary>The same chord with another bass (null for root position).</summary>
        public Chord WithBass(Note bass) => new Chord(Root, ChordType, bass);

        public static Chord[] Find(Note[] notes, bool strict)
        {
            var results = new List<Chord>();

            foreach (var note in notes)
            {
                foreach (var chordType in Enum.GetValues(typeof(ChordType)).Cast<ChordType>())
                {
                    try
                    {
                        var chord = new Chord(note, chordType);

                        if (chord.Notes.Length == notes.Length &&
                            chord.Notes.All(i => notes.Any(j => i == j ||
                                                                (!strict && Note.Normalize(i) == Note.Normalize(j)))))
                        {
                            results.Add(chord);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Cannot create chord {note} {chordType}");
                    }
                }
            }

            return results.OrderBy(i => i.ChordType)
                          .ToArray();
		}

        /// <summary>
        /// Names the chord sounded by notes played on an instrument (octaves and spelling are ignored).
        /// A chord matches when every played note belongs to it and the only chord notes not played
        /// are those that may be omitted (e.g. the fifth of a seventh chord).
        /// </summary>
        /// <param name="notes">Notes sounding on the strings.</param>
        /// <param name="bass">Note of the lowest sounding string, preferred as the root.</param>
        /// <returns>Matches ordered from the most likely: chords rooted in the bass first, then
        /// complete chords. Rootless voicings are returned only when nothing else matches.</returns>
        public static Chord[] FindByNotes(Note[] notes, Note bass)
        {
            var played = notes.Select(Note.Normalize)
                              .Distinct()
                              .ToArray();
            var normalizedBass = bass == null ? null : Note.Normalize(bass);
            var results = new List<(Chord Chord, int Missing, bool Rooted)>();

            foreach (var root in Note.AllNotesNormalized)
            {
                foreach (var chordType in Enum.GetValues(typeof(ChordType)).Cast<ChordType>())
                {
                    Chord chord;

                    try
                    {
                        chord = new Chord(PreferredSpelling(root), chordType);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    var chordNotes = chord.NormalizedNotes.Distinct().ToArray();
                    var optional = chord.NonMandatoryNotes.Select(Note.Normalize).ToArray();
                    var missing = chordNotes.Where(i => !played.Contains(i)).ToArray();

                    if (played.All(i => chordNotes.Contains(i)) &&
                        missing.All(i => optional.Contains(i)))
                    {
                        results.Add((chord, missing.Length, played.Contains(root)));
                    }
                }
            }
            if (results.Any(i => i.Rooted))
            {
                results.RemoveAll(i => !i.Rooted);
            }

            return results.OrderBy(i => Note.Normalize(i.Chord.Root) == normalizedBass ? 0 : 1)
                          .ThenBy(i => i.Missing)
                          .ThenBy(i => i.Chord.ChordType)
                          .Select(i => i.Chord)
                          .ToArray();
        }

        /// <summary>Usual spelling of a root: C#, Eb, F#, Ab, Bb.</summary>
        private static Note PreferredSpelling(Note normalized)
        {
            if (normalized.Accidental != Accidental.Sharp)
            {
                return normalized;
            }
            switch (normalized.Tone)
            {
                case Tone.D: return new Note(Tone.E, Accidental.Flat);
                case Tone.G: return new Note(Tone.A, Accidental.Flat);
                case Tone.A: return new Note(Tone.B, Accidental.Flat);
                default: return normalized;
            }
        }

		/// <summary>Parses a chord symbol such as Cmaj7, F#m or a slash chord D/F# (a 6/9 chord is not taken for one).</summary>
		public static bool TryParse(string str, NamingConvention conv, out Chord chord)
		{
            if (TryParseSymbol(str, conv, out chord))
            {
                return true;
            }
            var slash = str?.LastIndexOf('/') ?? -1;

            if (slash > 0 &&
                TryParseSymbol(str.Substring(0, slash), conv, out var upper) &&
                Note.TryParse(str.Substring(slash + 1), conv, out var bass))
            {
                chord = upper.WithBass(bass);
                return true;
            }
            chord = null;

            return false;
        }

		private static bool TryParseSymbol(string str, NamingConvention conv, out Chord chord)
		{
            chord = null;
            if (string.IsNullOrWhiteSpace(str) || !Enum.IsDefined(typeof(NamingConvention), conv))
			{
                return false;
			}

            Note root = null;
			var chordTypeOffset = 0;

            foreach (var n in Note.AllNotes.OrderByDescending(i => i.ToString()))
            {
                var notestr = n.ToString(conv);

                if (notestr.Length <= str.Length &&
                    notestr.Equals(str.Substring(0, notestr.Length), StringComparison.OrdinalIgnoreCase))
                {
                    root = n;
                    chordTypeOffset = notestr.Length;
                    break;
                }
            }
            if (root == null)
			{
                return false;
			}
            foreach (var chordType in Enum.GetValues(typeof(ChordType)).Cast<ChordType>())
			{
				var descriptions = chordType.GetDescriptions();

                if (descriptions.Any(i => i.Equals(str.Substring(chordTypeOffset), StringComparison.OrdinalIgnoreCase)) ||
                    chordType.ToString().Equals(str.Substring(chordTypeOffset), StringComparison.OrdinalIgnoreCase))
				{
                    chord = new Chord(root, chordType);

                    return true;
				}
			}

            return false;
		}

        /// <summary>
        /// The same chord a number of half-steps higher (or lower, for a negative number), its root spelled the way
        /// chord charts write it: B♭, E♭ and A♭ as flats, C♯ and F♯ as sharps.
        /// </summary>
        /// <remarks>
        /// With a capo on fret n the shape to hold for a sounding chord is <c>Transpose(-n)</c>: D with the capo on 2 is a C shape.
        /// </remarks>
        public Chord Transpose(int halftones)
        {
            return new Chord(TransposeNote(Root, halftones), ChordType, Bass == null ? null : TransposeNote(Bass, halftones));
        }

        private static Note TransposeNote(Note note, int halftones)
        {
            var result = Note.FromPitch(Note.AllNotesNormalized[0].ChromaticDistance(note) + halftones);

            if (result.Accidental == Accidental.Sharp && result.Tone != Tone.C && result.Tone != Tone.F)
            {
                result = Note.FromPitch(Note.AllNotesNormalized[0].ChromaticDistance(result), true);
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Chord;

            if(other == null)
            {
                return false;
            }

            return this.Root == other.Root && this.ChordType == other.ChordType && this.Bass == other.Bass;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = Root.GetHashCode();

                result = (result * 397) ^ ChordType.GetHashCode();
                result = (result * 397) ^ (Bass?.GetHashCode() ?? 0);

                return result;
            }
        }

        public override string ToString()
        {
            return Bass == null
                ? string.Format("[Chord: Root={0}, ChordType={1}]", Root, ChordType)
                : string.Format("[Chord: Root={0}, ChordType={1}, Bass={2}]", Root, ChordType, Bass);
        }
    }
}
