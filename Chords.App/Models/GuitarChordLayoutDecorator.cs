using System;
using System.Linq;
using System.Text;
using Chords.Core.Extensions;
using Chords.Core.Models;
using Microsoft.AspNetCore.Components;

namespace Chords.App.Models
{
	public class GuitarChordLayoutDecorator
	{
		private readonly GuitarChordLayout _layout;
		private readonly NamingConvention _namingConvention;

		public string GuitarChordType => _layout.GuitarChordType.ToDescription() ?? _layout.GuitarChordType.ToString();
		public int[] IntPositions => _layout.Positions;
		public string[] Positions => _layout.Positions
											.Select(i => i < 0 ? "X" : i.ToString())
											.ToArray();
        public Note[] Notes => _layout.Notes.ToArray();
		public int Fret => _layout.Fret;
		public bool Complete => _layout.Complete;
		public int RenderingFret { get; }
        public GuitarChordLayout Layout => _layout;
        public bool Favorite => Favorites.Contains(_layout.Chord, _layout);

		public GuitarChordLayoutDecorator(GuitarChordLayout layout, NamingConvention namingConvention = NamingConvention.English)
		{
			_layout = layout;
			RenderingFret = Fret - 1;
			var frets = layout.InstrumentInfo.DiagramFrets;
			if (Fret <= frets && IntPositions.All(i => i <= frets))
			{
				RenderingFret = 0;
			}
			_namingConvention = namingConvention;
		}

        public MarkupString Schema => RenderSchema(false, false);

        /// <summary>SVG chord diagram.</summary>
        /// <param name="horizontal">Strings run left to right with the nut on the left and the lowest string at the bottom.</param>
        /// <param name="leftHanded">Mirrors a horizontal diagram so the nut is on the right.</param>
        public MarkupString RenderSchema(bool horizontal, bool leftHanded)
        {
            var sb = new StringBuilder();
            var strings = IntPositions.Length;
            var color = Complete ? "black" : "grey";
            //diagram coordinates: a = across the strings (lowest string 0, next +20),
            //f = along the neck (note names 10, nut 20, frets every 40, dot of fret n at n*40)
            var span = (strings - 1) * 20;
            var info = _layout.InstrumentInfo;
            var frets = info.DiagramFrets;
            var neck = 20 + frets * 40;
            //an alternate tuning is spelled out past the end of the neck, its retuned strings highlighted
            var tuning = info.IsStandardTuning ? null
                : InstrumentTuning.FindPreset(info.Instrument, info.Pitches) ?? new InstrumentTuning(null, info.Instrument, info.Pitches);
            var tuningRoom = tuning == null ? 0 : 18;
            int svgWidth, svgHeight;
            Func<int, int, (int X, int Y)> P;
            if (horizontal)
            {
                svgWidth = 20 + neck + 40 + tuningRoom;
                svgHeight = 25 + span + 10;
                P = (a, f) => (leftHanded ? svgWidth - 20 - f : 20 + f, 25 + span - a);
            }
            else
            {
                svgWidth = 25 + span + 30;
                svgHeight = neck + 10 + tuningRoom;
                P = (a, f) => (25 + a, f);
            }
            int A(int s) => s * 20;
            string Line((int X, int Y) p1, (int X, int Y) p2, string stroke, int width)
                => $"\t\t<line x1=\"{p1.X}\" y1=\"{p1.Y}\" x2=\"{p2.X}\" y2=\"{p2.Y}\" stroke=\"{stroke}\" stroke-width=\"{width}\" />\n";
            string Circle((int X, int Y) c, string fill)
                => $"\t\t<circle cx=\"{c.X}\" cy=\"{c.Y}\" r=\"6\" fill=\"{fill}\" stroke=\"blue\" stroke-width=\"1\" />\n";

            sb.AppendLine($"\t<svg viewBox=\"0 0 {svgWidth} {svgHeight}\" width=\"{svgWidth}\" style=\"display:block; margin-bottom:10px;\">");
            //star next to the highest string, past the end of the neck when horizontal
            var star = horizontal ? P(span, neck + 25 + tuningRoom) : (X: 25 + span + 17, Y: 12);
            sb.Append($"\t\t<path class=\"star\" fill=\"#F8D64E\" d=\"M {star.X - 7} {star.Y + 11} l 7 -23 l 7 23 l -19 -14 h 24 Z\" />\n");
            //fretboard; fret wire n lies at 20 + (n - RenderingFret) * 40
            for (var s = 0; s < strings; s++)
            {
                //a short string (banjo's 5th) starts at its own nut further up the neck
                var start = Math.Max(20, 20 + (info.FirstFrets[s] - RenderingFret) * 40);
                if (start < neck)
                {
                    sb.Append(Line(P(A(s), start), P(A(s), neck), color, 1));
                }
                if (info.IsShortString(s) && start > 20 && start < neck)
                {
                    sb.Append(Line(P(A(s) - 4, start), P(A(s) + 4, start), color, 3));
                }
            }
            for (var i = 0; i <= frets; i++)
            {
                sb.Append(Line(P(A(info.FirstStringAtFret(RenderingFret + i)), 20 + i * 40), P(span, 20 + i * 40), color, 1));
            }
            if (RenderingFret == 0)
            {
                sb.Append(Line(P(A(info.FirstStringAtFret(0)), 20), P(span, 20), color, 2));
            }
            //note names
            var notes = ToString().Split(' ');
            for (var s = 0; s < strings; s++)
            {
                if (notes[s] == "X")
                {
                    continue;
                }
                //sideways the names need more room so they don't touch open-string circles
                var p = P(A(s), horizontal ? 0 : 10);
                sb.Append($"\t\t<text x=\"{p.X}\" y=\"{p.Y + (horizontal ? 4 : 0)}\" font-size=\"12\" fill=\"{color}\" stroke=\"{color}\" stroke-width=\"1\" text-anchor=\"middle\">{notes[s]}</text>\n");
            }
            //barre
            if (_layout.GuitarChordType == Core.Models.GuitarChordType.FiveStringBarre ||
                _layout.GuitarChordType == Core.Models.GuitarChordType.SixStringBarre)
            {
                var loBarreString = _layout.GuitarChordType == Core.Models.GuitarChordType.SixStringBarre ? 0 : 1;
                var f = (IntPositions[loBarreString] - RenderingFret) * 40;
                var end = P(A(strings - 1), f);
                sb.Append(Line(P(A(loBarreString), f), end, "blue", 5));
                sb.Append($"\t\t<circle cx=\"{end.X}\" cy=\"{end.Y}\" r=\"2\" fill=\"blue\" stroke=\"blue\" stroke-width=\"1\" />\n");
            }
            else if (BarreStrings() is (int first, int last))
            {
                var f = (IntPositions[first] - RenderingFret) * 40;
                sb.Append(Line(P(A(first), f), P(A(last), f), "blue", 5));
            }
            for (var s = 0; s < strings; s++)
            {
                //muted string
                if (IntPositions[s] == GuitarChordLayout.X)
                {
                    var c = P(A(s), 20);
                    sb.Append(Line((c.X - 6, c.Y - 6), (c.X + 6, c.Y + 6), "red", 4));
                    sb.Append(Line((c.X - 6, c.Y + 6), (c.X + 6, c.Y - 6), "red", 4));
                }
                //empty string
                else if (IntPositions[s] == 0)
                {
                    sb.Append(Circle(P(A(s), 20), "white"));
                }
                //pressed string
                else
                {
                    sb.Append(Circle(P(A(s), (IntPositions[s] - RenderingFret) * 40), "blue"));
                }
            }
            if (tuning != null)
            {
                for (var s = 0; s < strings; s++)
                {
                    var p = P(A(s), neck + (horizontal ? 16 : 17));
                    var retuned = tuning.Offset(s) != 0;
                    sb.Append($"\t\t<text class=\"tuning{(retuned ? " retuned" : null)}\" x=\"{p.X}\" y=\"{p.Y + (horizontal ? 4 : 0)}\" font-size=\"11\" text-anchor=\"middle\" fill=\"{(retuned ? "#d9822b" : "#999")}\" font-weight=\"{(retuned ? "bold" : "normal")}\">{tuning.NoteOf(s).ToString(_namingConvention)}</text>\n");
                }
            }
            //fret numbers: left of a vertical neck, above a horizontal one
            for (var i = 0; i <= frets; i++)
            {
                var p = horizontal ? (P(span, 20 + i * 40).X, 12) : (15, 23 + i * 40);
                sb.Append($"\t\t<text x=\"{p.Item1}\" y=\"{p.Item2}\" text-anchor=\"{(horizontal ? "middle" : "end")}\" fill=\"{color}\" stroke=\"none\" font-size=\"12\">{RenderingFret + i}</text>\n");
            }
            sb.AppendLine("\t</svg>");

            return new MarkupString(sb.ToString());
        }

        /// <summary>
        /// Barre of instruments other than the guitar (whose chord types say it): the index finger across
        /// at least three strings at the lowest pressed fret, with no open or muted string under it.
        /// </summary>
        private (int First, int Last)? BarreStrings()
        {
            if (_layout.Instrument == Instrument.Guitar || !IntPositions.Any(i => i > 0))
            {
                return null;
            }
            var fret = IntPositions.Where(i => i > 0).Min();
            var first = Array.IndexOf(IntPositions, fret);
            var last = Array.LastIndexOf(IntPositions, fret);

            if (IntPositions.Count(i => i == fret) < 3 ||
                IntPositions.Skip(first).Take(last - first + 1).Any(i => i < fret))
            {
                return null;
            }

            return (first, last);
        }

		public override string ToString()
		{
            var sb = new StringBuilder();
            var k = 0;

            for (var s = 0; s < IntPositions.Length && k < Notes.Length; s++)
            {
                if (IntPositions[s] == -1)
                {
                    sb.Append("X");
                }
                else
                {
                    sb.Append(Notes[k++].ToString(_namingConvention));
                }
                if (s < IntPositions.Length - 1)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
		}

        public override int GetHashCode()
        {
            return _layout.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as GuitarChordLayoutDecorator;

            if (other == null)
            {
                return false;
            }

            return this._layout.Equals(other._layout);
        }
	}
}
