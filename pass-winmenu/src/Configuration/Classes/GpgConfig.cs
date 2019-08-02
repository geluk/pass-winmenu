using System;
using PassWinmenu.Utilities;

namespace PassWinmenu.Configuration
{
	internal class GpgConfig
	{
		private string gpgPath = @"C:\Program Files (x86)\GnuPG\bin";
		public string GpgPath
		{
			get => gpgPath;
			set
			{
				if (value == null) gpgPath = null;
				else
				{
					var expanded = Environment.ExpandEnvironmentVariables(value);
					gpgPath = Helpers.NormaliseDirectory(expanded);
				}
			}
		}

		private string gnupghomeOverride;
		public string GnupghomeOverride
		{
			get => gnupghomeOverride;
			set
			{
				if (value == null) gnupghomeOverride = null;
				else
				{
					var expanded = Environment.ExpandEnvironmentVariables(value);
					gnupghomeOverride = Helpers.NormaliseDirectory(expanded);
				}

			}
		}

		public bool PinentryFix { get; set; } = false;

		public GpgAgentConfig GpgAgent { get; set; } = new GpgAgentConfig();
	}
}
