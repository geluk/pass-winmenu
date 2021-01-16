using System;

namespace PassWinmenu.WinApi
{
	public class ExecutableNotFoundException : Exception
	{
		public ExecutableNotFoundException(string message) : base(message) { }
	}
}

