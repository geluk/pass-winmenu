using System;
using System.IO.Abstractions;
using PassWinmenu.Utilities.ExtensionMethods;

namespace PassWinmenu.PasswordManagement
{
	internal class GpgRecipientFinder : IRecipientFinder
	{
		internal const string GpgIdFileName = ".gpg-id";

		private readonly IDirectoryInfo passwordStore;
		private readonly IFileSystem fileSystem;

		public GpgRecipientFinder(IDirectoryInfo passwordStore)
		{
			this.passwordStore = passwordStore;
			this.fileSystem = passwordStore.FileSystem;
		}

		public string[] FindRecipients(PasswordFile file)
		{
			var current = file.Directory;

			// Walk up from the innermost directory, and keep moving up until an existing directory 
			// containing a gpg-id file is found.
			while (!current.Exists || !current.ContainsFile(GpgIdFileName))
			{
				if (current.Parent == null || current.PathEquals(passwordStore))
				{
					return Array.Empty<string>();
				}
				current = current.Parent;
			}

			return fileSystem.File.ReadAllLines(fileSystem.Path.Combine(current.FullName, GpgIdFileName));
		}
	}
}
