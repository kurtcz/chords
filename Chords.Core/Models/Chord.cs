using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Extensions;

namespace Chords.Core.Models
{
	public class Chord
	{
		public Note Root { get; private set; }
		public ChordType ChordType { get; private set; }
        public Interval[] Intervals { get; private set; }
		public Note[] Notes { get; private set; }
        public Note[] NonMandatoryNotes { get; private set; }

		private Chord()
        {
        }

        public Chord(Note root, ChordType chordType)
        {
            Root = root;
            ChordType = chordType;
			Intervals = ChordType.ToIntervals();
			Notes = Intervals.Select(i => Root.NoteAtInterval(i))
							 .ToArray();
			NonMandatoryNotes = ChordType.ToNonMandatoryIntervals()
										 .Select(i => Root.NoteAtInterval(i))
										 .ToArray();
		}

		public static Chord Find(Note[] notes, bool strict)
		{
            foreach(var note in notes)
            {
                foreach(var chordType in Enum.GetValues(typeof(ChordType)).Cast<ChordType>())
                {
                    var chord = new Chord(note, chordType);

                    if (chord.Notes.Length == notes.Length &&
                        chord.Notes.All(i => notes.Any(j => i == j ||
                                                            (!strict && Note.Normalize(i) == Note.Normalize(j)))))
                    {
                        return chord;
                    }
                }
            }

            return null;
		}

		public static bool TryParse(string str, NamingConvention conv, out Chord chord)
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

        public override string ToString()
        {
            return string.Format("[Chord: Root={0}, ChordType={1}]", Root, ChordType);
        }
    }
}
