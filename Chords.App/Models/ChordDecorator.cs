using System;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.App.Models
{
	public class ChordDecorator
	{
		private Chord _chord;
		private NamingConvention _namingConvention;

		public Note Root => _chord.Root;
		public ChordType ChordType => _chord.ChordType;
		public string[] Intervals => _chord.Intervals
										   .Select(i => i.ToDescription())
										   .ToArray();
		public string[] Notes => _chord.Notes
									   .Select(i => i.ToString(_namingConvention))
									   .ToArray();
		public string[] Symbols
		{
			get
			{
				var symbols = _chord.ChordType.GetDescriptions()
									.Select(i => string.Format("{0}{1}", _chord.Notes[0].ToString(_namingConvention), i))
									.ToArray();
				if (symbols.Length == 0)
				{
					symbols = new[] { _chord.Notes[0].ToString(_namingConvention) };
				}

				return symbols;
			}
		}

		public ChordDecorator(Chord chord, NamingConvention namingConvention)
		{
			_chord = chord;
			_namingConvention = namingConvention;
		}

		/// <summary>Fingerings from the easiest, each difficulty level ordered up the neck.</summary>
		/// <param name="easyOnly">Only easy fingerings; when the chord has none, the easiest level it has.</param>
		public GuitarChordLayoutDecorator[] GenerateLayouts(InstrumentInfo instrument, bool allowBarre, bool allowSpecial, bool allowPartial, int maxFret, bool easyOnly)
		{
			var layouts = GuitarChordLayout.Generate(_chord, instrument, allowBarre, allowSpecial, allowPartial, maxFret).ToArray();

			if (easyOnly && layouts.Length > 0)
			{
				var easiest = layouts.Min(i => i.DifficultyLevel);
				layouts = layouts.Where(i => i.DifficultyLevel == easiest).ToArray();
			}

			return layouts.OrderBy(i => i.GuitarChordType)
						  .Select(i => new GuitarChordLayoutDecorator(i, _namingConvention))
						  .OrderBy(i => i.Layout.DifficultyLevel)
						  .ThenBy(i => i.RenderingFret)
						  .ThenBy(i => i.Complete ? 0 : 1)
						  .ThenBy(i => (i.Notes[0] == _chord.Root ? 0 : 1))
						  .ToArray();
		}
	}
}
