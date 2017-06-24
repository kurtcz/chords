using System;
using System.Linq;
using Chords.Extensions;

namespace Chords.Models
{
	public class Note
    {
        const int NumberOfTones = 7;
        const int NumberOfHalfTones = 12;
        public Tone[] Notes => Enum.GetValues(typeof(Tone)).Cast<Tone>().ToArray();
        public Tone Tone { get; set; }
        public Accidental Accidental { get; set; }

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

            return distance % NumberOfHalfTones;
		}

        public int Distance(Note other)
        {
            
            return (Array.IndexOf(Notes, other.Tone) - Array.IndexOf(Notes, Tone)) % NumberOfTones;
        }

        public static bool TryParse(string str, NamingConvention conv, out Note note)
        {
			note = null;
            if (string.IsNullOrWhiteSpace(str) || !Enum.IsDefined(typeof(NamingConvention), conv))
            {
				return false;
            }

            Tone tone = 0;
			var accidentalOffset = 0;

			foreach(var t in Enum.GetValues(typeof(Tone)).Cast<Tone>())
            {
                var description = t.GetDescriptions().ToArray()[(int)conv];

                if (description.Length <= str.Length &&
                    description == str.Substring(0, description.Length))
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
                var description = accidental.ToDescription();

                if (description.Length == str.Length - accidentalOffset &&
                    description == str.Substring(accidentalOffset))
                {
					note = new Note
					{
						Tone = tone,
						Accidental = accidental
					};

					return true;
				}                    
            }

            return false;
        }

        public string ToString(NamingConvention conv)
        {
            return string.Format("{0}{1}", Tone.GetDescriptions().ToArray()[(int)conv], Accidental.ToDescription());
        }

        public override string ToString()
		{
            return ToString(NamingConvention.English);
		}
	}
}
