using System;
using System.Collections.Specialized;
using Chords.Core.Models;

namespace Chords.Android.Models
{
    public class ChordParams
    {
		public string Root { get; set; }
		public string ChordType { get; set; }
		public bool Partial { get; set; }
		public bool Special { get; set; }
		public NamingConvention NamingConvention { get; set; }

        public ChordParams(NameValueCollection parameters)
        {
			NamingConvention conv;
            bool part;
            bool spec;

			if (!Enum.TryParse(parameters["conv"], out conv))
			{
				conv = NamingConvention.English;
			}
            NamingConvention = conv;
            if (!bool.TryParse(parameters["partial"], out part))
            {
                part = false;
            }
            Partial = part;
			if (!bool.TryParse(parameters["special"], out spec))
			{
				spec = false;
			}
			Special = spec;
            Root = parameters["root"];
            ChordType = parameters["type"];
		}
	}
}
