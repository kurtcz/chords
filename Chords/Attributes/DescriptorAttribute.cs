using System;

namespace Chords.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
	public class DescriptorAttribute : Attribute
	{
        public string[] Descriptions { get; set; }

		public DescriptorAttribute(params string[] descriptions)
		{
            this.Descriptions = descriptions;
		}
	}
}
