using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using LibGit2Sharp;

namespace PassWinmenu.ExternalPrograms
{
	public class NativeGitSyncStrategy : IGitSyncStrategy
	{
		private readonly string gitPath;
		private readonly string repositoryPath;
		private readonly TimeSpan gitCallTimeout = TimeSpan.FromSeconds(5);

		public NativeGitSyncStrategy(string gitPath, string repositoryPath)
		{
			this.gitPath = gitPath;
			this.repositoryPath = repositoryPath;
		}

		public void Fetch(Branch branch)
		{
			CallGit("fetch " + branch.RemoteName);
		}

		/// <summary>
		/// Pushes changes to remote.
		/// </summary>
		public void Push()
		{
			CallGit("push");
		}
		private void CallGit(string arguments)
		{
			var argList = new List<string>
			{
				// May be required in certain cases?
				//"--non-interactive"
			};

			var psi = new ProcessStartInfo
			{
				FileName = gitPath,
				WorkingDirectory = repositoryPath,
				Arguments = $"{arguments} {string.Join(" ", argList)}",
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				CreateNoWindow = true
			};
			if (!String.IsNullOrEmpty(Configuration.ConfigManager.Config.Git.SshPath))
			{
				psi.EnvironmentVariables.Add("GIT_SSH", Configuration.ConfigManager.Config.Git.SshPath);
			}
			Process gitProc;
			try
			{
				gitProc = Process.Start(psi);
			}
			catch (Win32Exception e)
			{
				throw new GitException("Git failed to start. " + e.Message, e);
			}

			gitProc.WaitForExit((int)gitCallTimeout.TotalMilliseconds);
			var output = gitProc.StandardOutput.ReadToEnd();
			var error = gitProc.StandardError.ReadToEnd();
			if (gitProc.ExitCode != 0)
			{
				throw new GitException($"Git exited with code {gitProc.ExitCode}", error);
			}
		}
	}
}
