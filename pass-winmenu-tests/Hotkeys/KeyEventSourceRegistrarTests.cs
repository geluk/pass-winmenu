using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PassWinmenu.Hotkeys
{
    [TestClass]
    public class KeyEventSourceRegistrarTests
    {
        private const string Category = "Hotkeys: KeyEventSource";

        private static DummyKeyEventSource _dummyEventSource;

        // Run before each test
        [TestInitialize]
        public void TestInit()
        {
            _dummyEventSource = new DummyKeyEventSource();
        }


        [TestMethod, TestCategory(Category)]
        public void Create_IKeyEventSource_ThrowsOnNullSource()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => HotkeyRegistrars.KeyEventSource.Create(null)
                );
        }

        [TestMethod, TestCategory(Category)]
        public void Create_TSource_ThrowsOnNullSource()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => HotkeyRegistrars.KeyEventSource.Create<object>(
                    null, o => throw new InvalidOperationException()
                    )
                );
        }
        [TestMethod, TestCategory(Category)]
        public void Create_TSource_ThrowsOnNullAdaptor()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => HotkeyRegistrars.KeyEventSource.Create<object>(
                    new object(), null
                    )
                );
        }
        [TestMethod, TestCategory(Category)]
        public void Create_TSource_ThrowsOnFailedAdaptation()
        {
            Assert.ThrowsException<ArgumentException>(
                () => HotkeyRegistrars.KeyEventSource.Create<object>(
                    new object(), o => null
                    )
                );
        }
    }
}
