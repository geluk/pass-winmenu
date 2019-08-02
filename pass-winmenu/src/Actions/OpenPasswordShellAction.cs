using System.Diagnostics;
using System.IO.Abstractions;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.ExternalPrograms.Gpg;

namespace PassWinmenu.Actions
{
	class OpenPasswordShellAction : IAction
	{
		private readonly GpgInstallation installation;
		private readonly PasswordStoreConfig passwordStore;
		private readonly IGpgHomedirResolver homedirResolver;
		private readonly IFileSystem fileSystem;
		private readonly IProcesses processes;

		public HotkeyAction ActionType => HotkeyAction.OpenShell;

		public OpenPasswordShellAction(GpgInstallation installation, PasswordStoreConfig passwordStore, IGpgHomedirResolver homedirResolver, IFileSystem fileSystem, IProcesses processes)
		{
			this.installation = installation;
			this.passwordStore = passwordStore;
			this.homedirResolver = homedirResolver;
			this.fileSystem = fileSystem;
			this.processes = processes;
		}

		public void Execute()
		{
			var powerShell = new ProcessStartInfo
			{
				FileName = "powershell",
				WorkingDirectory = passwordStore.Location,
				UseShellExecute = false
			};

			var gpgExe = installation.GpgExecutable.FullName;

			var homeDir = string.Empty;
			if (homedirResolver.GetConfiguredHomeDir() != null)
			{
				homeDir = $" --homedir \"{fileSystem.Path.GetFullPath(homedirResolver.GetConfiguredHomeDir())}\"";
			}
			powerShell.Arguments = $"-NoExit -Command \"function gpg() {{ & '{gpgExe}'{homeDir} $args }}\"";
			processes.Start(powerShell);
		}
	}

	internal interface IAction
	{
		void Execute();
		HotkeyAction ActionType { get; }
	}
}
