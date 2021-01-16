using System;
using System.Collections.Generic;
using System.Linq;
using McSherry.SemanticVersioning;
using PassWinmenu.Configuration;
using PassWinmenu.UpdateChecking.Chocolatey;
using PassWinmenu.UpdateChecking.Dummy;
using PassWinmenu.UpdateChecking.GitHub;
using PassWinmenu.WinApi;

namespace PassWinmenu.UpdateChecking
{
	static class UpdateCheckerFactory
	{
		public static UpdateChecker CreateUpdateChecker(UpdateCheckingConfig updateCfg, INotificationService notificationService)
		{
			IUpdateSource updateSource;
			switch (updateCfg.UpdateSource)
			{
				case UpdateSource.GitHub:
					updateSource = new GitHubUpdateSource();
					break;
				case UpdateSource.Chocolatey:
					updateSource = new ChocolateyUpdateSource();
					break;
				case UpdateSource.Dummy:
					updateSource = new DummyUpdateSource
					{
						Versions = new List<ProgramVersion>
						{
							new ProgramVersion
							{
								VersionNumber = new SemanticVersion(10, 0, 0),
								Important = true,
							},
							new ProgramVersion
							{
								VersionNumber = SemanticVersion.Parse("v11.0-pre1", ParseMode.Lenient),
								IsPrerelease = true,
							},
						}
					};
					break;
				default:
					throw new ArgumentOutOfRangeException(null, "Invalid update provider.");
			}
			var versionString = Program.Version.Split('-').First();

			var updateChecker = new UpdateChecker(updateSource,
			                                  SemanticVersion.Parse(versionString, ParseMode.Lenient),
			                                  updateCfg.AllowPrereleases,
			                                  updateCfg.CheckIntervalTimeSpan,
			                                  updateCfg.InitialDelayTimeSpan);

			updateChecker.UpdateAvailable += (sender, args) =>
			{
				notificationService.HandleUpdateAvailable(args);
			};
			return updateChecker;
		}
	}
}
