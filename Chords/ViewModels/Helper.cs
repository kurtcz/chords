using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Chords.Extensions;
using Chords.Models;

namespace Chords.ViewModels
{
    public static class Helper
    {
		public static readonly IEnumerable<SelectListItem> ChordTypeList =
			Enum.GetValues(typeof(ChordType))
				.Cast<ChordType>()
				.Select(i => new SelectListItem
				{
					Text = i.ToDescription(),
					Value = i.ToDescription(returnNullIfNoOrEmptyDescription: true) ?? string.Empty
				});

		public static readonly Dictionary<NamingConvention, SelectList> Notes = new Dictionary<NamingConvention, SelectList>
        {
            { NamingConvention.English, new SelectList(new [] { "C", "C#", "Db", "D", "D#", "Eb", "E", "F", "F#", "Gb", "G", "G#", "Ab", "A", "A#", "Bb", "B" }) },
            { NamingConvention.German, new SelectList(new [] { "C", "C#", "Db", "D", "D#", "Eb", "E", "F", "F#", "Gb", "G", "G#", "Ab", "A", "A#", "B", "H" }) },
            { NamingConvention.Latin, new SelectList(new [] { "Do", "Do#", "Reb", "Re", "Re#", "Mib", "Mi", "Fa", "Fa#", "Solb", "Sol", "Sol#", "Lab", "La", "La#", "Sib", "Si" }) }
        };

        public static IEnumerable<SelectListItem> NamingConventionList(NamingConvention conv)
        {
            return Enum.GetValues(typeof(NamingConvention))
                       .Cast<NamingConvention>()
                       .Select(i => new SelectListItem
                       {
                           Text = i.ToString(),
                           Value = i.ToString(),
                           Selected = i == conv
                       });
        }
    }
}
