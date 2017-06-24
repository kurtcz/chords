using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chords.Attributes;

namespace Chords.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum obj, bool returnNullIfNoOrEmptyDescription = false)
        {
            var descriptor = GetDescriptor(obj);

            if (descriptor == null || descriptor.Descriptions.All(i => string.IsNullOrEmpty(i)))
            {
                return returnNullIfNoOrEmptyDescription ? null : obj.ToString();
            }
            return descriptor.Descriptions.First();
        }

        public static IEnumerable<string> GetDescriptions(this Enum obj)
        {
			var descriptor = GetDescriptor(obj);

			return descriptor?.Descriptions ?? Enumerable.Empty<string>();
		}

		private static DescriptorAttribute GetDescriptor(Enum obj)
		{
			return obj.GetType()
                      .GetMember(obj.ToString())[0]
                      .GetCustomAttributes<DescriptorAttribute>(false)
                      .FirstOrDefault();
		}
	}
}
