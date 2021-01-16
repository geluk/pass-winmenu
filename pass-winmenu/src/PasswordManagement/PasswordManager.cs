using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.Utilities;

namespace PassWinmenu.PasswordManagement
{
	internal class PasswordManager : IPasswordManager
	{
		private readonly IDirectoryInfo passwordStore;
		private readonly ICryptoService cryptoService;
		private readonly IRecipientFinder recipientFinder;
		private readonly PasswordFileParser passwordFileParser;

		private IFileSystem FileSystem => passwordStore.FileSystem;

		public PasswordManager(
			IDirectoryInfo passwordStore,
			ICryptoService cryptoService,
			IRecipientFinder recipientFinder,
			PasswordFileParser passwordFileParser)
		{
			this.recipientFinder = recipientFinder;
			this.passwordFileParser = passwordFileParser;
			this.passwordStore = passwordStore;
			this.cryptoService = cryptoService;
		}

		/// <summary>
		/// Encrypts a password file at the specified path.
		/// If the path contains directories that do not exist, they will be created automatically.
		/// </summary>
		/// <param name="file">
		/// A <see cref="KeyedPasswordFile"/> instance specifying the contents
		/// of the password file to be encrypted.
		/// </param>
		public PasswordFile EncryptPassword(DecryptedPasswordFile file)
		{
			return EncryptPasswordInternal(file, true);
		}

		/// <summary>
		/// Adds a new password file at the specified path.
		/// </summary>
		/// <param name="path">A path, relative to the password store, indicating where the password should be created.</param>
		/// <param name="password">The password to be encrypted.</param>
		/// <param name="metadata">Any metadata that should be added.</param>
		/// <exception cref="InvalidOperationException">If a file already exists at the given location.</exception>
		public PasswordFile AddPassword(string path, string password, string metadata)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}
			if (FileSystem.Path.IsPathRooted(path))
			{
				throw new ArgumentException("Path to the password file must be relative.");
			}

			var file = CreatePasswordFileFromPath(path);
			var parsed = new ParsedPasswordFile(file, password, metadata);
			return EncryptPasswordInternal(parsed, false);
		}
		
		/// <summary>
		/// Get the content from an encrypted password file.
		/// </summary>
		/// <param name="file">A <see cref="PasswordFile"/> specifying the password file to be decrypted.</param>
		/// <param name="passwordOnFirstLine">Should be true if the first line of the file contains the password.
		/// Any content in the remaining lines will be considered metadata.
		/// If set to false, the contents of the entire file are considered to be the password.</param>
		public KeyedPasswordFile DecryptPassword(PasswordFile file, bool passwordOnFirstLine)
		{
			if (!file.FileInfo.Exists) throw new ArgumentException($"The password file \"{file.FullPath}\" does not exist.");

			var content = cryptoService.Decrypt(file.FullPath);
			return passwordFileParser.Parse(file, content, !passwordOnFirstLine);
		}

		/// <summary>
		/// Returns all password files that match a search pattern.
		/// </summary>
		/// <param name="pattern">The pattern against which the files should be matched.</param>
		public IEnumerable<PasswordFile> GetPasswordFiles(string pattern)
		{
			var patternRegex = new Regex(pattern);

			var files = passwordStore.EnumerateFiles("*", SearchOption.AllDirectories);
			var matchingFiles = files.Where(f => patternRegex.IsMatch(f.Name));
			var passwordFiles = matchingFiles.Select(CreatePasswordFile);

			return passwordFiles;
		}

		private PasswordFile EncryptPasswordInternal(DecryptedPasswordFile file, bool overwrite)
		{
			file.Directory.Create();
			if (file.FileInfo.Exists)
			{
				if (overwrite)
				{
					file.FileInfo.Delete();
				}
				else
				{
					throw new InvalidOperationException("A password file already exists at the specified location.");
				}
			}
			cryptoService.Encrypt(file.Content, file.FullPath, recipientFinder.FindRecipients(file));
			return new PasswordFile(file);
		}

		private PasswordFile CreatePasswordFile(IFileInfo file)
		{
			return new PasswordFile(file, passwordStore);
		}

		private PasswordFile CreatePasswordFileFromPath(string relativePath)
		{
			var fullPath = FileSystem.Path.Combine(passwordStore.FullName, relativePath);
			return new PasswordFile(FileSystem.FileInfo.FromFileName(fullPath), passwordStore);
		}
	}
}
