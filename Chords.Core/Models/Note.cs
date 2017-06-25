using System;
using System.Linq;
using Chords.Core.Extensions;

namespace Chords.Core.Models
{
	public class Note
    {
        const int NumberOfTones = 7;
        const int NumberOfHalfTones = 12;

        private static readonly Tone[] Tones = Enum.GetValues(typeof(Tone))
                                                   .Cast<Tone>()
                                                   .ToArray();
        public static readonly Note[] AllNotes = Enum.GetValues(typeof(Tone))
		                                             .Cast<Tone>()
		                                             .SelectMany(t => Enum.GetValues(typeof(Accidental))
		                                                                  .Cast<Accidental>()
		                                                                  .Select(a => new Note(t, a)))
		                                             .ToArray();
        public static readonly Note[] AllNotesNormalized =
        {
            new Note(Tone.C),
            new Note(Tone.C, Accidental.Sharp),
			new Note(Tone.D),
			new Note(Tone.D, Accidental.Sharp),
			new Note(Tone.E),
			new Note(Tone.F),
			new Note(Tone.F, Accidental.Sharp),
			new Note(Tone.G),
			new Note(Tone.G, Accidental.Sharp),
			new Note(Tone.A),
			new Note(Tone.A, Accidental.Sharp),
			new Note(Tone.B)
		};
        public Tone Tone { get; }
        public Accidental Accidental { get; }

        public Note(Tone tone)
            : this (tone, Accidental.Natural)
        {
        }

        public Note(Tone tone, Accidental accidental)
        {
            Tone = tone;
            Accidental = accidental;
        }

        public int ChromaticDistance(Note other)
        {
			var distance = other.Tone - Tone;

            switch (Accidental)
            {
                case Accidental.Sharp:
                    distance--; break;
				case Accidental.Flat:
					distance++; break;
				case Accidental.DoubleSharp:
					distance -= 2; break;
                case Accidental.DoubleFlat:
					distance += 2; break;
			}
			switch (other.Accidental)
			{
				case Accidental.Sharp:
					distance++; break;
				case Accidental.Flat:
					distance--; break;
				case Accidental.DoubleSharp:
					distance += 2; break;
				case Accidental.DoubleFlat:
					distance -= 2; break;
			}
            distance = (distance + NumberOfHalfTones) % NumberOfHalfTones;

            return distance;
		}

        public static Note Normalize(Note note)
        {
            if (note == null)
            {
                throw new ArgumentNullException();
            }
            switch(note.Accidental)
            {
                case Accidental.Natural:
                    return note;
                case Accidental.Sharp:
                    if (note.Tone == Tone.E || note.Tone == Tone.B)
                    {
                        return new Note(Tones[(Array.IndexOf(Tones, note.Tone) + 1) % NumberOfTones], Accidental.Natural);
					}
                    return note;
                case Accidental.Flat:
                    if (note.Tone == Tone.F || note.Tone == Tone.C)
                    {
                        return new Note(Tones[(Array.IndexOf(Tones, note.Tone) - 1) % NumberOfTones], Accidental.Natural);
                    }
                    return new Note(Tones[(Array.IndexOf(Tones, note.Tone) - 1) % NumberOfTones], Accidental.Sharp);
                case Accidental.DoubleSharp:
                    return new Note(Tones[(Array.IndexOf(Tones, note.Tone) + 1) % NumberOfTones], Accidental.Natural);
				case Accidental.DoubleFlat:
					return new Note(Tones[(Array.IndexOf(Tones, note.Tone) - 1) % NumberOfTones], Accidental.Natural);
			}
            throw new ArgumentException();
        }

        public Note NoteAtChromaticDistance(int halftones)
        {
            if (halftones < 0)
            {
                throw new ArgumentException();
            }
            var self = Normalize(this);
                           
            return AllNotesNormalized[(Array.IndexOf(AllNotesNormalized, self) + halftones) % NumberOfHalfTones];
        }

		public Note NoteAtInterval(Interval interval)
		{
            var halftones = interval.ToHalftones();
			var steps = interval.ToSteps();
			var normalizedNote = NoteAtChromaticDistance(halftones);
			var tone = Tones[(Array.IndexOf(Tones, Tone) + steps) % NumberOfTones];

            if (normalizedNote.Tone == tone)
            {
                return normalizedNote;
            }

			var note = new Note(tone);
            int correction = tone > normalizedNote.Tone
                              ? -normalizedNote.ChromaticDistance(note)
                              : -note.ChromaticDistance(normalizedNote);

            return new Note(tone, (Accidental)correction);
		}

        public static bool TryParse(string str, NamingConvention conv, out Note note)
        {
			note = null;
            if (string.IsNullOrWhiteSpace(str) || !Enum.IsDefined(typeof(NamingConvention), conv))
            {
				return false;
            }

            if (TryParseExceptionalStrings(str, conv, out note))
            {
                return true;
            }

            Tone tone = 0;
			var accidentalOffset = 0;

			foreach(var t in Enum.GetValues(typeof(Tone)).Cast<Tone>())
            {
                var description = t.GetDescriptions().ToArray()[(int)conv];

                if (description.Length <= str.Length &&
                    description.Equals(str.Substring(0, description.Length), StringComparison.OrdinalIgnoreCase))
                {
                    accidentalOffset = description.Length;
                    tone = t;
                    break;
                }
            }
            if (tone == 0)
            {
                return false;
            }
            foreach(var accidental in Enum.GetValues(typeof(Accidental)).Cast<Accidental>())
            {
                var description = accidental.ToDescription() ?? string.Empty;

                if (description.Length == str.Length - accidentalOffset &&
                    description == str.Substring(accidentalOffset))
                {
                    note = new Note(tone, accidental);

					return true;
				}                    
            }

            return false;
        }

        private static bool TryParseExceptionalStrings(string str, NamingConvention conv, out Note note)
        {
            note = null;

            if (conv == NamingConvention.German && str.ToUpper() == "B")
            {
                note = new Note(Tone.B, Accidental.Flat);

                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Note;

            if(other == null)
            {
                return false;
            }

            return this.ChromaticDistance(other) == 0 && this.Tone == other.Tone;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = Tone.GetHashCode();

                result = (result * 397) ^ Accidental.GetHashCode();

                return result;
            }
        }

        public static bool operator ==(Note a, Note b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

		public static bool operator !=(Note a, Note b)
		{
            return !(a == b);
		}

		public string ToString(NamingConvention conv)
        {
            if (Tone == Tone.B &&
                Accidental == Accidental.Flat &&
                conv == NamingConvention.German)
            {
                return "B";
            }

            return string.Format("{0}{1}", Tone.GetDescriptions().ToArray()[(int)conv], Accidental.ToDescription() ?? string.Empty);
        }

        public override string ToString()
		{
            return ToString(NamingConvention.English);
		}
	}
}
