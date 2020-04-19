using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassWinmenu.UpdateChecking
{
	public class UpdateException : Exception
	{
		public UpdateException(string message) : base(message) { }

		public UpdateException(string message, Exception innerException) : base(message, innerException) { }
	}
}
