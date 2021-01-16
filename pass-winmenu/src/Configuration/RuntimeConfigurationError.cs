using System;

namespace PassWinmenu.Configuration
{
	public class RuntimeConfigurationError : Exception
	{
		public RuntimeConfigurationError(string message) : base(message)
		{
		}
	}
}
