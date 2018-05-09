using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PassWinmenu.Utilities
{
    /// <summary>
    /// Tests the <see cref="Disposable"/> class.
    /// </summary>
    [TestClass]
    public class DisposableTests
    {
        private const string Category = "Utilities: IDisposable wrapper";

        [TestMethod, TestCategory(Category)]
        public void _Is_IDisposable()
        {
            var d = new Disposable(() => {});

            Assert.IsTrue(d is IDisposable);
        }

        [TestMethod, TestCategory(Category)]
        public void Throws_OnNullAction()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new Disposable(null, true));

            Assert.ThrowsException<ArgumentNullException>(
                () => new Disposable(null, false));


            Assert.ThrowsException<ArgumentNullException>(
                () => new Disposable(null));
        }

        [TestMethod, TestCategory(Category)]
        public void Dispose_DisallowMultipleDispose()
        {
            int i = 0;

            var d = new Disposable(() => i++, allowMultipleDispose: false);

            d.Dispose();
            d.Dispose();

            Assert.AreEqual(1, i);
        }
        [TestMethod, TestCategory(Category)]
        public void Dispose_AllowMultipleDispose()
        {
            int i = 0;

            var d = new Disposable(() => i++, allowMultipleDispose: true);

            d.Dispose();
            d.Dispose();

            Assert.AreEqual(2, i);
        }
    }
}
