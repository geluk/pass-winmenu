using Autofac;
using PassWinmenu.PasswordManagement;

namespace PassWinmenu.CommandLine;

public class ListCommand
{
	private readonly IPasswordManager passwordManager;

	public ListCommand(string configPath)
	{
		this.passwordManager = ContainerBuilder.Build(configPath).Resolve<IPasswordManager>();
	}

	public void List(string searchPath)
	{
		var passwords = this.passwordManager.GetPasswordFiles();
		foreach (var password in passwords)
		{
			Console.WriteLine(password.FullPath);
		}
	}
}
