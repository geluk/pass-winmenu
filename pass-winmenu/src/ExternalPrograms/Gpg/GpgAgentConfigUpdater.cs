using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal class GpgAgentConfigUpdater
	{
		public const string ManagedByPassWinmenuComment = "# This configuration key is automatically managed by pass-winmenu";

		private readonly IGpgAgentConfigReader reader;

		public GpgAgentConfigUpdater(IGpgAgentConfigReader reader)
		{
			this.reader = reader;
		}

		/// <summary>
		/// Update the gpg-agent config file with the given keys.
		/// </summary>
		public void UpdateAgentConfig(Dictionary<string, string> keys)
		{
			string[] lines;
			try
			{
				lines = reader.ReadConfigLines();
			}
			catch (Exception e)
			{
				Log.Send("Could not read agent config file. Updating it will not be possible.", LogLevel.Warning);
				Log.ReportException(e);
				return;
			}

			var newLines = UpdateAgentConfigKeyCollection(lines, keys.ToList()).ToArray();

			if (lines.SequenceEqual(newLines))
			{
				Log.Send("GPG agent config file already contains the correct settings; it'll be left untouched.");
				return;
			}

			Log.Send($"Modifying GPG agent config file ({string.Join(", ", keys.Keys)})");
			try
			{
				reader.WriteConfigLines(newLines);
			}
			catch (Exception e)
			{
				Log.Send("Could not update agent config file.", LogLevel.Warning);
				Log.ReportException(e);
			}
		}

		/// <summary>
		/// Iterates over a list of config lines, adding or replacing the given config keys.
		/// </summary>
		private IEnumerable<string> UpdateAgentConfigKeyCollection(string[] existingLines, List<KeyValuePair<string, string>> pairsToUpdate)
		{
			var configKeyRegex = new Regex(@"^(\s*([^#^\s][^\s]*)\s+)(.*)$");
			for (var i = 0; i < existingLines.Length; i++)
			{
				var line = existingLines[i];
				var match = configKeyRegex.Match(line);
				if (!match.Success)
				{
					// This line does not look like a config key, best not touch it.
					yield return line;
					continue;
				}

				// Line looks like a a configuration pair, let's check if we want to do something with it.
				var key = match.Groups[2].Value;
				var value = match.Groups[3].Value;
				var pairToSet = pairsToUpdate.FirstOrDefault(k => k.Key == key);
				if (pairToSet.Key == null)
				{
					// We don't recognise the key in this pair, so no need to change it.
					yield return line;
					continue;
				}

				// This pair may need its value updated, so remove it.
				pairsToUpdate.RemoveAll(k => k.Key == key);

				if (pairToSet.Value == value)
				{
					// The value is already correct, no need to do anything.
					yield return line;
					continue;
				}

				// Insert a comment explaining that we're managing this key,
				// unless such a comment already exists.
				if (i == 0 || existingLines[i - 1] != ManagedByPassWinmenuComment)
				{
					yield return ManagedByPassWinmenuComment;
				}

				// Now return the updated key-value pair.
				yield return $"{pairToSet.Key} {pairToSet.Value}";
			}

			while (pairsToUpdate.Any())
			{
				// Looks like some of the keys we need to set aren't in the config file yet, so let's add them.
				var next = pairsToUpdate[0];
				pairsToUpdate.RemoveAt(0);
				yield return ManagedByPassWinmenuComment;
				yield return $"{next.Key} {next.Value}";
			}
		}
	}
}
