using System;
using System.Collections.Specialized;

namespace Chords.Android.Models
{
    public class ShowChordResultModel
    {
        public NameValueCollection Parameters { get; set; }
        public ChordDecorator ChordDecorator { get; set; }
		public string Error { get; set; }
	}
}
