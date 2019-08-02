using System.IO.Abstractions;
using PassWinmenu.Utilities.ExtensionMethods;

namespace PassWinmenu.PasswordManagement
{
	internal class PasswordFile
	{
		/// <summary>
		/// Represents the root password store directory this file lives in.
		/// </summary>
		public IDirectoryInfo PasswordStore { get; }
		/// <summary>
		/// A <see cref="FileInfo"/> instance representing the password file.
		/// </summary>
		public IFileInfo FileInfo { get; }

		/// <summary>
		/// Represents the directory containing the password file.
		/// </summary>
		public IDirectoryInfo Directory => FileInfo.Directory;
		/// <summary>
		/// The full path to this file.
		/// </summary>
		public string FullPath => FileInfo.FullName;
		/// <summary>
		/// The base name of the password file, without its extension.
		/// </summary>
		public string FileNameWithoutExtension => FileInfo.Name.RemoveEnd(FileInfo.Extension);
		
		public PasswordFile(IFileInfo file, IDirectoryInfo passwordStore)
		{
			FileInfo = file;
			PasswordStore = passwordStore;
		}

		public PasswordFile(PasswordFile original)
		{
			FileInfo = original.FileInfo;
			PasswordStore = original.PasswordStore;
		}
	}
}
