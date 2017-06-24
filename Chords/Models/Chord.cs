using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Extensions;

namespace Chords.Models
{
	public class Chord
	{
		public NamingConvention NamingConvention { get; private set; }
		public string Root { get; private set; }
		public ChordType ChordType { get; private set; }
		public string[] Symbols { get; private set; }
		public string[] Intervals { get; private set; }
		public string[] Notes { get; private set; }
        public string[] NonMandatoryNotes { get; private set; }

		private Chord()
        {
        }

        public Chord(string root, ChordType chordType, NamingConvention conv)
        {
            Root = root;
            ChordType = chordType;
            NamingConvention = conv;
            CalculateProperties();
        }

        public static Chord Parse(string str, NamingConvention conv)
		{
			if (string.IsNullOrWhiteSpace(str))
			{
				throw new ArgumentNullException();
			}

			var result = new Chord();
			var chordTypeOffset = 0;
			var chordTypeFound = false;

			foreach (var note in Definitions.Notes1[conv]                          //C,C#,D,D#,...
										  .Concat(Definitions.Notes2[conv])       //C,Db,D,Eb,...
										  .Distinct()
										  .Where(i => i.Length <= str.Length)
										  .OrderByDescending(i => i.Length))
			{
				if (string.Equals(note, str.Substring(0, note.Length), StringComparison.CurrentCultureIgnoreCase))
				{
					chordTypeOffset = note.Length;
					result.Root = note;
                    break;
				}
			}
            if (result.Root == null)
			{
				throw new ArgumentException("Unknown base note");
			}
            foreach (var chordType in Enum.GetValues(typeof(ChordType)).Cast<ChordType>())
			{
				var descriptions = chordType.GetDescriptions();

				if (descriptions.Any(i => string.Equals(i, str.Substring(chordTypeOffset), StringComparison.CurrentCultureIgnoreCase)))
				{
					result.ChordType = chordType;
					chordTypeFound = true;
					break;
				}
			}
			if (!chordTypeFound)
			{
				throw new ArgumentException("Unknown chord type");
			}
			result.NamingConvention = conv;
            result.CalculateProperties();

			return result;
		}

        private void CalculateProperties()
        {
            var rootIndex1 = Array.IndexOf(Definitions.Notes1[NamingConvention], Root);
            var rootIndex2 = Array.IndexOf(Definitions.Notes2[NamingConvention], Root);
            var rootIndex = rootIndex1 >= 0 ? rootIndex1 : rootIndex2;
            var normalizedNoteIndices = Definitions.ChordIntervals[ChordType]
                                                 .Select(interval => (rootIndex + Definitions.IntervalToHalftones[interval]) %
                                                                     Definitions.Notes1[NamingConvention].Length);
            var normalizedNonMandatoryNoteIndices = Definitions.NonMandatoryChordIntervals.ContainsKey(ChordType)
                                        ? Definitions.NonMandatoryChordIntervals[ChordType]
                                          .Select(interval => (rootIndex + Definitions.IntervalToHalftones[interval]) %
                                                              Definitions.Notes1[NamingConvention].Length)
                                        : Enumerable.Empty<int>();
            var chordTypeSymbol = ChordType.ToDescription();

            Notes = rootIndex1 >= 0
                             ? normalizedNoteIndices.Select(i => Definitions.Notes1[NamingConvention][i]).ToArray()
                    : normalizedNoteIndices.Select(i => Definitions.Notes2[NamingConvention][i]).ToArray();
            NonMandatoryNotes = rootIndex1 >= 0
            ? normalizedNonMandatoryNoteIndices.Select(i => Definitions.Notes1[NamingConvention][i]).ToArray()
            : normalizedNonMandatoryNoteIndices.Select(i => Definitions.Notes2[NamingConvention][i]).ToArray();

            Intervals = Definitions.ChordIntervals[ChordType].Select(i => i.ToDescription()).ToArray();
            Symbols = ChordType.GetDescriptions()
                               .Select(i => $"{Notes[0]}{i}")
                               .ToArray();
        }

        public override string ToString()
        {
            return string.Format("[Chord: Root={0}, ChordType={1}]", Root, ChordType);
        }
    }
}
