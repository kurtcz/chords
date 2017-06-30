using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.Android.Models
{
    public class ShowChordModel : FindChordModel
    {
        public Note Root { get; set; }
        public ChordType ChordType { get; set; }
        public readonly Dictionary<string, string> ChordTypeList = Enum.GetValues(typeof(ChordType))
												                       .Cast<ChordType>()
												                       .Select(i => new
												                       {
												                           Text = i.ToDescription() ?? i.ToString(),
												                           Value = i.ToDescription() ?? string.Empty
												                       })
                                                                       .ToDictionary(k => k.Text, v => v.Value);
	}
}
