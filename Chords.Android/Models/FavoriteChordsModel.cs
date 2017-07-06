using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Chords.Android.Models;
using Chords.Core.Models;

namespace Chords.Android.Models
{
    public class FavoriteChordsModel
    {
        public NamingConvention conv { get; set; }
        public IEnumerable<KeyValuePair<ChordDecorator, GuitarChordLayoutDecorator[]>> Chords { get; set; }
    }
}
