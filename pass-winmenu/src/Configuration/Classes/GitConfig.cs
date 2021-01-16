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
		public string SyncModeString { get; set; } = "auto";

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
		public bool AutoFetch { get; set; } = true;
		public double AutoFetchInterval { get; set; } = 3600;
		[YamlIgnore]
		public TimeSpan AutoFetchIntervalTimeSpan => TimeSpan.FromSeconds(AutoFetchInterval);

	}
}
