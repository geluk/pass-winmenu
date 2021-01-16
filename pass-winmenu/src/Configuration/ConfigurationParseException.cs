using System;

namespace PassWinmenu.Configuration
{
	internal class ConfigurationParseException : ConfigurationException
	{
		public ConfigurationParseException(string message, Exception innerException) : base(message, innerException) { }
		
		public ConfigurationParseException(string message) : base(message) { }
	}
}
