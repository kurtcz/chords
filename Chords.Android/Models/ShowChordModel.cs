using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.Android.Models
{
    public class ShowChordModel : FindChordModel
    {
        public Note Root { get; set; }
        public ChordType ChordType { get; set; }
		public Note[] AllRoots { get; set; }
		public ChordType[] AllChordTypes { get; set; }
        public bool ShowBasicChordTypes { get; set; }
        private static ChordType[] BasicChordTypes =
        {
            ChordType.Major, ChordType.Minor, ChordType.DiminishedSeventh,
            ChordType.DominantSeventh, ChordType.MajorSeventh, ChordType.MinorSeventh,
            ChordType.Sixth,
            ChordType.Ninth, ChordType.MajorNinth, ChordType.MinorNinth,
            ChordType.Thirteenth
        };
		public readonly Dictionary<string, string> ChordTypeList = Enum.GetValues(typeof(ChordType))
												                       .Cast<ChordType>()
												                       .Select(i => new
												                       {
												                           Text = i.ToDescription() ?? i.ToString(),
												                           Value = i.ToDescription() ?? string.Empty
												                       })
                                                                       .ToDictionary(k => k.Text, v => v.Value);
		public readonly Dictionary<string, string> BasicChordTypeList = Enum.GetValues(typeof(ChordType))
																	   .Cast<ChordType>()
                                                                       .Where(i => BasicChordTypes.Contains(i))
																	   .Select(i => new
																	   {
																		   Text = i.ToDescription() ?? i.ToString(),
																		   Value = i.ToDescription() ?? string.Empty
																	   })
																	   .ToDictionary(k => k.Text, v => v.Value);

        public void Populate(NameValueCollection parameters)
        {
            Note root = null;
            ChordType chordType = 0;

            if(Note.TryParse(parameters["root"], NamingConvention.English, out root))
            {
                Root = root;
            }
            if (parameters["type"] != null)
            {
                foreach(var ct in Enum.GetValues(typeof(ChordType))
                                      .Cast<ChordType>())
                {
                    if (ct.GetDescriptions().FirstOrDefault() == parameters["type"])
                    {
                        ChordType = ct;
                        break;
                    }
                }
            }
		}
    }
}
