using System;

namespace PassWinmenu.Hotkeys
{
	[Serializable]
	internal class HotkeyException : Exception
	{
		public HotkeyException(string message) : base(message) { }
		public HotkeyException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}
