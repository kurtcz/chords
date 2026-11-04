using System;
using Chords.Core.Attributes;

namespace Chords.Core.Models
{
    public enum Accidental
    {
		[Descriptor("bb")]
		DoubleFlat = -2,
		[Descriptor("b")]
		Flat = -1,
		[Descriptor("")]
        Natural = 0,
        [Descriptor("#")]
        Sharp = 1,
        [Descriptor("x")]
        DoubleSharp = 2
    }
}
