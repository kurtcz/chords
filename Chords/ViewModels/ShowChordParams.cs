using System;
using Chords.Models;

namespace Chords.ViewModels
{
    public class ShowChordParams
    {
        public string root { get; set; }
        public string @type { get; set; }
        public bool @partial { get; set; }
        public bool special { get; set; }
        public NamingConvention conv { get; set; }

        public override string ToString()
        {
            return string.Format("[root={0}, type={1}, partial={2}, special={3}, conv={4}]", root, @type, @partial, special, conv);
        }
    }
}
