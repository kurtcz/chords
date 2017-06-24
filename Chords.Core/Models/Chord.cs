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
		public string[] Symbols { get; private set; }
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
			Symbols = ChordType.GetDescriptions()
							   .Select(i => $"{Notes[0]}{i}")
                               .ToArray();
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

            foreach (var n in Note.AllNotes)
            {
                var notestr = n.ToString(conv);

                if (notestr.Length <= str.Length &&
                    notestr == str.Substring(0, notestr.Length))
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

                if (descriptions.Any(i => i.Equals(str.Substring(chordTypeOffset), StringComparison.OrdinalIgnoreCase)))
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
