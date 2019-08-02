// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

// ---------------------------------------------------------
// See issue https://github.com/dotnet/roslyn-analyzers/issues/291
// CA2213 will still be raised if the null conditional operator is
// used to dispose members. Only in this case may the warning be 
// suppressed, by adding it below here:
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "program", Scope = "member", Target = "PassWinmenu.Windows.MainWindow.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "repo", Scope = "member", Target = "PassWinmenu.ExternalPrograms.Git.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "downloadUpdate", Scope = "member", Target = "PassWinmenu.WinApi.Notifications.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "git", Scope = "member", Target = "PassWinmenu.Program.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "hotkeys", Scope = "member", Target = "PassWinmenu.Program.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "updateChecker", Scope = "member", Target = "PassWinmenu.Program.#Dispose()")]
