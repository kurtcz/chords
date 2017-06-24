using System;
using Chords.Core.Attributes;

namespace Chords.Core.Models
{
	public enum Tone
	{
		[Descriptor("C", "C", "Do")]
		C = 1,
		[Descriptor("D", "D", "Re")]
		D = 3,
		[Descriptor("E", "E", "Mi")]
		E = 5,
		[Descriptor("F", "F", "Fa")]
		F = 6,
		[Descriptor("G", "G", "Sol")]
		G = 8,
		[Descriptor("A", "A", "La")]
		A = 10,
		[Descriptor("B", "H", "Si")]
		B = 12
	}
}
