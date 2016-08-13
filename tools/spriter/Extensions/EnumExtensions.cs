using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Spriter.Extensions {
	public static class EnumExtensions {
		public static string GetDisplayDescription(this Enum en) {
			var enumString = en.ToString();
			var info = en.GetType().GetMember(enumString);
			if (!info.Any()) return enumString;
			var attrs = info.First().GetCustomAttributes<DisplayAttribute>();
			return attrs.Any()
				? attrs.First().Description
				: enumString
			;
		}
	}
}