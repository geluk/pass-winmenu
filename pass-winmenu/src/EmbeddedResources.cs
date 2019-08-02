using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace PassWinmenu
{
	public static class EmbeddedResources
	{
		public static Icon Icon => new Icon(Assembly.GetEntryAssembly().GetManifestResourceStream("PassWinmenu.embedded.pass-winmenu.ico"));
		public static Stream DefaultConfig => Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.default-config.yaml");
		public static string Version { get; private set; }

		public static void Load()
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.version.txt");
			if (stream == null)
			{
				throw new InvalidOperationException("Version number could not be read from the assembly.");
			}
			using (var reader = new StreamReader(stream))
			{
				Version = reader.ReadLine();
			}
		}
	}
}