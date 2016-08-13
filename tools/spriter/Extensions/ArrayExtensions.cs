using System;

namespace Spriter.Extensions {
	public static class ArrayExtensions {
		public static T[] Reverse<T>(this T[] array) {
			Array.Reverse(array);
			return array;
		}
	}
}