using System;

namespace PassWinmenu.UpdateChecking
{
	public class UpdateException : Exception
	{
		public UpdateException(string message) : base(message) { }

		public UpdateException(string message, Exception innerException) : base(message, innerException) { }
	}
}
