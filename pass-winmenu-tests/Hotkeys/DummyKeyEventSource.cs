using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using PassWinmenu.Hotkeys;
using PassWinmenu.Utilities.ExtensionMethods;

namespace PassWinmenuTests.Hotkeys
{
	/// <summary>
	/// A dummy, manipulable <see cref="IKeyEventSource"/> for use in testing.
	/// </summary>
	public sealed class DummyKeyEventSource
		: IKeyEventSource
	{
		private readonly HwndSource _dummyPresentationSource;


		public DummyKeyEventSource()
		{
			_dummyPresentationSource = new HwndSource(
				0, 0, 0, 0, 0, String.Empty, IntPtr.Zero
			);
		}


		/// <summary>
		/// Triggers a <see cref="KeyDown"/> event for the specified key with the
		/// value of the <see cref="KeyEventArgs.IsRepeat"/> property set to the 
		/// value provided.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="isRepeat"></param>
		public void Actuate(Key key, bool isRepeat)
		{
			var kea = new KeyEventArgs(
				keyboard:    Keyboard.PrimaryDevice,
				inputSource: _dummyPresentationSource,
				timestamp:   0,
				key:         key
				)
			{
				RoutedEvent = Keyboard.KeyDownEvent
			};

			kea.SetRepeat(isRepeat);

			this.KeyDown?.Invoke(this, kea);
		}
		/// <summary>
		/// Triggers a <see cref="KeyDown"/> for the specified key.
		/// </summary>
		public void Actuate(Key key)
		{
			this.Actuate(key, isRepeat: false);
		}
		/// <summary>
		/// Triggers a <see cref="KeyDown"/> event for each of the specified keys
		/// with the value of the <see cref="KeyEventArgs.IsRepeat"/> property
		/// set to the value provided for that key.
		/// </summary>
		/// <param name="keys"></param>
		public void Actuate(IReadOnlyDictionary<Key, bool> keys)
		{
			foreach (var kvp in keys)
				this.Actuate(kvp.Key, isRepeat: kvp.Value);
		}
		/// <summary>
		/// Triggers a <see cref="KeyDown"/> event for each of the specified keys
		/// with the value of the <see cref="KeyEventArgs.IsRepeat"/> property set
		/// to the specified value for all keys.
		/// </summary>
		/// <param name="keys"></param>
		/// <param name="isRepeat"></param>
		public void Actuate(IEnumerable<Key> keys, bool isRepeat)
		{
			foreach (var key in keys)
				this.Actuate(key, isRepeat);
		}
		/// <summary>
		/// Triggers a <see cref="KeyDown"/> event for each of the specified keys.
		/// </summary>
		public void Actuate(IEnumerable<Key> keys)
		{
			foreach (var key in keys)
				this.Actuate(key);
		}

		/// <summary>
		/// Triggers a <see cref="KeyUp"/> event for the specified key.
		/// </summary>
		public void Release(Key key)
		{
			var kea = new KeyEventArgs(
				keyboard:    Keyboard.PrimaryDevice,
				inputSource: _dummyPresentationSource,
				timestamp:   0,
				key:         key
				)
			{
				RoutedEvent = Keyboard.KeyUpEvent
			};

			this.KeyUp?.Invoke(this, kea);
		}
		/// <summary>
		/// Triggers a <see cref="KeyUp"/> event for the specified keys.
		/// </summary>
		public void Release(IEnumerable<Key> keys)
		{
			foreach (var key in keys)
				this.Release(key);
		}

		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;
	}
}
