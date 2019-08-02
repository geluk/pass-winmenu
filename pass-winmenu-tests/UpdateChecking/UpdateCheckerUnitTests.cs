using System;
using System.Collections.Generic;
using System.Threading;
using McSherry.SemanticVersioning;
using PassWinmenu.UpdateChecking;
using PassWinmenu.UpdateChecking.Dummy;
using PassWinmenuTests.Utilities;
using Xunit;

namespace PassWinmenuTests.UpdateChecking
{
		public class UpdateCheckerUnitTests
	{
		private const string Category = "Updates: update sources";

		[Fact, TestCategory(Category)]
		public void UpdateChecker_TriggersOnNewerVersion()
		{
			var raisesMajor = RaisesEvent(new SemanticVersion(1, 1, 1), new SemanticVersion(2, 0, 0));
			Assert.True(raisesMajor);
			var raisesMinor = RaisesEvent(new SemanticVersion(1, 1, 1), new SemanticVersion(1, 2, 0));
			Assert.True(raisesMinor);
			var raisesPatch = RaisesEvent(new SemanticVersion(1, 1, 1), new SemanticVersion(1, 1, 2));
			Assert.True(raisesPatch);
		}

		[Fact, TestCategory(Category)]
		public void UpdateChecker_IgnoresEqualVersion()
		{
			var raisesEqual = RaisesEvent(new SemanticVersion(1, 0, 0), new SemanticVersion(1, 0, 0));
			Assert.False(raisesEqual);
		}

		[Fact, TestCategory(Category)]
		public void UpdateChecker_IgnoresOlderVersion()
		{
			var raisesMajor = RaisesEvent(new SemanticVersion(2, 2, 2), new SemanticVersion(1, 3, 3));
			Assert.False(raisesMajor);
			var raisesMinor = RaisesEvent(new SemanticVersion(2, 2, 2), new SemanticVersion(2, 1, 3));
			Assert.False(raisesMinor);
			var raisesPatch = RaisesEvent(new SemanticVersion(2, 2, 2), new SemanticVersion(2, 2, 1));
			Assert.False(raisesPatch);
		}

		[Fact, TestCategory(Category)]
		public void UpdateChecker_RaisesAfterSpecifiedTime()
		{
			const int checkBeforeMs = 800;
			const int raiseAfterMs = 1000;
			const int checkAgainMs = 400;
			const int checkAttempts = 10;

			var source = new DummyUpdateSource
			{
				Versions = new List<ProgramVersion>
				{
					new ProgramVersion
					{
						VersionNumber = new SemanticVersion(1, 0, 1)
					}
				}
			};

			var checker = new UpdateChecker(source, new SemanticVersion(1, 0, 0), false, TimeSpan.FromMilliseconds(raiseAfterMs));

			var raised = false;
			checker.UpdateAvailable += (sender, args) => { raised = true; };
			checker.Start();

			// Validate that the event is not raised before the time specified in raiseAfterMs has expired.
			Thread.Sleep(checkBeforeMs);
			Assert.False(raised, "Notification was raised before update interval expired");

			// Validate that the event has been raised now.
			Thread.Sleep(checkAgainMs);
			for (int i = 0; i < checkAttempts && !raised; i++)
			{
				Thread.Sleep(checkAgainMs);
			}

			Assert.True(raised, "Notification was not raised");
		}


		[Fact, TestCategory(Category)]
		public void UpdateChecker_UpdatesPreReleases()
		{
			var current = SemanticVersion.Parse("2.0.0-pre2", ParseMode.Lenient);
			var sourceWithNewPrerelease = new DummyUpdateSource
			{
				Versions = new List<ProgramVersion>
				{
					new ProgramVersion
					{
						VersionNumber = SemanticVersion.Parse("2.0.0-pre2"),
						IsPrerelease = true
					},
					new ProgramVersion
					{
						VersionNumber = SemanticVersion.Parse("2.0.0-pre3"),
						IsPrerelease = true
					},
					// Versions may be specified in any order
					new ProgramVersion
					{
					VersionNumber = SemanticVersion.Parse("2.0.0-pre1"),
					IsPrerelease = true
					}
				}
			};

			using (var checker = new UpdateChecker(sourceWithNewPrerelease, current, true, TimeSpan.FromMilliseconds(1)))
			{
				AssertUpdateCheck(checker, (sender, args) =>
				{
					Assert.Equal(args.Version.VersionNumber, SemanticVersion.Parse("2.0.0-pre3", ParseMode.Lenient));
				});
			}

			// Ensure we get back the final release, even if it's specified as a pre-release.
			var sourceWithFinalRelease = new DummyUpdateSource
			{
				Versions = new List<ProgramVersion>
				{
					new ProgramVersion
					{
						VersionNumber = SemanticVersion.Parse("2.0.0-pre3"),
						IsPrerelease = true
					},
					new ProgramVersion
					{
						VersionNumber = SemanticVersion.Parse("2.0.0-pre2"),
						IsPrerelease = true
					},
					new ProgramVersion
					{
						VersionNumber = SemanticVersion.Parse("2.0.0-pre1"),
						IsPrerelease = true
					},
					new ProgramVersion
					{
						VersionNumber = SemanticVersion.Parse("2.0.0-pre4"),
						IsPrerelease = true
					},
					new ProgramVersion
					{
						VersionNumber = SemanticVersion.Parse("2.0.0"),
						IsPrerelease = true
					}
				}
			};

			using (var checker = new UpdateChecker(sourceWithFinalRelease, current, true, TimeSpan.FromMilliseconds(1)))
			{
				AssertUpdateCheck(checker, (sender, args) =>
				{
					Assert.Equal(args.Version.VersionNumber, SemanticVersion.Parse("2.0.0", ParseMode.Lenient));
				});
			}
		}

		[Fact, TestCategory(Category)]
		public void UpdateChecker_ProvidesCorrectReleaseType()
		{
			var sourceWithNewerPrerelease = new DummyUpdateSource
			{
				Versions = new List<ProgramVersion>
				{
					new ProgramVersion
					{
						VersionNumber = new SemanticVersion(3, 0, 0),
						IsPrerelease = true
					},
					new ProgramVersion
					{
						VersionNumber = new SemanticVersion(2, 0, 0),
					}
				}
			};
			var sourceWithOlderPrerelease = new DummyUpdateSource
			{
				Versions = new List<ProgramVersion>
				{
					new ProgramVersion
					{
						VersionNumber = new SemanticVersion(3, 0, 0),
					},
					new ProgramVersion
					{
						VersionNumber = new SemanticVersion(2, 0, 0),
						IsPrerelease = true
					},
					new ProgramVersion
					{
					VersionNumber = new SemanticVersion(1, 0, 0)
					}
				}
			};

			// An update checker that may not return prereleases should return the latest non-prerelease version.
			using (var checker = new UpdateChecker(sourceWithNewerPrerelease, new SemanticVersion(1, 0, 0), false, TimeSpan.FromMilliseconds(1)))
			{
				AssertUpdateCheck(checker, (sender, args) =>
				{
					Assert.False(args.Version.IsPrerelease);
					Assert.Equal(args.Version.VersionNumber, new SemanticVersion(2, 0, 0));
				});
			}

			// An update checker that may return prereleases should return a prerelease if it's the latest version.
			using (var checker = new UpdateChecker(sourceWithNewerPrerelease, new SemanticVersion(1, 0, 0), true, TimeSpan.FromMilliseconds(1)))
			{
				AssertUpdateCheck(checker, (sender, args) =>
				{
					Assert.True(args.Version.IsPrerelease);
					Assert.Equal(args.Version.VersionNumber, new SemanticVersion(3, 0, 0));
				});
			}


			// An update checker that may return prereleases should not return a prerelease if there's a newer general release.
			using (var checker = new UpdateChecker(sourceWithOlderPrerelease, new SemanticVersion(1, 0, 0), true, TimeSpan.FromMilliseconds(1)))
			{
				AssertUpdateCheck(checker, (sender, args) =>
				{
					Assert.False(args.Version.IsPrerelease);
					Assert.Equal(args.Version.VersionNumber, new SemanticVersion(3, 0, 0));
				});
			}
		}

		private void AssertUpdateCheck(UpdateChecker checker, EventHandler<UpdateAvailableEventArgs> handler)
		{

			var raised = false;
			checker.UpdateAvailable += handler;
			checker.UpdateAvailable += (sender, args) => raised = true;
			checker.Start();

			// Wait a while to see if the event is raised.
			for (var i = 0; i < 20; i++)
			{
				Thread.Sleep(50);
				if (raised) break;
			}

			Assert.True(raised, "Update checker did not raise event.");
		}

		/// <summary>
		/// Checks whether supplying a given update version against a given base version raises an update-available event.
		/// </summary>
		private bool RaisesEvent(SemanticVersion currentVersion, SemanticVersion updateVersion)
		{
			var source = new DummyUpdateSource
			{
				Versions = new List<ProgramVersion>
				{
					new ProgramVersion
					{
						VersionNumber = updateVersion
					}
				}
			};

			var checker = new UpdateChecker(source, currentVersion, false, TimeSpan.FromMilliseconds(1));

			var raised = false;
			checker.UpdateAvailable += (sender, args) =>
			{
				raised = true;
				checker.Dispose();
			};
			checker.Start();

			// Wait a while to see if the event is raised.
			for (var i = 0; i < 20; i++)
			{
				Thread.Sleep(50);
				if (raised) return true;
			}

			return raised;
		}
	}
}
