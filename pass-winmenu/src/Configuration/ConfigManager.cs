using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PassWinmenu.Configuration
{
	internal class ConfigManager
	{
		public static Config Config { get; private set; } = new Config();
		private static FileSystemWatcher watcher;

		~ConfigManager()
		{
			watcher?.Dispose();
		}

		public static void EnableAutoReloading(string fileName)
		{
			var directory = Path.GetDirectoryName(fileName);
			if (string.IsNullOrWhiteSpace(directory))
			{
				directory = Directory.GetCurrentDirectory();
			}
			watcher = new FileSystemWatcher(directory)
			{
				IncludeSubdirectories = false,
				EnableRaisingEvents = true
			};
			watcher.Changed += (sender, args) =>
			{
				// Wait a moment to allow the writing process to close the file.
				Thread.Sleep(500);
				Log.Send($"Configuration file changed (change type: {args.ChangeType}), attempting reload.");

				// This needs to be done on the main thread reloading the configuration file
				// involves creating UI resources (Brush/Thickness) that need to be created
				// on the same thread as the thread that will apply those resources to the interface.
				Application.Current.Dispatcher.Invoke(() =>
				{
					Reload(fileName);
				});
			};
		}

		public static LoadResult Load(string fileName)
		{
			if (!File.Exists(fileName))
			{
				try
				{
					using (var defaultConfig = EmbeddedResources.DefaultConfig)
					using (var configFile = File.Create(fileName))
					{
						defaultConfig.CopyTo(configFile);
					}
				}
				catch (Exception e) when (e is FileNotFoundException || e is FileLoadException || e is IOException)
				{
					return LoadResult.FileCreationFailure;
				}

				return LoadResult.NewFileCreated;
			}

			var deserialiser = BuildDeserialiser();

			using (var reader = File.OpenText(fileName))
			{
				var versionCheck = deserialiser.Deserialize<Dictionary<string, object>>(reader);
				if (versionCheck == null || !versionCheck.ContainsKey("config-version"))
				{
					return LoadResult.NeedsUpgrade;
				}
				if (versionCheck["config-version"] as string != Program.LastConfigVersion) return LoadResult.NeedsUpgrade;
			}

			using (var reader = File.OpenText(fileName))
			{
				Config = deserialiser.Deserialize<Config>(reader);
			}

			return LoadResult.Success;
		}

		private static IDeserializer BuildDeserialiser()
		{
			var builder = new DeserializerBuilder()
				.WithNamingConvention(HyphenatedNamingConvention.Instance)
				.WithTypeConverter(new WidthConverter())
				.WithTypeConverter(new BrushConverter());
			return builder.Build();
		}

		public static void Reload(string fileName)
		{
			try
			{
				var deserialiser = BuildDeserialiser();
				using (var reader = File.OpenText(fileName))
				{
					var newConfig = deserialiser.Deserialize<Config>(reader);
					Config = newConfig;
				}
				Log.Send("Configuration file reloaded successfully.");

			}
			catch (Exception e)
			{
				Log.Send($"Could not reload configuration file. An exception occurred.");
				Log.ReportException(e);
				// No need to do anything, we can simply continue using the old configuration.
			}
		}

		public static string Backup(string fileName)
		{
			var extension = Path.GetExtension(fileName);
			var name = Path.GetFileNameWithoutExtension(fileName);
			var directory = Path.GetDirectoryName(fileName);

			// Find an unused name to which we can rename the old configuration file.
			var root = string.IsNullOrEmpty(directory) ? name : Path.Combine(directory, name);
			var newFileName = $"{root}-backup{extension}";
			var counter = 2;
			while (File.Exists(newFileName))
			{
				newFileName =$"{root}-backup-{counter++}{extension}";
			}

			File.Move(fileName, newFileName);
		
			using (var defaultConfig = EmbeddedResources.DefaultConfig)
			using (var configFile = File.Create(fileName))
			{
				defaultConfig.CopyTo(configFile);
			}
			return newFileName;
		}
	}
}
