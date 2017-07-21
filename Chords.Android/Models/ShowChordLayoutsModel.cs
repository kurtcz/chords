using System;
namespace Chords.Android.Models
{
    public class ShowChordLayoutsModel : ShowChordResultModel
    {
        public bool ShowTips { get; set; }
		public GuitarChordLayoutDecorator[] Layouts { get; set; }
	}
}
