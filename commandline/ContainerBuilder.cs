using Autofac;
using PassWinmenu.Configuration;

namespace PassWinmenu.CommandLine;

public static class ContainerBuilder
{
	public static IContainer Build(string configPath)
	{
		var loadResult = ConfigManager.Load(configPath, allowCreate: false);
		var configManager = loadResult switch
		{
			LoadResult.Success s => s.ConfigManager,
			LoadResult.NeedsUpgrade => throw new Exception("Outdated configuration file"),
			LoadResult.NotFound => throw new Exception("Configuration file not found"),
			_ => throw new ArgumentOutOfRangeException(),
		};

		return Setup.InitialiseCommandLine(configManager);
	}
}
