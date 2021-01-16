using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PassWinmenu.Utilities.ExtensionMethods
{
	internal static class StringExtensions
	{
		private static string ToCamelOrPascalCase(string str, Func<char, char> firstLetterTransform)
		{
			if (string.IsNullOrWhiteSpace(str)) return str;
			var input = str;
			var pattern = "([_\\-])(?<char>[a-z])";
			var num = 1;
			var str1 = Regex.Replace(input, pattern, (MatchEvaluator)(match => match.Groups["char"].Value.ToUpperInvariant()), (RegexOptions)num);
			return firstLetterTransform(str1[0]).ToString(CultureInfo.InvariantCulture) + str1.Substring(1);
		}

		/// <summary>
		/// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to
		/// pascal case (ThisIsATest). Pascal case is the same as camel case, except the first letter
		/// is uppercase.
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <returns>
		/// Converted string
		/// </returns>
		public static string ToPascalCase(this string str)
		{
			return ToCamelOrPascalCase(str, char.ToUpperInvariant);
		}

		/// <summary>
		/// Extracts the Unicode code points from a string.
		/// </summary>
		/// <param name="str">The string from which the code points should be extracted.</param>
		/// <returns>An integer array representing the discovered code points.</returns>
		public static int[] ToCodePoints(this string str)
		{
			// TODO: test handling of surrogate pairs
			if (str == null) throw new ArgumentNullException(nameof(str));

			if (!str.IsNormalized())
			{
				str = str.Normalize();
			}

			var codePoints = new List<int>();
			for (var i = 0; i < str.Length; i++)
			{
				codePoints.Add(char.ConvertToUtf32(str, i));
				if (char.IsHighSurrogate(str[i]))
					i += 1;
			}

			return codePoints.ToArray();
		}

		public static string RemoveEnd(this string current, string toRemove)
		{
			if (!current.EndsWith(toRemove))
			{
				throw new ArgumentException("The given string does not end with the string to be removed.");
			}

			return current.Substring(0, current.Length - toRemove.Length);
		}
	}
}
