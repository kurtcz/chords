﻿﻿using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.ViewModels
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

        public GuitarChordLayoutDecorator[] GenerateLayouts(bool allowBarre, bool allowSpecial, bool allowPartial, int maxFret)
        {
			return GuitarChordLayout.Generate(_chord, allowBarre, allowSpecial, allowPartial, maxFret)
									.OrderBy(i => i.GuitarChordType)
									.Select(i => new GuitarChordLayoutDecorator(i, _namingConvention))
									.OrderBy(i => i.RenderingFret)
									.ThenBy(i => i.Complete ? 0 : 1)
									.ThenBy(i => (i.Notes[0] == _chord.Root ? 0 : 1))
									.ToArray();
		}
    }
}
