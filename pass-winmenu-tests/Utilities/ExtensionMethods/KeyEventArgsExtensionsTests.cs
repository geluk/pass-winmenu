using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PassWinmenu.Utilities.ExtensionMethods
{
    [TestClass]
    public class KeyEventArgsExtensionsTests
    {
        private const string Category = "Utilities: KeyEventArgs extensions";

        [TestMethod, TestCategory(Category)]
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

            Assert.IsNotNull(keArgs);
            Assert.IsFalse(keArgs.IsRepeat);

            var setRepeatInfo = typeof(KeyEventArgs).GetMethod(
                name: "SetRepeat",
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance
                );

            setRepeatInfo.Invoke(keArgs, new object[] { true });

            Assert.IsTrue(keArgs.IsRepeat);
        }

        [TestMethod, TestCategory(Category)]
        public void SetRepeat_ThrowsOnNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => KeyEventArgsExtensions.SetRepeat(null, true)
                );

            Assert.ThrowsException<ArgumentNullException>(
                () => KeyEventArgsExtensions.SetRepeat(null, false)
                );
        }

        [TestMethod, TestCategory(Category)]
        public void SetRepeat_SetsProperty()
        {
            var keArgs = new KeyEventArgs(
                keyboard: Keyboard.PrimaryDevice,
                inputSource: new HwndSource(0, 0, 0, 0, 0, String.Empty, IntPtr.Zero),
                timestamp: 0,
                key: Key.A
                );

            Assert.IsFalse(keArgs.IsRepeat);

            KeyEventArgsExtensions.SetRepeat(keArgs, true);

            Assert.IsTrue(keArgs.IsRepeat);

            KeyEventArgsExtensions.SetRepeat(keArgs, false);

            Assert.IsFalse(keArgs.IsRepeat);
        }

        [TestMethod, TestCategory(Category)]
        public void SetRepeat_ReturnsArg()
        {
            var keArgs = new KeyEventArgs(
                keyboard: Keyboard.PrimaryDevice,
                inputSource: new HwndSource(0, 0, 0, 0, 0, String.Empty, IntPtr.Zero),
                timestamp: 0,
                key: Key.A
                );

            Assert.AreSame(keArgs, keArgs.SetRepeat(true));
            Assert.AreSame(keArgs, keArgs.SetRepeat(false));
        }
    }
}
