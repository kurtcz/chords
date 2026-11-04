using System;
using System.Linq;
using Chords.Core.Extensions;

namespace Chords.Core.Models
{
    public class Scale
    {
        public Note Root { get; }
        public ScaleType ScaleType { get; }
        public Note[] Notes { get; }
 
        public Scale(Note root, ScaleType scaleType)
        {
            Root = root;
            ScaleType = scaleType;
            Notes = scaleType.ToIntervals()
                             .Select(i => root.NoteAtInterval(i))
                             .ToArray();
        }
    }
}
