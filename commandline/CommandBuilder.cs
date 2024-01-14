using System.CommandLine;

namespace PassWinmenu.CommandLine;

public class CommandBuilder
{
	private readonly Option<string> configFile;

	private CommandBuilder()
	{
		RootCommand = new RootCommand("Commandline companion to pass-winmenu");
		configFile = new Option<string>(
			"--config-file",
			"Path to the configuration file, relative to the main executable");
		var defaultConfigPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "pass-winmenu.yaml");
		configFile.SetDefaultValue(defaultConfigPath);
		RootCommand.AddGlobalOption(configFile);
	}

	private RootCommand RootCommand { get; }

	private CommandBuilder AddList()
	{
		var list = new Command("list", "List passwords");
		var path = new Argument<string>("path", "Search path") {Arity = ArgumentArity.ZeroOrOne};
		list.AddArgument(path);
		list.SetHandler(Commands.List, configFile, path);
		
		RootCommand.Add(list);

		return this;
	}
	
	private CommandBuilder AddShow()
	{
		var show = new Command("show", "Show a password");
		var path = new Argument<string>("path", "Path to the password to be shown");

		var all = new Command("all", "Show an entire password file, including metadata");
		all.AddArgument(path);
		all.SetHandler(Commands.ShowAll, configFile, path);
		
		var password = new Command("password", "Show a password");
		password.AddArgument(path);
		password.SetHandler(Commands.ShowPassword, configFile, path);
		
		var key = new Command("key", "Show the value of a key");
		var keyArg = new Argument<string>("key", "Name of the key to be shown");
		key.AddArgument(keyArg);
		key.AddArgument(path);
		key.SetHandler(Commands.ShowKey, configFile, path, keyArg);
		
		show.AddCommand(all);
		show.AddCommand(password);
		show.AddCommand(key);

		RootCommand.Add(show);

		return this;
	}

	public static RootCommand Build()
	{
		return new CommandBuilder()
			.AddList()
			.AddShow()
			.RootCommand;
	}
}
