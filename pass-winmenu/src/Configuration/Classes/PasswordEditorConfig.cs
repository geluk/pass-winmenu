using System;
using System.IO;
using PassWinmenu.Utilities;

namespace PassWinmenu.Configuration
{
	internal class PasswordEditorConfig
	{
		public bool UseBuiltin { get; set; } = true;

		private string temporaryFileDirectory = Environment.ExpandEnvironmentVariables(@"%temp%");
		public string TemporaryFileDirectory
		{
			get => temporaryFileDirectory;
			set
			{
				var expanded = Environment.ExpandEnvironmentVariables(value);
				temporaryFileDirectory = Path.GetFullPath(Helpers.NormaliseDirectory(expanded));
			}
		}
	}
}
