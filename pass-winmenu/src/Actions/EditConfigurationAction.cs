using System.Diagnostics;
using PassWinmenu.Configuration;

namespace PassWinmenu.Actions
{
	class EditConfigurationAction : IAction
	{
		public void Execute()
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = "explorer", 
				Arguments = Program.ConfigFileName,
			};
			Process.Start(startInfo);
		}

		public HotkeyAction ActionType => HotkeyAction.EditConfiguration;
	}
}
