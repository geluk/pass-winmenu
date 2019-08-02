using System;
using YamlDotNet.Serialization;

namespace PassWinmenu.Configuration
{
	internal class UpdateCheckingConfig
	{
		public bool CheckForUpdates { get; set; } = true;
		public bool AllowPrereleases { get; set; } = false;
		public UpdateSource UpdateSource { get; set; } = UpdateSource.GitHub;
		public int CheckInterval { get; set; } = 3600;
		[YamlIgnore]
		public TimeSpan CheckIntervalTimeSpan => TimeSpan.FromSeconds(CheckInterval);
		public int InitialDelay { get; set; } = 3600;
		[YamlIgnore]
		public TimeSpan InitialDelayTimeSpan => TimeSpan.FromSeconds(InitialDelay);

	}
}
