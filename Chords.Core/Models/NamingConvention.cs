using System;
using Chords.Core.Attributes;

namespace Chords.Core.Models
{
	public enum NamingConvention
	{
        [Descriptor("English notation")]
		English,
        [Descriptor("German notation")]
		German,
        [Descriptor("Latin notation")]
		Latin
	}
}
