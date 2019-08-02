using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using PassWinmenu.Configuration;

namespace PassWinmenu.PasswordGeneration
{
	internal class PasswordGenerator : IDisposable
	{
		public PasswordGenerationConfig Options { get; }

		private readonly RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();

		public PasswordGenerator(PasswordGenerationConfig options)
		{
			Options = options;
		}

		public string GeneratePassword(int length)
		{
			if (!Options.CharacterGroups.Any(g => g.Enabled)) return null;

			// Build a complete set of all characters in all enabled groups
			var completeCharSet = new HashSet<int>();
			foreach (var group in Options.CharacterGroups.Where(g => g.Enabled))
			{
				completeCharSet.UnionWith(group.CharacterSet);
			}

			// Transform the set into a list, to assign an index to each character.
			var charList = completeCharSet.ToList();

			// Generate as many random list indices as we need to build a password.
			var indices = GetIntegers(charList.Count, length);

			// Transform the list of indices into a list of characters.
			var characters = indices.Select(i => charList[i]).ToArray();

			var password = string.Join("", characters.Select(char.ConvertFromUtf32));
			return password;
		}


		/// <summary>
		/// Generates a list of cryptographically secure randomly generated integers.
		/// </summary>
		private IEnumerable<int> GetIntegers(int max, int count)
		{
			for (var i = 0; i < count; i++)
			{
				yield return GetRandomInteger(max);
			}
		}

		/// <summary>
		/// Generates a cryptographically secure random number less than the given maximum value.
		/// </summary>
		private int GetRandomInteger(int maxValue)
		{
			// Generate a random uint64 using 8 random bytes.
			var bytes = new byte[8];
			csprng.GetBytes(bytes);
			var randomNumber = BitConverter.ToUInt64(bytes, 0);

			// Convert the random integer into a fraction of its maximum possible value.
			var fraction = randomNumber / (double)ulong.MaxValue;

			
			return (int)(fraction * maxValue);
		}

		public void Dispose()
		{
			csprng.Dispose();
		}
	}
}
