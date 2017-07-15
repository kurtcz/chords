﻿using System;
using System.Linq;
using System.Text;
using Chords.Core.Extensions;
using Chords.Core.Models;
using PortableRazor.Web.Mvc;

namespace Chords.Android.Models
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
        public bool Favorite => Favorites.Chords.ContainsKey(_layout.Chord) &&
                                Favorites.Chords[_layout.Chord].Contains(_layout);

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

        public HtmlString Schema
        {
            get
            {
				var sb = new StringBuilder();
				var star = string.Join(" ", new[]
				{
					"M", "135", "23",
					"l", "7", "-23",
                    "l", "7", "23",
                    "l", "-19", "-14",
                    "h", "24",
                    "Z"
				});
                var fret0 = string.Join(" ", new[]
                {
                    "M", "25", "20",
                    "h", "100"
                });
                var frets = string.Join(" ", new[]
                {
                    "M", "25", "20",
                    "v", "160",
                    "M", "45", "20",
                    "v", "160",
					"M", "65", "20",
					"v", "160",
					"M", "85", "20",
					"v", "160",
					"M", "105", "20",
					"v", "160",
					"M", "125", "20",
					"v", "160",
					"M", "25", "20",
                    "h", "100",
					"M", "25", "60",
					"h", "100",
					"M", "25", "100",
					"h", "100",
					"M", "25", "140",
					"h", "100",
					"M", "25", "180",
					"h", "100"
				});
				sb.AppendLine("\t<svg viewBox=\"0 0 155 190\" width=\"155\" style=\"display:block; margin-bottom:10px;\">");
				//sb.AppendLine("\t<svg width=\"160\" height=\"200\" style=\"display:block;\">");
                //star
                sb.AppendFormat("\t\t<path class=\"star\" fill=\"#F8D64E\" d=\"{0}\" />\n", star);
				//fretboard
				sb.AppendFormat("\t\t<path d=\"{0}\" fill=\"none\" stroke=\"{1}\" stroke-width=\"1\" />\n", frets, Complete ? "black" : "grey");
                if (RenderingFret == 0)
                {
                    sb.AppendFormat("\t\t<path d=\"{0}\" fill=\"{1}\" stroke=\"{1}\" stroke-width=\"2\" />\n", fret0, Complete ? "black" : "grey");
                }
				//note names
				var notes = ToString().Split(' ');

                for (var s = 0; s < 6; s++)
                {
                    if (notes[s] == "X")
                    {
                        continue;
                    }
                    sb.AppendFormat("\t\t<text x=\"{0}\", y=\"{1}\" font-size=\"12\" fill=\"{2}\" stroke=\"{2}\" stroke-width=\"1\" text-anchor=\"middle\">{3}</text>\n", 
                                    25 + s * 20, 10, Complete ? "black" : "grey", notes[s]);
                }
				//barre
				if (_layout.GuitarChordType == Core.Models.GuitarChordType.FiveStringBarre ||
                    _layout.GuitarChordType == Core.Models.GuitarChordType.SixStringBarre)
                {
                    var loBarreString = _layout.GuitarChordType == Core.Models.GuitarChordType.SixStringBarre ? 0 : 1;
					var hiBarreString = 5;

                    sb.AppendFormat("\t\t<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" stroke=\"blue\" stroke-width=\"5\" />\n",
                                    25 + loBarreString * 20, (IntPositions[loBarreString] - RenderingFret) * 40, 
                                    25 + hiBarreString * 20, (IntPositions[loBarreString] - RenderingFret) * 40);
                    sb.AppendFormat("\t\t<circle cx=\"{0}\" cy=\"{1}\" r=\"2\" fill=\"blue\" stroke=\"blue\" stroke-width=\"1\" />\n",
                                    25 + hiBarreString * 20, (IntPositions[loBarreString] - RenderingFret) * 40);
                }
				for (var s = 0; s < 6; s++)
				{
                    //muted string
					if (IntPositions[s] == GuitarChordLayout.X)
                    {
                        sb.AppendFormat("\t\t<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" stroke=\"red\" stroke-width=\"4\" />\n",
                                        25 + s * 20 - 6, 20 - 6,
                                        25 + s * 20 + 6, 20 + 6);
						sb.AppendFormat("\t\t<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" stroke=\"red\" stroke-width=\"4\" />\n",
										25 + s * 20 - 6, 20 + 6,
										25 + s * 20 + 6, 20 - 6);
					}
                    //empty string
                    else if (IntPositions[s] == 0)
                    {
						sb.AppendFormat("\t\t<circle cx=\"{0}\" cy=\"{1}\" r=\"6\" fill=\"white\" stroke=\"blue\" stroke-width=\"1\" />\n", 25 + s * 20, 20);
					}
                    //pressed string
                    else
					{
                        sb.AppendFormat("\t\t<circle cx=\"{0}\" cy=\"{1}\" r=\"6\" fill=\"blue\" stroke=\"blue\" stroke-width=\"1\" />\n", 25 + s * 20, (IntPositions[s] - RenderingFret) * 40);
					}
				}
                //fret numbers
                for (var f = 0; f < 5; f++)
                {
                    sb.AppendFormat("\t\t<text x=\"{0}\" y=\"{1}\" text-anchor=\"end\" fill=\"{2}\" stroke=\"none\" font-size=\"12\">{3}</text>\n",
                                     15, 23 + f * 40, Complete ? "black" : "grey", RenderingFret + f);
                }
                sb.AppendLine("\t</svg>");

				return new HtmlString(sb.ToString());
            }
        }

		public override string ToString()
		{
            var sb = new StringBuilder();
            var k = 0;

            for (var s = 0; s < 6 && k < Notes.Length; s++)
            {
                if (IntPositions[s] == -1)
                {
                    sb.Append("X");
                }
                else
                {
                    sb.Append(Notes[k++].ToString(_namingConvention));
                }
                if (s < 5)
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
