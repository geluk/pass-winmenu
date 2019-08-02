using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Helpers = PassWinmenu.Utilities.Helpers;

namespace PassWinmenu.Hotkeys
{
	// Abstraction over a message-only window for the Windows hotkey registrar.
	//
	// See main hotkeys registrar file:
	//      /src/Hotkeys/HotkeyRegistrars.cs
	//
	// See main Windows hotkey registrar file:
	//      /src/Hotkeys/HotkeyRegistrars.Windows.cs
	public static partial class HotkeyRegistrars
	{
		private sealed partial class WindowsHotkeyRegistrar
		{
			/// <summary>
			/// The types of message the window can receive. This enumeration
			/// does not exhaustively list message types.
			/// </summary>
			private enum WindowMessage : uint
			{
				/// <summary>
				/// The message received when a global hotkey is triggered.
				/// </summary>
				Hotkey  = 0x0312,
			}

			/// <summary>
			/// A procedure for handling messages received by the window.
			/// </summary>
			/// <param name="handle">
			/// A handle to the window receiving the message.
			/// </param>
			/// <param name="message">
			/// The type of message received.
			/// </param>
			/// <param name="wParam">
			/// Additional information dependent on the value of
			/// <paramref name="message"/>.
			/// </param>
			/// <param name="lParam">
			/// Additional information dependent on the value of
			/// <paramref name="message"/>.
			/// </param>
			/// <returns>
			/// A value which depends on the value of <paramref name="message"/>
			/// and which indicates the result of processing the message. Return
			/// null to defer to the next available window procedure (which is
			/// the default procedure if no other procedure is registered).
			/// </returns>
			private delegate Nullable<IntPtr> WindowProcedure(
				IntPtr          handle,
				WindowMessage   message,
				UIntPtr         wParam,
				IntPtr          lParam
				);

			/// <summary>
			/// Represents a message-only window, which can send and receive
			/// messages but which is not visible, not enumerable, and does
			/// not receive broadcast messages.
			/// </summary>
			private sealed class MessageWindow
				: IDisposable
			{
				private delegate IntPtr WndProc(
					IntPtr hWnd, uint uMsg, UIntPtr wParam, IntPtr lParam
					);

				/// <summary>
				/// Creates an overlapped, pop-up, or child window with an extended
				/// window style (otherwise identical to CreateWindow).
				/// </summary>
				/// <param name="dwExtStyle">
				/// The extended window styles to apply to the window.
				/// </param>
				/// <param name="lpClassName">
				/// A pointer to the name of the class to use for the window, or an
				/// atom returned by <see cref="RegisterClass(ref WindowClass)"/>.
				/// </param>
				/// <param name="lpWindowName">
				/// The name of the window to create.
				/// </param>
				/// <param name="dwStyle">
				/// The style values for the window to create.
				/// </param>
				/// <param name="x">
				/// The horizontal position of the window.
				/// </param>
				/// <param name="y">
				/// The vertical position of the window.
				/// </param>
				/// <param name="nWidth">
				/// The width of the window in device units.
				/// </param>
				/// <param name="nHeight">
				/// The height of the window in device units.
				/// </param>
				/// <param name="hWndParent">
				/// The handle to the window that is to be the parent of the
				/// created window.
				/// </param>
				/// <param name="hMenu">
				/// The handle to a menu or child window.
				/// </param>
				/// <param name="hInstance">
				/// The handle to an instance of the module to be associated
				/// with the window.
				/// </param>
				/// <param name="lpParam">
				/// A pointer to a value to be passed as the <c>lParam</c> of the
				/// <c>WM_CREATE</c> message sent to the window on creation.
				/// </param>
				/// <returns></returns>
				[DllImport("user32.dll", SetLastError = true)]
				private static extern IntPtr CreateWindowEx(
					uint    dwExtStyle,
					UIntPtr lpClassName,
					IntPtr  lpWindowName,
					uint    dwStyle,
					int     x,
					int     y,
					int     nWidth,
					int     nHeight,
					IntPtr  hWndParent,
					IntPtr  hMenu,
					IntPtr  hInstance,
					IntPtr  lpParam
					);

				/// <summary>
				/// The handle to specify as the parent of a window when creating
				/// a message-only window.
				/// </summary>
				private static IntPtr HWND_MESSAGE { get; } = new IntPtr(-3);

				/// <summary>
				/// Destroys a window.
				/// </summary>
				/// <param name="hWnd">
				/// The handle to the window to destroy.
				/// </param>
				/// <returns>
				/// True if the window was successfully destroyed, false if
				/// otherwise.
				/// </returns>
				[DllImport("user32.dll", SetLastError = true)]
				[return: MarshalAs(UnmanagedType.Bool)]
				private static extern bool DestroyWindow(IntPtr hWnd);

				/// <summary>
				/// Registers a window class for use in creating a window.
				/// </summary>
				/// <param name="wndClass">
				/// A description of the class to be created.
				/// </param>
				/// <returns>
				/// Zero if the operation does not succeed, or an atom uniquely
				/// identifying the registered class otherwise.
				/// </returns>
				[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
				private static extern ushort RegisterClass(ref WindowClass wndClass);

				/// <summary>
				/// Unregisters a class registered using
				/// <see cref="RegisterClass(ref WindowClass)"/>.
				/// </summary>
				/// <param name="lpClassName">
				/// A pointer to the name of the class, or an atom returned
				/// by <see cref="RegisterClass(ref WindowClass)"/>.
				/// </param>
				/// <param name="hInstance">
				/// A handle to the instance of the module that created the
				/// class.
				/// </param>
				/// <returns>
				/// True if the class was unregistered, false if otherwise.
				/// </returns>
				[DllImport("user32.dll", SetLastError = true)]
				[return: MarshalAs(UnmanagedType.Bool)]
				private static extern bool UnregisterClass(
					IntPtr lpClassName, IntPtr hInstance
					);

				/// <summary>
				/// A description of a window class to be registered through
				/// <see cref="RegisterClass(ref WindowClass)"/>.
				/// </summary>
				[StructLayout(LayoutKind.Sequential)]
				private struct WindowClass
				{
					/// <summary>
					/// The styles for the window class.
					/// </summary>
					public uint style;

					/// <summary>
					/// The window procedure for handling messages received
					/// by windows of this class.
					/// </summary>
					[MarshalAs(UnmanagedType.FunctionPtr)]
					public WndProc lpfnWndProc;

					/// <summary>
					/// The quantity of additional bytes to allocate after the
					/// window class structure.
					/// </summary>
					public int cbClsExtra;
					/// <summary>
					/// The quantity of additional bytes to allocate after an
					/// instance of a window of this class.
					/// </summary>
					public int cbWndExtra;
					/// <summary>
					/// A handle to the instance that contains the window
					/// procedure for the class.
					/// </summary>
					public IntPtr hInstance;
					/// <summary>
					/// A handle to the icon for the class.
					/// </summary>
					public IntPtr hIcon;
					/// <summary>
					/// A handle to the cursor for the class.
					/// </summary>
					public IntPtr hCursor;
					/// <summary>
					/// A handle to the class background brush.
					/// </summary>
					public IntPtr hbrBackground;

					/// <summary>
					/// The name of the default menu for the class.
					/// </summary>
					[MarshalAs(UnmanagedType.LPWStr)]
					public string lpszMenuName;

					/// <summary>
					/// The name for this window class.
					/// </summary>
					[MarshalAs(UnmanagedType.LPWStr)]
					public string lpszClassName;
				}

				/// <summary>
				/// The default window procedure.
				/// </summary>
				[DllImport("user32.dll", SetLastError = true)]
				private static extern IntPtr DefWindowProc(
					IntPtr hWnd, uint uMsg, UIntPtr wParam, IntPtr lParam
					);


				// Internal window procedure
				private IntPtr _proc(IntPtr hWnd, uint uMsg, UIntPtr wp, IntPtr lp)
				{
					IntPtr? ret = null;
					foreach (var wndProc in this.Procedures)
					{
						ret = wndProc(hWnd, (WindowMessage)uMsg, wp, lp);

						// If the procedure returned a value, we stop deferring
						// through window procedures.
						if (ret.HasValue)
							break;
					}

					// If we have a value, return it.
					//
					// Otherwise, if we don't, either there were no procedures
					// or all the procedures wanted to defer. Either way, we
					// want to defer to the default window procedure.
					return ret ?? DefWindowProc(hWnd, uMsg, wp, lp);
				}

				// Whether we've been disposed
				private bool _disposed = false;
				// Atom representing our window class
				private readonly ushort _windowAtom;
				// Guid used for the window class name.
				private readonly Guid _windowClassName;
				// A reference to our window procedure delegate. Required to prevent
				// the GC collecting the delegate we pass to unmanaged code.
				private readonly WndProc _procRef;
				
				/// <summary>
				/// Creates a message-only window with the specified procedures
				/// for processing messages.
				/// </summary>
				/// <param name="procs">
				/// The procedures for processing messages received by the window,
				/// in the order to which the procedures should be deferred.
				/// </param>
				/// <exception cref="ArgumentNullException">
				/// <paramref name="procs"/> is null or contains an element
				/// that is null.
				/// </exception>
				public MessageWindow(IReadOnlyCollection<WindowProcedure> procs)
				{
					// Create new list to avoid unintended side effects of
					// modifying the collection we were passed as a parameter.
					//
					// This initialisation needs to be before the window is
					// created, as a message is sent to the window immediately
					// on creation and an NRE will result if this is not set.
					this.Procedures = procs.ToList();

					_windowClassName = Guid.NewGuid();

					var hInstance = Process.GetCurrentProcess().Handle;

					// Keep a reference around for our window procedure to avoid
					// the GC collecting it. Important that this is not removed.
					_procRef = _proc;

					var wndClass = new WindowClass
					{
						// Always use [_procRef] and not [_proc]; see above.
						lpfnWndProc   = _procRef,
						lpszClassName = _windowClassName.ToString(),
						hInstance     = hInstance,
					};

					_windowAtom = RegisterClass(ref wndClass);

					if (_windowAtom == 0)
					{
						throw Helpers.LastWin32Exception();
					}

					this.Handle = CreateWindowEx(
						dwExtStyle:     0,
						lpClassName:    (UIntPtr)_windowAtom,
						lpWindowName:   IntPtr.Zero,
						dwStyle:        0,
						x:              0,
						y:              0,
						nWidth:         0,
						nHeight:        0,
						hWndParent:     IntPtr.Zero,
						hMenu:          IntPtr.Zero,
						hInstance:      hInstance,
						lpParam:        IntPtr.Zero
						);

					if (this.Handle == IntPtr.Zero)
					{
						throw Helpers.LastWin32Exception();
					}
				}
				/// <summary>
				/// Creates a message-only window with the specified procedure
				/// for processing messages.
				/// </summary>
				/// <param name="procs">
				/// The procedure for processing messages received by the window.
				/// </param>
				/// <exception cref="ArgumentNullException">
				/// <paramref name="wndProc"/> is null.
				/// </exception>
				public MessageWindow(WindowProcedure wndProc)
					: this(new[] { wndProc })
				{

				}


				/// <summary>
				/// The handle to the window that will receive the messages.
				/// </summary>
				public IntPtr Handle
				{
					get;
				}
				/// <summary>
				/// The window procedures for the message window, in the order
				/// to which they are deferred.
				/// </summary>
				public IList<WindowProcedure> Procedures
				{
					get;
				}


				/// <summary>
				/// Releases the unmanaged resources held by the instance.
				/// </summary>
				public void Dispose()
				{
					if (_disposed)
						return;

					if (!DestroyWindow(this.Handle) ||
						!UnregisterClass((IntPtr)_windowAtom, IntPtr.Zero))
					{
						throw Helpers.LastWin32Exception();
					}

					this.Procedures.Clear();

					_disposed = true;
				}
			}
		}
	}
}
