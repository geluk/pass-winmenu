using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PassWinmenu.WinApi
{
	internal class StartupLink
	{
		public class StartupLinkCreationException : Exception
		{
			public StartupLinkCreationException(string message) : base(message) { }
		}

		public string Name { get; }
		public string ShortcutPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Name);
		public bool Exists => File.Exists(ShortcutPath);

		public StartupLink(string name)
		{
			Name = name + ".lnk";
		}

		/// <summary>
		/// Toggle a startup link, linking to the specified file.
		/// If the link does not exist, it is created.
		/// If it does exist, it is deleted.
		/// </summary>
		/// <param name="targetPath">The path to the application that should be executed.</param>
		/// <param name="workingDirectory">The working directory in which the application should execute.
		/// Leave at Null to use the directory containing the target application.</param>
		public void Toggle(string targetPath, string workingDirectory = null)
		{
			if (Exists)
			{
				Delete();
			}
			else
			{
				Create(targetPath, workingDirectory);
			}
		}

		/// <summary>
		/// Create a startup link, linking to the specified file.
		/// </summary>
		/// <param name="targetPath">The path to the application that should be executed.</param>
		/// <param name="workingDirectory">The working directory in which the application should execute.
		/// Leave at Null to use the directory containing the target application.</param>
		public void Create(string targetPath, string workingDirectory = null)
		{
			if (workingDirectory == null)
			{
				workingDirectory = Path.GetDirectoryName(targetPath);
			}
			CreateShortcutInternal(false, targetPath, workingDirectory);
		}

		private void Delete()
		{
			File.Delete(ShortcutPath);
		}

		/// <summary>
		/// Creates a shortcut to the application, overwriting any existing shortcuts with the same name.
		/// </summary>
		private void CreateShortcutInternal(bool overwrite, string targetPath, string workingDirectory)
		{
			if (Exists)
			{
				if (overwrite)
				{
					File.Delete(ShortcutPath);
				}
				else
				{
					throw new StartupLinkCreationException($"A startup link already exists at \"{ShortcutPath}\"");
				}
			}

			var t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
			dynamic shell = Activator.CreateInstance(t);
			// We use try/finally to ensure the resources we create get cleaned up,
			// while still allowing exceptions to propagate up the call stack.
			try
			{
				var lnk = shell.CreateShortcut(ShortcutPath);
				try
				{
					lnk.TargetPath = targetPath;
					lnk.WorkingDirectory = workingDirectory;
					lnk.IconLocation = "shell32.dll, 1";
					lnk.Save();
				}
				finally
				{
					Marshal.FinalReleaseComObject(lnk);
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(shell);
			}
		}
	}
}
