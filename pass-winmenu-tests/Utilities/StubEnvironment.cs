using System;
using System.Collections.Generic;
using System.Linq;
using PassWinmenu.WinApi;

namespace PassWinmenuTests.Utilities
{
	public class StubEnvironment : IEnvironment
	{
		private readonly IDictionary<Environment.SpecialFolder, string> specialFolders;
		private readonly IDictionary<string, string> environmentVariables;

		private StubEnvironment(IDictionary<Environment.SpecialFolder, string> specialFolders, IDictionary<string, string> environmentVariables)
		{
			this.specialFolders = specialFolders;
			this.environmentVariables = environmentVariables.ToDictionary(p => p.Key.ToUpper(), p => p.Value);
		}

		public string GetEnvironmentVariable(string variableName)
		{
			variableName = variableName.ToUpper();

			if (environmentVariables.TryGetValue(variableName, out var value))
			{
				return value;
			}

			return null;
		}

		public string GetFolderPath(Environment.SpecialFolder folder)
		{
			return specialFolders[folder];
		}

		public static StubEnvironmentBuilder Create()
		{
			return new StubEnvironmentBuilder();
		}

		public class StubEnvironmentBuilder
		{
			private readonly IDictionary<Environment.SpecialFolder, string> specialFolders = new Dictionary<Environment.SpecialFolder, string>();
			private readonly IDictionary<string, string> environmentVariables = new Dictionary<string, string>();

			public StubEnvironmentBuilder WithSpecialFolder(Environment.SpecialFolder folder, string path)
			{
				specialFolders[folder] = path;
				return this;
			}

			public StubEnvironmentBuilder WithEnvironmentVariable(string variable, string value)
			{
				environmentVariables[variable] = value;
				return this;
			}

			public StubEnvironment Build()
			{
				return new StubEnvironment(specialFolders, environmentVariables);
			}
		}
	}
}
