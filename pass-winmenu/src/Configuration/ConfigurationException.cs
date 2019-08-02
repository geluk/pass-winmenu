using System;

namespace PassWinmenu.Configuration
{
	[Serializable]
	internal class ConfigurationException : Exception
	{
		public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
		public ConfigurationException(string message) : base(message) { }
	}
}
