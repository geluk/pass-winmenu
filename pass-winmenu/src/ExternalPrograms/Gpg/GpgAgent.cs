using System;
using System.Diagnostics;
using System.Linq;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal class GpgAgent : IGpgAgent
	{
		private const string GpgAgentProcessName = "gpg-agent";

		private readonly TimeSpan agentConnectTimeout = TimeSpan.FromSeconds(2);
		private readonly TimeSpan agentReadyTimeout = TimeSpan.FromSeconds(3);
		private readonly IProcesses processes;
		private readonly GpgInstallation gpgInstallation;

		public GpgAgent(IProcesses processes, GpgInstallation gpgInstallation)
		{
			this.processes = processes;
			this.gpgInstallation = gpgInstallation;
		}

		public void EnsureAgentResponsive()
		{
			// In certain situations, gpg-agent may hang and become unresponsive to input.
			// This will cause any decryption attempts to hang indefinitely, without any indication of what's happening.
			// Since that's obviously not desirable, we need to check whether the gpg-agent is still responsive, and if not, kill and restart it.

			if (processes.GetProcessesByName(GpgAgentProcessName).Length == 0)
			{
				// If gpg-agent isn't running yet, it obviously can't be unresponsive, so we don't have to do anything here.
				return;
			}

			if (!gpgInstallation.GpgConnectAgentExecutable.Exists)
			{
				Log.Send($"Unable to confirm that gpg-agent is alive. No connect-agent found at \"{gpgInstallation.GpgConnectAgentExecutable.FullName}\".", LogLevel.Warning);
				return;
			}

			IProcess proc;
			try
			{
				// We have a gpg-agent, let's see if we can connect to it.
				proc = processes.Start(new ProcessStartInfo
				{
					FileName = gpgInstallation.GpgConnectAgentExecutable.FullName,
					Arguments = "/bye",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
				});
			}
			catch (Exception e)
			{
				Log.Send($"Unable to launch connect-agent process ({e.GetType().Name}: {e.Message})", LogLevel.Warning);
				return;
			}

			
			// Now check what the process returns.
			var readTask = proc.StandardError.ReadLineAsync();
			// Give gpg-connect-agent a moment to see if it produces any output.
			if (readTask.Wait(agentConnectTimeout))
			{
				if (readTask.Result == null)
				{
					// Nothing was sent through stdout, which means we successfully connected to a running agent,
					// which means the agent is still responsive, so we won't have to do anything here.
					Log.Send("gpg-agent alive.");
					return;
				}
				else if (readTask.Result.Contains("waiting for agent"))
				{
					Log.Send("waiting for agent to respond...");
				}
				else if (readTask.Result.Contains("no running gpg-agent"))
				{
					Log.Send("gpg-agent is not running, waiting for it to start...");
				}
				else
				{
					Log.Send($"gpg-agent produced unexpected output: \"{readTask.Result}\"", LogLevel.Warning);
					Log.Send($"waiting for agent to respond...");
				}

				// Now wait for gpg-connect-agent to quit, indicating that gpg-agent is running and responsive.
				if (proc.WaitForExit(agentReadyTimeout))
				{
					Log.Send("gpg-agent ready.");
					return;
				}

				Log.Send($"gpg-agent failed to start/respond within {agentReadyTimeout.TotalSeconds:F} seconds, starting a new one", LogLevel.Warning);
			}
			else
			{
				Log.Send($"gpg-connect-agent failed to produce any output within {agentConnectTimeout.TotalSeconds:F} seconds", LogLevel.Warning);
			}
			Log.Send($"gpg-agent is most likely unresponsive and will be restarted", LogLevel.Warning);
			// First, kill the connect-agent process.
			proc.Kill();
			// Now try to find the correct gpg-agent process.
			// We'll start by looking for a process whose filename matches the installation directory we're working with.
			// This means that if there are several gpg-agents running, we will ignore those that our gpg process likely won't try to connect to.
			var matches = processes.GetProcesses().Where(p => p.MainModuleName == gpgInstallation.GpgAgentExecutable.FullName).ToList();
			if (matches.Any())
			{
				Log.Send($"Agent process(es) found (\"{gpgInstallation.InstallDirectory.FullName}\")");
				// This should normally only return one match at most, but in certain cases
				// GPG seems to be able to detect that the agent has become unresponsive, 
				// and will start a new one without killing the old process.
				foreach (var match in matches)
				{
					Log.Send($" > killing gpg-agent {match.Id} (started {match.StartTime:G}), path: \"{match.MainModuleName}\"");
					match.Kill();
				}
			}
			else
			{
				// We didn't find any direct matches, so let's widen our search.
				foreach (var match in processes.GetProcessesByName(GpgAgentProcessName))
				{
					Log.Send($" > killing gpg-agent {match.Id} (started {match.StartTime:G}), path: \"{match.MainModuleName}\"");
					match.Kill();
				}
			}
		}
	}
}
