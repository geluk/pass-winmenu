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
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "program", Scope = "member", Target = "PassWinmenu.Windows.MainWindow.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "repo", Scope = "member", Target = "PassWinmenu.ExternalPrograms.Git.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "downloadUpdate", Scope = "member", Target = "PassWinmenu.WinApi.Notifications.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "git", Scope = "member", Target = "PassWinmenu.Program.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "hotkeys", Scope = "member", Target = "PassWinmenu.Program.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "updateChecker", Scope = "member", Target = "PassWinmenu.Program.#Dispose()")]

// Ideally we'd only enable this on Exception types, but I have no idea how to do that.
[assembly: SuppressMessage(
	"Usage", 
	"CA2237:Mark ISerializable types with serializable",
	Justification = "This would generate a lot of boilerplate code that will never be used, since pass-winmenu does not serialize any exceptions.")]
[assembly: SuppressMessage(
	"Design",
	"CA1032:Implement standard exception constructors",
	Justification = "These exceptions are only used internally and must always be created with an exception message.")]
[assembly: SuppressMessage(
	"Design", 
	"CA1031:Do not catch general exception types",
	Justification = "Catching general exceptions is sometimes necessary in non-critical code paths in user-facing applications.")]
[assembly: SuppressMessage(
	"Globalization", "CA1303:Do not pass literals as localized parameters",
	Justification = "Localisation is currently out of scope for this project.")]

[assembly: SuppressMessage(
	"Naming",
	"CA1716:Identifiers should not match keywords",
	Justification = "Interop with other languages is not a feature of pass-winmenu, so this name can be used safely.",
	Scope = "type", 
	Target = "~T:PassWinmenu.Utilities.Option")]
[assembly: SuppressMessage(
	"Naming",
	"CA1716:Identifiers should not match keywords",
	Justification = "Interop with other languages is not a feature of pass-winmenu, so this name can be used safely.",
	Scope = "type",
	Target = "~T:PassWinmenu.Utilities.Option`1")]
