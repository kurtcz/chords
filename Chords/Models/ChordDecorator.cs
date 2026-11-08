using System;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.Models
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
		/// <summary>Notes from the lowest: a slash chord starts with its bass (E G C for C/E).</summary>
		public string[] Notes => _chord.NotesFromBass
									   .Select(i => i.ToString(_namingConvention))
									   .ToArray();
		/// <summary>Chord symbols, the usual one first; a slash chord ends with its bass (D/F♯).</summary>
		public string[] Symbols
		{
			get
			{
				var root = _chord.Root.ToString(_namingConvention);
				var bass = _chord.Bass == null ? null : "/" + _chord.Bass.ToString(_namingConvention);
				var symbols = _chord.ChordType.GetDescriptions()
									.Select(i => root + i + bass)
									.ToArray();
				if (symbols.Length == 0)
				{
					symbols = new[] { root + bass };
				}

				return symbols;
			}
		}

		public ChordDecorator(Chord chord, NamingConvention namingConvention)
		{
			_chord = chord;
			_namingConvention = namingConvention;
		}

		/// <summary>Fingerings up the neck from the nut, at the same fret the easiest first.</summary>
		/// <param name="easyOnly">Only easy fingerings; when the chord has none, the easiest level it has.</param>
		/// <param name="rootBassOnly">Only fingerings with the root lowest, without inversions (C/E, C/G).</param>
		public GuitarChordLayoutDecorator[] GenerateLayouts(InstrumentInfo instrument, bool allowBarre, bool allowSpecial, bool allowPartial, int maxFret, bool easyOnly, bool rootBassOnly = false)
		{
			var layouts = GuitarChordLayout.Generate(_chord, instrument, allowBarre, allowSpecial, allowPartial, maxFret).ToArray();

			if (rootBassOnly)
			{
				layouts = layouts.Where(i => !i.OtherBass).ToArray();
			}

			if (easyOnly && layouts.Length > 0)
			{
				var easiest = layouts.Min(i => i.DifficultyLevel);
				layouts = layouts.Where(i => i.DifficultyLevel == easiest).ToArray();
			}

			return layouts.OrderBy(i => i.GuitarChordType)
						  .Select(i => new GuitarChordLayoutDecorator(i, _namingConvention))
						  .OrderBy(i => i.RenderingFret)
						  .ThenBy(i => i.Layout.DifficultyLevel)
						  .ThenBy(i => i.Complete ? 0 : 1)
						  .ToArray();
		}
	}
}
