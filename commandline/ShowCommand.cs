using Autofac;
using PassWinmenu.PasswordManagement;
using PassWinmenu.Utilities;

namespace PassWinmenu.CommandLine;

public class ShowCommand
{
	private readonly string passwordPath;
	private readonly IPasswordManager passwordManager;

	public ShowCommand(string configPath, string passwordPath)
	{
		if (!passwordPath.EndsWith(PassWinmenu.Program.EncryptedFileExtension))
		{
			passwordPath += PassWinmenu.Program.EncryptedFileExtension;
		}

		this.passwordPath = passwordPath;
		this.passwordManager = ContainerBuilder.Build(configPath).Resolve<IPasswordManager>();
	}
	
	public void All()
	{
		var password = GetPasswordFile();
		var decrypted = passwordManager.DecryptPassword(password, false).Content;
		Console.WriteLine(decrypted);
	}

	public void Password()
	{
		var password = GetPasswordFile();
		var decrypted = passwordManager.DecryptPassword(password, true).Password;
		Console.WriteLine(decrypted);
	}

	public void Key(string key)
	{
		var password = GetPasswordFile();
		var keys = passwordManager.DecryptPassword(password, true).Keys;

		var matchingPair = keys
			.Select<KeyValuePair<string, string>, KeyValuePair<string, string>?>(p => p)
			.FirstOrDefault(k => string.Equals(k!.Value.Key, key, StringComparison.CurrentCultureIgnoreCase));
		if (matchingPair.HasValue)
		{
			Console.WriteLine(matchingPair.Value.Value);
		}
		else
		{
			Console.Error.WriteLine("Key does not exist!");
			Environment.Exit(1);
		}
	}

	private PasswordFile GetPasswordFile()
	{
		var file = passwordManager.QueryPasswordFile(passwordPath).ValueOrDefault();
		if (file == null)
		{
			Console.Error.WriteLine("Password does not exist!");
			Environment.Exit(1);
		}

		return file;
	}
}
