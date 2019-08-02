using System;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Interop;
using PassWinmenu.Utilities.ExtensionMethods;
using Xunit;

namespace PassWinmenuTests.Utilities.ExtensionMethods
{
	public class KeyEventArgsExtensionsTests
	{
		private const string Category = "Utilities: KeyEventArgs extensions";

		[StaFact, TestCategory(Category)]
		public void _KeyEventArgs_HasMethod_SetRepeat()
		{
			// The [KeyEventArgs] class has a private method used to set its
			// [IsRepeat] property's backing field. In order to properly test
			// other components using the [DummyKeyEventSource], we need to be
			// able to indicate whether a keypress is a repeat.
			//
			// As the method is private, we need to use reflection. This test
			// will fail if we are unable to access the [SetRepeat] method and
			// use it to change the [IsRepeat] property.

			var keArgs = new KeyEventArgs(
				keyboard: Keyboard.PrimaryDevice,
				inputSource: new HwndSource(0, 0, 0, 0, 0, String.Empty, IntPtr.Zero),
				timestamp: 0,
				key: Key.A
			);

			Assert.NotNull(keArgs);
			Assert.False(keArgs.IsRepeat);

			var setRepeatInfo = typeof(KeyEventArgs).GetMethod(
				name: "SetRepeat",
				bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance
			);

			setRepeatInfo.Invoke(keArgs, new object[] {true});

			Assert.True(keArgs.IsRepeat);
		}

		[StaFact, TestCategory(Category)]
		public void SetRepeat_ThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(
				() => KeyEventArgsExtensions.SetRepeat(null, true)
			);

			Assert.Throws<ArgumentNullException>(
				() => KeyEventArgsExtensions.SetRepeat(null, false)
			);
		}

		[StaFact, TestCategory(Category)]
		public void SetRepeat_SetsProperty()
		{
			var keArgs = new KeyEventArgs(
				keyboard: Keyboard.PrimaryDevice,
				inputSource: new HwndSource(0, 0, 0, 0, 0, String.Empty, IntPtr.Zero),
				timestamp: 0,
				key: Key.A
			);

			Assert.False(keArgs.IsRepeat);

			KeyEventArgsExtensions.SetRepeat(keArgs, true);

			Assert.True(keArgs.IsRepeat);

			KeyEventArgsExtensions.SetRepeat(keArgs, false);

			Assert.False(keArgs.IsRepeat);
		}

		[StaFact, TestCategory(Category)]
		public void SetRepeat_ReturnsArg()
		{
			var keArgs = new KeyEventArgs(
				keyboard: Keyboard.PrimaryDevice,
				inputSource: new HwndSource(0, 0, 0, 0, 0, String.Empty, IntPtr.Zero),
				timestamp: 0,
				key: Key.A
			);

			Assert.Same(keArgs, keArgs.SetRepeat(true));
			Assert.Same(keArgs, keArgs.SetRepeat(false));
		}
	}
}
