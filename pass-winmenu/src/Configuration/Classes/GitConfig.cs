using System;
using PassWinmenu.Utilities;
using PassWinmenu.Utilities.ExtensionMethods;
using YamlDotNet.Serialization;

namespace PassWinmenu.Configuration
{
	internal class GitConfig
	{
		public bool UseGit { get; set; } = true;

		[YamlIgnore]
		public SyncMode SyncMode => (SyncMode)Enum.Parse(typeof(SyncMode), SyncModeString.ToPascalCase(), true);
		[YamlMember(Alias = "sync-mode")]
		public string SyncModeString { get; set; } = "builtin";

		private string gitPath = @"git";
		public string GitPath
		{
			get => gitPath;
			set
			{
				if (value == null) gitPath = null;
				else
				{
					var expanded = Environment.ExpandEnvironmentVariables(value);
					gitPath = Helpers.NormaliseDirectory(expanded);
				}
			}
		}

		public string SshPath { get; set; } = null;
	}
}
