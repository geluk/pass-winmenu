using System;

namespace PassWinmenu.WinApi
{
	[Serializable]
	public class ExecutableNotFoundException : Exception
	{
		public ExecutableNotFoundException(string message) : base(message) { }

		public ExecutableNotFoundException() { }
	}
}
