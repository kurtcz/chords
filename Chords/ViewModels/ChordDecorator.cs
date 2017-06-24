using System.Linq;
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
        public string[] Symbols => _chord.Symbols;
        public string[] Intervals => _chord.Intervals
                                           .Select(i => i.ToDescription())
                                           .ToArray();
        public string[] Notes => _chord.Notes
                                       .Select(i => i.ToString(_namingConvention))
                                       .ToArray();

        public ChordDecorator(Chord chord, NamingConvention namingConvention = NamingConvention.English)
        {
            _chord = chord;
            _namingConvention = namingConvention;
		}

        public GuitarChordLayoutDecorator[] GenerateLayouts(bool allowSpecial, bool allowPartial)
        {
			return GuitarChordLayout.Generate(_chord, allowSpecial, allowPartial)
									.OrderBy(i => i.GuitarChordType)
									.Select(i => new GuitarChordLayoutDecorator(i, _namingConvention))
									.OrderBy(i => i.RenderingFret)
									.ThenBy(i => i.Complete ? 0 : 1)
									.ThenBy(i => (i.Notes[0] == _chord.Root ? 0 : 1))
									.ToArray();
		}
    }
}
