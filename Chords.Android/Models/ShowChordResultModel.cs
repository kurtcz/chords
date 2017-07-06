using System;
using System.Collections.Specialized;
using Chords.Core.Models;

namespace Chords.Android.Models
{
    public class ShowChordResultModel
    {
        public NameValueCollection Parameters { get; set; }
        public NamingConvention conv { get; set; }
        public ChordDecorator ChordDecorator { get; set; }
        public Note Root { get; set; }
        public string ChordType { get; set; }
        public Note[] AllRoots { get; set; }
        public string[] AllChordTypes { get; set; }
        public bool AllowPartial { get; set; }
        public bool AllowSpecial { get; set; }
		public string Error { get; set; }

        public void Populate(NameValueCollection parameters)
        {
            var b = false;

            if(bool.TryParse(parameters["partial"], out b))
            {
				AllowPartial = b;
			}

			if (!bool.TryParse(parameters["special"], out b))
			{
                b = false;
			}
            AllowSpecial = b;
		}
    }
}
