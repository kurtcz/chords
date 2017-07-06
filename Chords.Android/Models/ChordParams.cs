using System;
using System.Collections.Specialized;
using Chords.Core.Models;

namespace Chords.Android.Models
{
    public class ChordParams
    {
		public string Root { get; set; }
		public string ChordType { get; set; }
        public string[] AllRoots { get; set; }
        public string[] AllChordTypes { get; set; }
        public bool Partial { get; set; }
		public bool Special { get; set; }
        public bool Strict { get; set; }

        public ChordParams(NameValueCollection parameters)
        {
            bool part;
            bool spec;
            bool strict;

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
            if (!bool.TryParse(parameters["strict"], out strict))
			{
				strict = false;
			}
			Strict = strict;
			Root = parameters["root"];
            ChordType = parameters["type"];
            AllRoots = parameters["roots"]?.Split(',') ?? new string[0];
            AllChordTypes = parameters["types"]?.Split(',') ?? new string[0];
		}
	}
}
