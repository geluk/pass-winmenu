namespace PassWinmenu.Configuration
{
	internal class RuntimeConfiguration
	{
		public string ConfigFileLocation { get; private set; }
		
		private RuntimeConfiguration()
		{

		}

		internal static RuntimeConfiguration Parse(string[] args)
		{
			var configuration = new RuntimeConfiguration();

			if (args.Length > 1)
			{
				if (args.Length == 3 && args[1] == "--config-file")
				{
					configuration.ConfigFileLocation = args[2];
				}
				else
				{
					throw new RuntimeConfigurationError($"Invalid argument: {args[1]}");
				}
			}


			return configuration;
		}

	}
}
