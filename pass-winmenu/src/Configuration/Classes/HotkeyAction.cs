namespace PassWinmenu.Configuration
{
	internal enum HotkeyAction
	{
		DecryptPassword, // Decrypt a password
		AddPassword, // Add a new password
		EditPassword, // Edit a password
		DecryptMetadata, // Fetch the metadata from a password
		PasswordField, // Fetch the field from a metadata key
		GitPull, // Run `git pull` on the password store
		GitPush, // Run `git push` on the password store
		OpenShell, // Open a shell with GPG access in your password store
		OpenExplorer, // Open the password store in Windows Explorer
		SelectNext, // Select the next entry in the password menu
		SelectPrevious, // Select the previous entry in the password menu
		SelectFirst, // Select the first entry in the password menu
		SelectLast, // Select the last entry in the password menu
		// Debugging actions
		ShowDebugInfo, // Show a window with some debug information.
		CheckForUpdates, // Force an update check.
	}
}
