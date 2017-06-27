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
		private GuitarChordLayout _layout;
		private NamingConvention _namingConvention;

		public string GuitarChordType => _layout.GuitarChordType.ToDescription() ?? _layout.GuitarChordType.ToString();
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

        public HtmlString Schema
        { 
            get
            {
                var sb = new StringBuilder();

                for (var f = 0; f < 5; f++)
                {
                    if (f == 0)
                    {
                        sb.AppendFormat("<div style=\"font-family: monospace;\">[{0}]&nbsp;{1}", RenderingFret, RenderingFret < 10 ? "&nbsp;" : "");
                        sb.Append(RenderingFret == 0 ? "<span style=\"font-weight:bold;\">=</span>" : "-");

                        for (var s = 0; s < 6; s++)
                        {
                            if (IntPositions[s] == 0 ||
                                IntPositions[s] == GuitarChordLayout.X)
                            {
                                sb.AppendFormat("<span class=\"{0}\">{0}</span>", IntPositions[s] == GuitarChordLayout.X ? "X" : "O");
                                sb.Append(RenderingFret == 0 ? "<span style=\"font-weight:bold;\">=</span>" : "-");
                            }
                            else
                            {
                                sb.Append(RenderingFret == 0 ? "<span style=\"font-weight:bold;\">==</span>" : "--");
                            }
                        }
                        sb.AppendLine("</div>");
                        continue;
                    }
                    sb.AppendFormat("<div style=\"font-family: monospace;\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|");
                    for (var s = 0; s < 6; s++)
                    {
                        if (IntPositions[s] == RenderingFret + f)
                        {
                            sb.Append("<span class=\"O\">O</span>");
                        }
                        else
                        {
                            sb.Append("&nbsp;");
                        }
                        sb.Append("|");
                    }
                    sb.AppendLine("</div>");
                    sb.AppendFormat("<div style=\"font-family: monospace;\">[{0}]&nbsp;{1}-------------</div>\n",
                                    RenderingFret + f, RenderingFret + f < 10 ? "&nbsp;" : "");
                }

				return new HtmlString(sb.ToString());
			}
        }

		public HtmlString ToHtmlString()
		{
			return new HtmlString(ToString().Replace("\n", "<br />\n"));
		}
		public override string ToString()
		{
			return string.Format("{0}\n{1}", _layout, string.Join(" ", Notes.Select(i => i.ToString(_namingConvention))));
		}
	}
}
