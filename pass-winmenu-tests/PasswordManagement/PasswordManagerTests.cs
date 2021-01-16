using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Moq;
using PassWinmenu.Configuration;
using PassWinmenu.PasswordManagement;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.PasswordManagement
{
	public class PasswordManagerTests
	{
		private string passwordStorePath = @"C:\password-store";

		[Fact]
		public void GetPasswordFiles_ReturnsFilesInPasswordDirectory()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password_1_content")
				.WithFile(@"C:\password-store\sub\password_2", "password_2_content")
				.WithFile(@"C:\password-store\password_3", "password_3_content")
				.WithFile(@"C:\other\password_4", "password_4_content")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));

			var files = passwordManager.GetPasswordFiles(".*").ToList();

			files.ShouldSatisfyAllConditions(
				() => files.Count.ShouldBe(3),
				() => files[0].FileNameWithoutExtension.ShouldBe("password_1"),
				() => files[1].FileNameWithoutExtension.ShouldBe("password_2"),
				() => files[2].FileNameWithoutExtension.ShouldBe("password_3")
			);
		}

		[Fact]
		public void GetPasswordFiles_WithPattern_ReturnsMatchingFilesOnly()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password_1_content")
				.WithFile(@"C:\password-store\password_2", "password_2_content")
				.WithFile(@"C:\password-store\sub\password_3", "password_3_content")
				.WithFile(@"C:\password-store\sub\not_a_password", "not_a_password")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));

			var files = passwordManager.GetPasswordFiles("password_.*").ToList();

			files.ShouldSatisfyAllConditions(
				() => files.Count.ShouldBe(3),
				() => files[0].FileNameWithoutExtension.ShouldBe("password_1"),
				() => files[1].FileNameWithoutExtension.ShouldBe("password_2"),
				() => files[2].FileNameWithoutExtension.ShouldBe("password_3")
			);
		}

		[Fact]
		public void EncryptPassword_EncryptsPasswordAtSpecifiedLocation()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password_1_content")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));
			var newPassword = CreateDecryptedPassword(fileSystem, @"C:\password-store\new_password", "new_content");

			passwordManager.EncryptPassword(newPassword);

			fileSystem.File.ReadAllText(@"C:\password-store\new_password").ShouldBe("new_content");
		}

		[Fact]
		public void EncryptPassword_WithExistingFile_OverwritesOriginalFile()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password_1_content")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));
			var newPassword = CreateDecryptedPassword(fileSystem, @"C:\password-store\password_1", "new_content");

			passwordManager.EncryptPassword(newPassword);

			fileSystem.File.ReadAllText(@"C:\password-store\password_1").ShouldBe("new_content");
		}

		[Fact]
		public void EncryptPassword_ReturnsSamePasswordFileAsProvided()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password_1_content")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));
			var newPassword = CreateDecryptedPassword(fileSystem, @"C:\password-store\new_password", "password_content");

			var encrypted = passwordManager.EncryptPassword(newPassword);

			encrypted.ShouldSatisfyAllConditions(
				() => encrypted.FileNameWithoutExtension.ShouldBe(newPassword.FileNameWithoutExtension),
				() => encrypted.FullPath.ShouldBe(newPassword.FullPath),
				() => encrypted.PasswordStore.FullName.ShouldBe(newPassword.PasswordStore.FullName)
			);
		}

		[Fact]
		public void AddPassword_NullPath_ThrowsArgumentNullException()
		{
			var fileSystem = new MockFileSystemBuilder().Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));

			Should.Throw<ArgumentNullException>(() => passwordManager.AddPassword(null, "new_content", null));
		}

		[Fact]
		public void AddPassword_AbsolutePath_ThrowsArgumentException()
		{
			var fileSystem = new MockFileSystemBuilder().Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));

			Should.Throw<ArgumentException>(() => passwordManager.AddPassword(@"C:\password-store\new_password", "new_content", null));
		}

		[Fact]
		public void AddPassword_RelativePath_AddsPasswordAtLocationRelativeToPasswordStore()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password_1_content")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));

			passwordManager.AddPassword(@"sub\new_password", "new_content", null);

			fileSystem.File.ReadAllText(@"C:\password-store\sub\new_password").ShouldBe("new_content");
		}

		[Fact]
		public void AddPassword_WithExistingFile_ThrowsInvalidOperationException()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password_1_content")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));

			Should.Throw<InvalidOperationException>(() => passwordManager.AddPassword(@"password_1", "new_content", null));
		}

		[Fact]
		public void DecryptPassword_FileDoesNotExist_ThrowsArgumentException()
		{
			var fileSystem = new MockFileSystemBuilder().Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));
			var file = new PasswordFile(fileSystem.FileInfo.FromFileName(@"C:\password-store\password_1"), passwordDirectory);

			Should.Throw<ArgumentException>(() => passwordManager.DecryptPassword(file, true));
		}


		[Fact]
		public void DecryptPassword_FileExists_DecryptsPassword()
		{
			var fileSystem = new MockFileSystemBuilder()
				.WithFile(@"C:\password-store\password_1", "password\nmetadata")
				.Build();
			var passwordDirectory = new MockDirectoryInfo(fileSystem, passwordStorePath);
			var passwordManager = new PasswordManager(passwordDirectory, new FakeCryptoService(fileSystem), Mock.Of<IRecipientFinder>(), new PasswordFileParser(new UsernameDetectionConfig()));
			var file = new PasswordFile(fileSystem.FileInfo.FromFileName(@"C:\password-store\password_1"), passwordDirectory);

			var decrypted = passwordManager.DecryptPassword(file, true);

			decrypted.Content.ShouldBe("password\nmetadata");
		}

		private DecryptedPasswordFile CreateDecryptedPassword(MockFileSystem fileSystem, string path, string content)
		{
			return new DecryptedPasswordFile(new PasswordFile(
				fileSystem.FileInfo.FromFileName(path),
				fileSystem.DirectoryInfo.FromDirectoryName(@"C:\password-store")
			), content);
		}
	}
}
