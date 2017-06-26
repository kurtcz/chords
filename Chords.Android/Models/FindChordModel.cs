using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;
using PortableRazor.Web.Mvc;

namespace Chords.Android.Models
{
    public class FindChordModel
    {
		public NamingConvention conv { get; set; }
        public string[] SelectedNotes { get; set; }
		public Dictionary<NamingConvention, string[]> Notes = new Dictionary<NamingConvention, string[]>
		{
			{ NamingConvention.English, new [] { "Cb", "C", "C#", "Db", "D", "D#", "Eb", "E", "E#", "Fb", "F", "F#", "Gb", "G", "G#", "Ab", "A", "A#", "Bb", "B" } },
			{ NamingConvention.German, new [] { "Cb", "C", "C#", "Db", "D", "D#", "Eb", "E", "E#", "Fb", "F", "F#", "Gb", "G", "G#", "Ab", "A", "A#", "B", "H" } },
			{ NamingConvention.Latin, new [] { "Dob", "Do", "Do#", "Reb", "Re", "Re#", "Mib", "Mi", "Mi#", "Fab", "Fa", "Fa#", "Solb", "Sol", "Sol#", "Lab", "La", "La#", "Sib", "Si" } }
		};
        public readonly Dictionary<NamingConvention, HtmlString[]> AllNotes =
            Enum.GetValues(typeof(NamingConvention))
                .Cast<NamingConvention>()
                .SelectMany(i => Enum.GetValues(typeof(Tone))
                                     .Cast<Tone>()
                                     .SelectMany(t => Enum.GetValues(typeof(Accidental))
                                                          .Cast<Accidental>()
                                                          .Select(a => new
                                                          {
                                                              Tone = t,
                                                              Accidental = a,
                                                              NamingConvention = i
                                                          })))
                .OrderBy(i => i.Tone)
                .ThenBy(i => i.Accidental)
                .GroupBy(i => i.NamingConvention)
                .ToDictionary(k => k.Key,
                              v => v.Select(i => GetHtmlString(i.Tone, i.Accidental, i.NamingConvention))
                                    .ToArray());
		public readonly NamingConvention[] NamingConventions = Enum.GetValues(typeof(NamingConvention))
														           .Cast<NamingConvention>()
														           .ToArray();
        public string Error { get; set; }

        private static HtmlString GetHtmlString(Tone tone, Accidental accidental, NamingConvention conv)
        {
            if (tone == Tone.B && accidental == Accidental.Flat && conv == NamingConvention.German)
            {
                return new HtmlString("B");
            }
            var str = string.Format("{0}<sub>{1}</sub>", tone.GetDescriptions().ToArray()[(int)conv], accidental.ToDescription());

            return new HtmlString(str);
        }
	}
}
