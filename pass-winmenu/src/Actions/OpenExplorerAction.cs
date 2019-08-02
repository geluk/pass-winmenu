using System.Diagnostics;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;

namespace PassWinmenu.Actions
{
	class OpenExplorerAction : IAction
	{
		private readonly IProcesses processes;
		private readonly PasswordStoreConfig passwordStore;

		public HotkeyAction ActionType => HotkeyAction.OpenExplorer;

		public OpenExplorerAction(IProcesses processes, PasswordStoreConfig passwordStore)
		{
			this.processes = processes;
			this.passwordStore = passwordStore;
		}

		public void Execute()
		{
			processes.Start(new ProcessStartInfo(passwordStore.Location));
		}
	}
}
