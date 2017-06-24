using System;
using Chords.Attributes;

namespace Chords.Models
{
    public enum Accidental
    {
        [Descriptor("")]
        Natural,
        [Descriptor("#")]
        Sharp,
        [Descriptor("b")]
        Flat,
        [Descriptor("x")]
        DoubleSharp,
        [Descriptor("bb")]
        DoubleFlat
    }
}
