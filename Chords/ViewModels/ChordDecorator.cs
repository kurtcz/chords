using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Models;

namespace Chords.ViewModels
{
    public class ChordDecorator
    {
        private Chord _chord;
        public string Root => _chord.Root;
        public ChordType ChordType => _chord.ChordType;
        public string[] Symbols => _chord.Symbols;
        public string[] Intervals => _chord.Intervals;
        public string[] Notes => _chord.Notes;
        public GuitarChordLayoutDecorator[] Layouts { get; private set; }

        public ChordDecorator(Chord chord, bool allowSpecial = false, bool allowPartial = false)
        {
            _chord = chord;

            Layouts = GuitarChordLayout.Generate(chord, allowSpecial, allowPartial)
                                       .OrderBy(i => i.GuitarChordType)
									   .Select(i => new GuitarChordLayoutDecorator(i))
									   .OrderBy(i => i.RenderingFret)
                                       .ThenBy(i => i.Complete ? 0 : 1)
									   .ThenBy(i => (i.Notes[0] == _chord.Root ? 0 : 1))
									   .ToArray();
		}
    }
}
