using System;
using Chords.Core.Attributes;

namespace Chords.Core.Models
{
    public enum GuitarChordType
    {
        [Descriptor("Six string chord")]
        SixString,
        [Descriptor("Five string chord")]
        FiveString,
        [Descriptor("Four string chord")]
        FourString,
        [Descriptor("Barre chord")]
        SixStringBarre,
        [Descriptor("Small barre chord")]
        FiveStringBarre,
        [Descriptor("Special chord")]
        Special
    }
}
