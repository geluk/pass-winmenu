using System.Linq;
using System.Windows.Forms;

namespace PassWinmenu.WinApi
{
	static class KeyboardEmulator
	{
		private static readonly SendKeysEscapeGenerator escapeGenerator = new SendKeysEscapeGenerator();

		/// <summary>
		/// Sends text directly to the topmost window, as if it was entered by the user.
		/// This method automatically escapes all characters with special meaning, 
		/// then calls SendKeys.Send().
		/// </summary>
		/// <param name="text">The text to be sent to the active window.</param>
		/// <param name="escapeDeadKeys">Whether dead keys should be escaped or not. 
		/// If true, inserts a space after every dead key in order to prevent it from being combined with the next character.</param>
		internal static void EnterText(string text, bool escapeDeadKeys)
		{
			var escaped = escapeGenerator.Escape(text, escapeDeadKeys);
			SendKeys.SendWait(escaped);
		}

		internal static void EnterRawText(string text)
		{
			SendKeys.SendWait(text);
		}
	}
}
