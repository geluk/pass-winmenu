namespace PassWinmenu.CommandLine;

public static class Commands
{
	public static void ShowAll(string configPath, string passwordPath)
	{
		new ShowCommand(configPath, passwordPath).All();
	}

	public static void ShowPassword(string configPath, string passwordPath)
	{
		new ShowCommand(configPath, passwordPath).Password();
	}

	public static void ShowKey(string configPath, string passwordPath, string key)
	{
		new ShowCommand(configPath, passwordPath).Key(key);
	}

	public static void List(string configPath, string searchPath)
	{
		new ListCommand(configPath).List(searchPath);
	}
}
