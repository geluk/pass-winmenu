using System;

namespace PassWinmenu.Configuration
{
	[Serializable]
	internal class ConfigurationParseException : ConfigurationException
	{
		public ConfigurationParseException(string message, Exception innerException) : base(message, innerException) { }
		public ConfigurationParseException(string message) : base(message) { }
	}
}
