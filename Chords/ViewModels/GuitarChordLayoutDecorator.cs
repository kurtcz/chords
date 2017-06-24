using System;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.ViewModels
{
    public class GuitarChordLayoutDecorator
    {
        private GuitarChordLayout _layout;
		private NamingConvention _namingConvention;

        public string GuitarChordType => _layout.GuitarChordType.ToDescription();
        public int[] IntPositions => _layout.Positions;
        public string[] Positions => _layout.Positions
                                            .Select(i => i < 0 ? "X" : i.ToString())
                                            .ToArray();
        public Note[] Notes => _layout.Notes;
        public int Fret => _layout.Fret;
        public bool Complete => _layout.Complete;
        public int RenderingFret { get; }

        public GuitarChordLayoutDecorator(GuitarChordLayout layout, NamingConvention namingConvention = NamingConvention.English)
        {
            _layout = layout;
			RenderingFret = Fret - 1;
			if (Fret <= 4 && IntPositions.All(i => i <= 4))
			{
				RenderingFret = 0;
			}
			_namingConvention = namingConvention;
		}

        public override string ToString()
        {
            return string.Format("{0}\n{1}", _layout, string.Join(" ", Notes.Select(i => i.ToString(_namingConvention))));
        }
    }
}
