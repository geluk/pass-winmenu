using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal class GpgTransport : IGpgTransport
	{
		private const string StatusMarker = "[GNUPG:] ";

		private readonly TimeSpan gpgCallTimeout = TimeSpan.FromSeconds(5);
		private readonly IGpgHomedirResolver homedirResolver;
		private readonly GpgInstallation installation;
		private readonly IProcesses processes;

		public GpgTransport(IGpgHomedirResolver homedirResolver, GpgInstallation installation, IProcesses processes)
		{
			this.homedirResolver = homedirResolver;
			this.installation = installation;
			this.processes = processes;
		}

		public GpgResult CallGpg(string arguments, string input = null)
		{
			var gpgProc = CreateGpgProcess(arguments, input);
			gpgProc.WaitForExit(gpgCallTimeout);

			string stderrLine;
			var stderrMessages = new List<string>();
			var statusMessages = new List<StatusMessage>();
			while ((stderrLine = gpgProc.StandardError.ReadLine()) != null)
			{
				Log.Send($"[GPG]: {stderrLine}");
				if (stderrLine.StartsWith(StatusMarker, StringComparison.Ordinal))
				{
					// This line is a status line, so extract status information from it.
					var statusLine = stderrLine.Substring(StatusMarker.Length);
					var spaceIndex = statusLine.IndexOf(" ", StringComparison.Ordinal);
					if (spaceIndex == -1)
					{
						statusMessages.Add(new StatusMessage(statusLine, null));
					}
					else
					{
						var statusLabel = statusLine.Substring(0, spaceIndex);
						// Length+1 because the space after the status label should be skipped.
						var statusMessage = statusLine.Substring(statusLabel.Length + 1);
						statusMessages.Add(new StatusMessage(statusLabel, statusMessage));
					}
				}
				else
				{
					stderrMessages.Add(stderrLine);
				}
			}

			string output;
			// We can use the standard UTF-8 encoding here, as it should be able to handle input without BOM.
			using (var reader = new StreamReader(gpgProc.StandardOutput.BaseStream, Encoding.UTF8))
			{
				output = reader.ReadToEnd();
			}

			return new GpgResult(gpgProc.ExitCode, output, statusMessages, stderrMessages);
		}

		/// <summary>
		/// Spawns a GPG process.
		/// </summary>
		private IProcess CreateGpgProcess(string arguments, string input = null)
		{
			Log.Send($"Calling GPG with \"{arguments}\"");
			// Only redirect stdin if we're going to send anything to it.
			var psi = CreateGpgProcessStartInfo(arguments, input != null);

			var gpgProc = processes.Start(psi);
			if (input != null)
			{
				// Explicitly define the encoding to not send a BOM, to ensure other platforms can handle our output.
				using (var writer = new StreamWriter(gpgProc.StandardInput.BaseStream, new UTF8Encoding(false)))
				{
					writer.Write(input);
				}
			}
			return gpgProc;
		}

		/// <summary>
		/// Generates a ProcessStartInfo object that can be used to spawn a GPG process.
		/// </summary>
		private ProcessStartInfo CreateGpgProcessStartInfo(string arguments, bool redirectStdin)
		{
			// Maybe use --display-charset utf-8?
			var argList = new List<string>
			{
				"--batch", // Ensure GPG does not ask for input or user action
				"--no-tty", // Let GPG know we're not a TTY
				"--status-fd 2", // Write status messages to stderr
				"--with-colons", // Use colon notation for displaying keys
				"--exit-on-status-write-error", //  Exit if status messages cannot be written
			};
			var homedirOverride = homedirResolver.GetConfiguredHomeDir();
			if (homedirOverride != null)
			{
				argList.Add($"--homedir \"{homedirOverride}\"");
			}

			var psi = new ProcessStartInfo
			{
				FileName = installation.GpgExecutable.FullName,
				Arguments = $"{string.Join(" ", argList)} {arguments}",
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = redirectStdin,
				CreateNoWindow = true
			};
			return psi;
		}

	}
}
