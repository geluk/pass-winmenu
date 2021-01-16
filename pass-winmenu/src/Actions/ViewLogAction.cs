using System.Linq;
using PassWinmenu.Configuration;
using PassWinmenu.Utilities;
using PassWinmenu.Windows;

namespace PassWinmenu.Actions
{
	class ViewLogAction : IAction
	{
		public HotkeyAction ActionType => HotkeyAction.ViewLog;

		public void Execute()
		{
			Helpers.AssertOnUiThread();
			var viewer = new LogViewer(string.Join("\n", Log.History.Select(l => l.ToString())));
			viewer.ShowDialog();
		}
	}
}
