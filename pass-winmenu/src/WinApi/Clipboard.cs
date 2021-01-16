using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using PassWinmenu.Configuration;
using PassWinmenu.Utilities;

namespace PassWinmenu.WinApi
{
	public static class ClipboardHelper
	{
		/// <summary>
		/// Copies a string to the clipboard. If it still exists on the clipboard after the amount of time
		/// specified in <paramref name="timeout"/>, it will be removed again.
		/// </summary>
		/// <param name="text">The text to add to the clipboard.</param>
		/// <param name="timeout">The amount of time, in seconds, the text should remain on the clipboard.</param>
		public static void Place(string text, TimeSpan timeout)
		{
			Helpers.AssertOnUiThread();

			var clipboardBackup = MakeClipboardBackup();
			Clipboard.SetDataObject(text);

			Task.Delay(timeout).ContinueWith(_ =>
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					try
					{
						// Only reset the clipboard to its previous contents if it still contains the text we copied to it.
						if (!Clipboard.ContainsText() || Clipboard.GetText() != text) return;

						Clipboard.Clear();

						if (!ConfigManager.Config.Interface.RestoreClipboard) return;

						// Create a new DataObject into which we can restore our data.
						var dataObject = new DataObject();
						Log.Send($"Restoring previous clipboard contents:");
						foreach (var pair in clipboardBackup)
						{
							Log.Send($" - {pair.Key}");
							dataObject.SetData(pair.Key, pair.Value);
						}

						// Now place it on the clipboard.
						Clipboard.SetDataObject(dataObject, true);
					}
					catch (Exception e)
					{
						Log.Send($"Failed to restore previous clipboard contents: An exception occurred ({e.GetType().Name}: {e.Message})", LogLevel.Error);
					}
				});
			});
		}

		/// <summary>
		/// Backs up the current clipboard data to a dictionary mapping data formats to contents.
		/// </summary>
		private static Dictionary<string, object> MakeClipboardBackup()
		{
			var clipboardBackup = new Dictionary<string, object>();
			var dataObject = Clipboard.GetDataObject();
			if (dataObject == null)
			{
				return clipboardBackup;
			}
			Log.Send("Creating clipboard backup.");
			var formats = dataObject.GetFormats(false);
			Log.Send($" - Formats: {string.Join(", ", formats)}");
			foreach (var format in formats)
			{
				try
				{
					clipboardBackup[format] = dataObject.GetData(format, false);
				}
				catch (Exception e)
				{
					Log.Send($"Couldn't store format \"{format}\": {e.GetType().Name} ({e.Message})", LogLevel.Warning);
				}
			}
			return clipboardBackup;
		}


		public static string GetText()
		{
			return Clipboard.ContainsText() ? Clipboard.GetText() : null;
		}
	}
}
