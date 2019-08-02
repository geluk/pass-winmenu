using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using McSherry.SemanticVersioning;

namespace PassWinmenu.UpdateChecking
{
	internal class UpdateChecker : IDisposable
	{

		public IUpdateSource UpdateSource { get; }
		public SemanticVersion CurrentVersion { get; }
		public ProgramVersion? LatestVersion { get; private set; }
		public TimeSpan CheckInterval { get; set; }
		public TimeSpan InitialDelay { get; set; }
		public bool AllowPrerelease { get; set; }

		public event EventHandler<UpdateAvailableEventArgs> UpdateAvailable;

		private Timer timer;

		public UpdateChecker(IUpdateSource   updateSource,
		                     SemanticVersion currentVersion,
		                     bool            allowPrerelease,
		                     TimeSpan        checkInterval,
		                     TimeSpan        initialDelay)
		{
			UpdateSource = updateSource;
			CurrentVersion = currentVersion;
			AllowPrerelease = allowPrerelease;
			CheckInterval = checkInterval;
			InitialDelay = initialDelay;
		}

		public UpdateChecker(IUpdateSource   updateSource,
		                     SemanticVersion currentVersion,
		                     bool            allowPrerelease,
		                     TimeSpan        checkInterval)
			: this(updateSource, currentVersion, allowPrerelease, checkInterval, checkInterval)
		{
		}


		/// <summary>
		/// Starts checking for updates.
		/// </summary>
		public void Start()
		{
			timer = new Timer(InitialDelay.TotalMilliseconds)
			{
				AutoReset = true,
			};

			timer.Elapsed += (sender, args) =>
			{
				// Update the timer interval so it takes the value from
				// CheckInterval instead of InitialDelay after it has 
				// elapsed at least once.
				timer.Interval = CheckInterval.TotalMilliseconds;
				CheckForUpdates();
			};
			timer.Start();
		}

		/// <summary>
		/// Checks for any available updates, raising <see cref="UpdateAvailable"/> if an update is found.
		/// </summary>
		public bool CheckForUpdates()
		{
			Log.Send("Checking for available updates...", LogLevel.Debug);
			var update = GetUpdate();
			if (update == null)
			{
				Log.Send($"No update found (latest version is {LatestVersion}).", LogLevel.Debug);
				return false;
			}

			// Stop automatic update checking if we've found an update.
			timer.Stop();
			timer.Dispose();
			Log.Send($"New update found: {update.Value.VersionNumber} - prerelease: {update.Value.IsPrerelease} - important: {update.Value.Important}", LogLevel.Debug);
			NotifyUpdate(update.Value);
			return true;
		}


		/// <summary>
		/// Checks for an available update.
		/// </summary>
		/// <returns>
		/// A <see cref="ProgramVersion"/> representing the available update,
		/// or <value>nul</value> if no update is available.
		/// </returns>
		private ProgramVersion? GetUpdate()
		{
			if (UpdateSource.RequiresConnectivity && !GetConnectivity())
			{
				return null;
			}

			LatestVersion = UpdateSource.GetLatestVersion(AllowPrerelease);
			if (LatestVersion == null) return null;

			if (LatestVersion.Value.VersionNumber > CurrentVersion)
			{
				return LatestVersion;
			}
			return null;
		}

		/// <summary>
		/// Makes a lightweight HTTP request to Google to check whether an internet connection is available.
		/// </summary>
		private bool GetConnectivity()
		{
			try
			{
				using (var client = new WebClient())
				using (client.OpenRead("http://clients3.google.com/generate_204"))
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Dispatches the <see cref="UpdateAvailable"/> event to notify listeners that an update is available.
		/// </summary>
		/// <param name="version">A <see cref="ProgramVersion"/> representing the new version.</param>
		private void NotifyUpdate(ProgramVersion version)
		{
			UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(version));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			timer?.Dispose();
		}
	}

	internal class UpdateAvailableEventArgs : EventArgs
	{
		public ProgramVersion Version { get; set; }

		public UpdateAvailableEventArgs(ProgramVersion version)
		{
			Version = version;
		}
	}
}
