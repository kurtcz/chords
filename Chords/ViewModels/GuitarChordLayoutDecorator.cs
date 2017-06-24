using System;
using System.Linq;
using Chords.Extensions;
using Chords.Models;

namespace Chords.ViewModels
{
    public class GuitarChordLayoutDecorator
    {
        private GuitarChordLayout _layout;
        public string GuitarChordType => _layout.GuitarChordType.ToDescription();
		public string[] Positions => _layout.Positions;
		public int[] IntPositions => _layout.IntPositions;
        public string[] Notes => _layout.Notes;
        public int Fret => _layout.Fret;
        public bool Complete => _layout.Complete;
        public int RenderingFret { get; }

        public GuitarChordLayoutDecorator(GuitarChordLayout layout)
        {
            _layout = layout;
			RenderingFret = Fret - 1;
			if (Fret <= 4 && IntPositions.All(i => i <= 4))
			{
				RenderingFret = 0;
			}
		}

        public override string ToString()
        {
            return string.Format("{0}\n{1}", _layout, string.Join(" ", Notes));
        }
    }
}
