using PassWinmenu.Configuration;

namespace PassWinmenu.Windows
{
	internal sealed partial class EditWindow : PasswordWindow
	{
		public EditWindow(string path, string content, PasswordGenerationConfig options) : base(path, options)
		{
			Title = $"Editing '{path}'";

			string[] parts = content.Split(new char[] { '\n' }, 2);
			Password.Text = parts[0];
			if (parts.Length == 2)
			{
				ExtraContent.Text = parts[1];
			}
		}
	}
}
