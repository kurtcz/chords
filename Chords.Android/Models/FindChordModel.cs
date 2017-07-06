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
        public bool Strict { get; set; }
        public string[] SelectedNotes { get; set; }
        public readonly Note[] Notes =
        {
            new Note(Tone.C, Accidental.Flat),
            new Note(Tone.C),
            new Note(Tone.C, Accidental.Sharp),
            new Note(Tone.D, Accidental.Flat),
            new Note(Tone.D),
            new Note(Tone.D, Accidental.Sharp),
            new Note(Tone.E, Accidental.Flat),
            new Note(Tone.E),
            new Note(Tone.F),
            new Note(Tone.F, Accidental.Sharp),
            new Note(Tone.G, Accidental.Flat),
            new Note(Tone.G),
            new Note(Tone.G, Accidental.Sharp),
            new Note(Tone.A, Accidental.Flat),
            new Note(Tone.A),
            new Note(Tone.A, Accidental.Sharp),
            new Note(Tone.B)
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
