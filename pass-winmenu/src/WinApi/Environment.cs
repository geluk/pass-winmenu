using System;

namespace PassWinmenu.WinApi
{
	internal class SystemEnvironment : IEnvironment
	{
		public string GetEnvironmentVariable(string variableName)
		{
			return Environment.GetEnvironmentVariable(variableName);
		}

		public string GetFolderPath(Environment.SpecialFolder folder)
		{
			return Environment.GetFolderPath(folder);
		}
	}
}
