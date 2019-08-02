using System;
using System.IO;
using PassWinmenu.Utilities;

namespace PassWinmenu.Configuration
{
	internal class PasswordStoreConfig
	{
		private string location = Environment.ExpandEnvironmentVariables(@"%userprofile%\.password-store");
		public string Location
		{
			get => location;
			set
			{
				var expanded = Environment.ExpandEnvironmentVariables(value);
				location = Path.GetFullPath(Helpers.NormaliseDirectory(expanded));
			}
		}

		public string PasswordFileMatch { get; set; } = ".*\\.gpg$";

		public bool FirstLineOnly { get; set; } = true;

		public PasswordGenerationConfig PasswordGeneration { get; set; } = new PasswordGenerationConfig();
		public UsernameDetectionConfig UsernameDetection { get; set; } = new UsernameDetectionConfig();
	}
}
