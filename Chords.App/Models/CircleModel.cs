using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.App.Models
{
    public class CircleModel
    {
        public readonly NamingConvention[] NamingConventions = Enum.GetValues(typeof(NamingConvention))
                                                                   .Cast<NamingConvention>()
                                                                   .ToArray();
		public Dictionary<NamingConvention, string[]> CircleOfFifths =
			Enum.GetValues(typeof(NamingConvention))
				.Cast<NamingConvention>()
				.SelectMany(i => Enumerable.Range(0, 33)
										   .Select(signature =>
										   {
                                               var root = new Note(Tone.G, Accidental.DoubleFlat);
											   Note note = root;

											   for (var k = 0; k < signature; k++)
											   {
												   note = note.NoteAtInterval(Interval.P5);
											   }

											   return new
											   {
												   Key = i,
												   Value = note.ToString(i)
											   };
										   }))
				.GroupBy(g => g.Key)
				.ToDictionary(k => k.Key, v => v.Select(i => i.Value).ToArray());

        public Dictionary<NamingConvention, string[]> MajorCircle { get; }        
        public Dictionary<NamingConvention, string[]> MinorCircle { get; }        
        public Dictionary<NamingConvention, string[]> DiminishedCircle { get; }

		public NamingConvention conv { get; set; }
        public int index { get; set; }
        public string Signature { get; set; }
        public string Error { get; set; }

        /// <summary>Lowest and highest index the circle can show (Cb major … C# major); 13 is C major.</summary>
        public const int MinIndex = 6, MaxIndex = 20, CIndex = 13;

        /// <summary>Tonic of the major key at <paramref name="index"/> fifths above Gbb.</summary>
        public static Note MajorTonic(int index)
        {
            var note = new Note(Tone.G, Accidental.DoubleFlat);

            for (var k = 0; k < index; k++)
            {
                note = note.NoteAtInterval(Interval.P5);
            }

            return note;
        }

        /// <summary>
        /// Index of a major key; an enharmonic spelling the circle doesn't show (D# major) is matched
        /// to the one with fewer accidentals (Eb major).
        /// </summary>
        public static int IndexOf(Note majorTonic)
        {
            var indexes = Enumerable.Range(MinIndex, MaxIndex - MinIndex + 1).ToArray();
            var exact = indexes.Where(i => MajorTonic(i).Equals(majorTonic)).ToArray();

            if (exact.Length > 0)
            {
                return exact[0];
            }

            return indexes.Where(i => MajorTonic(i).ChromaticDistance(majorTonic) == 0)
                          .OrderBy(i => Math.Abs(i - CIndex))
                          .DefaultIfEmpty(CIndex)
                          .First();
        }

        public CircleModel(int index)
        {
            if (index < 6)
            {
                index += 12;
            }
            if (index > 20)
            {
                index -= 12;
            }
            this.index = index;

            Signature = string.Empty;
            if (index < 13)
            {
                Signature = $"{13 - index}b";
            }
            else if (index > 13)
            {
                Signature = $"{index - 13}#";
            }

			MajorCircle =
				Enum.GetValues(typeof(NamingConvention))
					.Cast<NamingConvention>()
					.SelectMany(i =>
						CircleOfFifths[i].Skip(index - 6)
										 .Take(12)
										 .Select(j => new
										 {
											 Key = i,
											 Value = j
										 }))
					.GroupBy(g => g.Key)
					.ToDictionary(k => k.Key, v => v.Select(i => i.Value).ToArray());
			MinorCircle =
				Enum.GetValues(typeof(NamingConvention))
					.Cast<NamingConvention>()
					.SelectMany(i =>
						CircleOfFifths[i].Skip(index - 3)
										 .Take(12)
										 .Select(j => new
										 {
											 Key = i,
											 Value = $"{j}m"
										 }))
					.GroupBy(g => g.Key)
					.ToDictionary(k => k.Key, v => v.Select(i => i.Value).ToArray());
            DiminishedCircle =
				Enum.GetValues(typeof(NamingConvention))
					.Cast<NamingConvention>()
					.SelectMany(i =>
						CircleOfFifths[i].Skip(index - 1)
										 .Take(12)
										 .Select(j => new
										 {
											 Key = i,
	                                         Value = $"{j}°"
										 }))
					.GroupBy(g => g.Key)
					.ToDictionary(k => k.Key, v => v.Select(i => i.Value).ToArray());
        }
    }
}
