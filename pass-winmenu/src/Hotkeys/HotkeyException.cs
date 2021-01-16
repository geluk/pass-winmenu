using System;

namespace PassWinmenu.Hotkeys
{
	public class HotkeyException : Exception
	{
		public HotkeyException(string message) : base(message) { }
		public HotkeyException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}
