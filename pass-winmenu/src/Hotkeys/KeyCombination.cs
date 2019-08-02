using System;
using System.Linq;
using System.Windows.Input;

namespace PassWinmenu.Hotkeys
{
	internal struct KeyCombination
	{
		public ModifierKeys ModifierKeys;
		public Key Key;

		public override string ToString()
		{
			return ModifierKeys + ", " + Key;
		}

		/// <summary>
		/// Parse a key combination string into a KeyCombination object.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static KeyCombination Parse(string str)
		{
			// Split a whitespace-delimited list of keys and normalise key names.
			var combination = str.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(key => key.Trim())
				.Select(key => key.ToLower())
				.Select(key => key == "ctrl" ? "control" : key)
				.Select(key => (key == "win" || key == "super") ? "windows" : key);

			var mods = ModifierKeys.None;
			var keys = Key.None;
			foreach (var keyStr in combination)
			{
				ModifierKeys parsedMod;
				Key parsedKey;
				if (Enum.TryParse(keyStr, true, out parsedMod))
				{
					mods |= parsedMod;
				}
				else if (Enum.TryParse(keyStr, true, out parsedKey))
				{
					if (keys == Key.None)
						keys = parsedKey;
					else
						throw new ArgumentException("A hotkey may not consist of multiple non-modifier keys.");
				}
				else
				{
					throw new ArgumentException($"Invalid key name: '{keyStr}'");
				}
			}
			return new KeyCombination { ModifierKeys = mods, Key = keys };
		}
	}
}