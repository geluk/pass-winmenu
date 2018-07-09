using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PassWinmenu.Hotkeys
{
    [TestClass]
    public class KeyEventSourceRegistrarTests
    {
        private const string Category = "Hotkeys: KeyEventSource";

        private static readonly DummyKeyEventSource _dummyEventSource;

        static KeyEventSourceRegistrarTests()
        {
            _dummyEventSource = new DummyKeyEventSource();
        }


        private IHotkeyRegistrar _registrar;

        // Run before each test
        [TestInitialize]
        public void TestInit()
        {
            _registrar = HotkeyRegistrars.KeyEventSource.Create(_dummyEventSource);
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


        [TestMethod, TestCategory(Category)]
        public void Register_ThrowsOnNullHandler()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _registrar.Register(
                    modifierKeys: ModifierKeys.Control,
                    key:          Key.P,
                    repeats:      false,
                    firedHandler: null
                    )
                );
        }

        [TestMethod, TestCategory(Category)]
        public void Register_KeyEquivalence_Control()
        {
            int firedCount = 0;

            var deregisterer = _registrar.Register(
                modifierKeys:   ModifierKeys.Control,
                key:            Key.A, 
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            var left  = new[] { Key.LeftCtrl,  Key.A };
            var right = new[] { Key.RightCtrl, Key.A };

            _dummyEventSource.Actuate(left);
            _dummyEventSource.Release(left);

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(right);
            _dummyEventSource.Release(right);

            Assert.AreEqual(2, firedCount);
        }
        [TestMethod, TestCategory(Category)]
        public void Register_KeyEquivalence_Shift()
        {
            int firedCount = 0;

            var deregisterer = _registrar.Register(
                modifierKeys:   ModifierKeys.Alt,
                key:            Key.X, 
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            var left  = new[] { Key.LeftAlt,  Key.X };
            var right = new[] { Key.RightAlt, Key.X };

            _dummyEventSource.Actuate(left);
            _dummyEventSource.Release(left);

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(right);
            _dummyEventSource.Release(right);

            Assert.AreEqual(2, firedCount);
        }
        [TestMethod, TestCategory(Category)]
        public void Register_KeyEquivalence_Alt()
        {
            int firedCount = 0;

            var deregisterer = _registrar.Register(
                modifierKeys:   ModifierKeys.Shift,
                key:            Key.M, 
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            var left  = new[] { Key.LeftShift,  Key.M };
            var right = new[] { Key.RightShift, Key.M };

            _dummyEventSource.Actuate(left);
            _dummyEventSource.Release(left);

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(right);
            _dummyEventSource.Release(right);

            Assert.AreEqual(2, firedCount);
        }

        [TestMethod, TestCategory(Category)]
        public void Register_CombinationSimple_NoRepeat()
        {
            int firedCount = 0;

            var deregisterer = _registrar.Register(
                modifierKeys:   ModifierKeys.Control,
                key:            Key.A, 
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            var keys = new[] { Key.LeftCtrl, Key.A };

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Release(keys);

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Actuate(keys, isRepeat: true);
            _dummyEventSource.Release(keys);

            // Should ignore repeat actuations
            Assert.AreEqual(2, firedCount);

            deregisterer.Dispose();

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Release(keys);

            Assert.AreEqual(2, firedCount);
        }
        [TestMethod, TestCategory(Category)]
        public void Register_CombinationSimple_Repeats()
        {
            int firedCount = 0;

            var deregisterer = _registrar.Register(
                modifierKeys:   ModifierKeys.Alt,
                key:            Key.X,
                repeats:        true,
                firedHandler:   (s, e) => firedCount++
                );

            var keys = new[] { Key.RightAlt, Key.X };

            // Actuate multiple times, indicating that the keys are repeats
            // after the first time, to emulate a held-down key combo.

            Assert.AreEqual(0, firedCount);

            _dummyEventSource.Actuate(keys, isRepeat: false);

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(keys, isRepeat: true);

            Assert.AreEqual(2, firedCount);

            _dummyEventSource.Release(keys);

            Assert.AreEqual(2, firedCount);

            deregisterer.Dispose();

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Release(keys);

            Assert.AreEqual(2, firedCount);

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Actuate(keys, isRepeat: true);
            _dummyEventSource.Release(keys);

            Assert.AreEqual(2, firedCount);
        }

        [TestMethod, TestCategory(Category)]
        public void Register_CombinationComplex_NoRepeat()
        {
            int firedCount = 0;

            var deregisterer = _registrar.Register(
                modifierKeys:   ModifierKeys.Control | ModifierKeys.Alt,
                key:            Key.A, 
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            var keys = new[] { Key.LeftCtrl, Key.RightAlt, Key.A };

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Release(keys);

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Actuate(keys, isRepeat: true);
            _dummyEventSource.Release(keys);

            Assert.AreEqual(2, firedCount);

            deregisterer.Dispose();

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Actuate(keys, isRepeat: true);
            _dummyEventSource.Release(keys);

            Assert.AreEqual(2, firedCount);
        }
        [TestMethod, TestCategory(Category)]
        public void Register_CombinationComplex_Repeats()
        {
            int firedCount = 0;

            var deregisterer = _registrar.Register(
                modifierKeys:   ModifierKeys.Control | ModifierKeys.Shift,
                key:            Key.X, 
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            var keys = new[] { Key.RightCtrl, Key.LeftShift, Key.X };

            _dummyEventSource.Actuate(keys, isRepeat: false);

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(keys, isRepeat: true);

            Assert.AreEqual(2, firedCount);

            _dummyEventSource.Release(keys);

            Assert.AreEqual(2, firedCount);

            deregisterer.Dispose();

            _dummyEventSource.Actuate(keys, isRepeat: false);

            Assert.AreEqual(2, firedCount);

            _dummyEventSource.Actuate(keys, isRepeat: true);

            Assert.AreEqual(2, firedCount);
        }

        [TestMethod, TestCategory(Category)]
        public void Register_UnreliableActuation()
        {
            // Unreliable actuation, i.e. where the user would actuate keys,
            // release some of them, then actuate again to finally make the
            // full key combination.
            //
            // Key repeats are thrown in to emulate a key being continuously
            // held down before the combination is fully made.

            int firedCount = 0;

            _registrar.Register(
                modifierKeys:   ModifierKeys.Control | ModifierKeys.Alt,
                key:            Key.A, 
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            _dummyEventSource.Actuate(Key.LeftCtrl);

            Assert.AreEqual(0, firedCount);

            _dummyEventSource.Actuate(Key.LeftAlt);
            _dummyEventSource.Actuate(Key.LeftCtrl, isRepeat: true);

            Assert.AreEqual(0, firedCount);

            _dummyEventSource.Release(Key.LeftAlt);
            _dummyEventSource.Actuate(Key.LeftCtrl, isRepeat: true);

            Assert.AreEqual(0, firedCount);

            _dummyEventSource.Actuate(Key.LeftCtrl, isRepeat: true);

            Assert.AreEqual(0, firedCount);

            _dummyEventSource.Actuate(new[] { Key.LeftAlt, Key.A });

            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(
                new[] { Key.LeftCtrl, Key.LeftAlt, Key.A }, isRepeat: true
                );

            Assert.AreEqual(1, firedCount);
        }

        // The registrar should enforce that the combination is only
        // triggered when the keys are pressed in order.
        //
        // The correct order is modifier keys first, followed by the
        // regular key. The order of the modifier keys is ignored. This
        // is the behaviour confirmed by experiment for the Windows
        // hotkey registrar.

        [TestMethod, TestCategory(Category)]
        public void Register_EnforceCombinationOrder_Basic()
        {
            // Basic test, correct order works and wrong order doesn't with a
            // simple combination.

            int firedCount = 0;

            _registrar.Register(
                ModifierKeys.Control, Key.A, repeats: false,
                firedHandler: (s, e) => firedCount++
                );

            var keys = new[] { Key.LeftCtrl, Key.A };

            _dummyEventSource.Actuate(keys);
            _dummyEventSource.Release(keys);

            // Correct order triggers
            Assert.AreEqual(1, firedCount);

            _dummyEventSource.Actuate(keys.Reverse());
            _dummyEventSource.Release(keys.Reverse());

            // Wrong order does not trigger
            Assert.AreEqual(1, firedCount);
        }
        [TestMethod, TestCategory(Category)]
        public void Register_EnforceCombinationOrder_ModifierOrderAgnostic()
        {
            // It should be possible to actuate the modifier keys in any order
            // and still trigger the hotkey.

            int firedCount = 0;

            _registrar.Register(
                modifierKeys:   ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt,
                key:            Key.A,
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            // All these orders should see the hotkey triggered
            var validOrders = new []
            {
                /*
                 * a b c
                 * c a b
                 * b c a
                 * b a c
                 * a c b
                 * c b a
                 */

                new[] { Key.LeftCtrl,   Key.LeftShift,  Key.LeftAlt,    Key.A },
                new[] { Key.LeftAlt,    Key.LeftCtrl,   Key.LeftShift,  Key.A },
                new[] { Key.LeftShift,  Key.LeftAlt,    Key.LeftCtrl,   Key.A },
                new[] { Key.LeftShift,  Key.LeftCtrl,   Key.LeftAlt,    Key.A },
                new[] { Key.LeftCtrl,   Key.LeftAlt,    Key.LeftShift,  Key.A },
                new[] { Key.LeftAlt,    Key.LeftShift,  Key.LeftCtrl,   Key.A },
            };

            for (int i = 0; i < validOrders.Length; i++)
            {
                _dummyEventSource.Actuate(validOrders[i]);
                _dummyEventSource.Release(validOrders[i]);

                Assert.AreEqual(i + 1, firedCount, $"Index {i}");
            }
        }
        [TestMethod, TestCategory(Category)]
        public void Register_EnforceCombinationOrder_RegularKeyLastOnly()
        {
            // The regular key must always come after the modifier keys in order
            // to trigger the hotkey, and so the hotkey shouldn't be triggered if
            // it's actuated in any order position.

            int firedCount = 0;

            _registrar.Register(
                modifierKeys:   ModifierKeys.Control | ModifierKeys.Shift,
                key:            Key.M,
                repeats:        false,
                firedHandler:   (s, e) => firedCount++
                );

            // Valid orders confirmed in another test
            var invalidOrders = new[]
            {
                new[] { Key.RightCtrl,  Key.M,          Key.RightShift },
                new[] { Key.RightShift, Key.M,          Key.RightCtrl  },
                new[] { Key.M,          Key.RightShift, Key.RightCtrl  },
                new[] { Key.M,          Key.RightCtrl,  Key.RightShift },
            };

            for (int i = 0; i < invalidOrders.Length; i++)
            {
                _dummyEventSource.Actuate(invalidOrders[i]);
                _dummyEventSource.Release(invalidOrders[i]);

                // Invalid orders should never trigger the hotkey
                Assert.AreEqual(0, firedCount, $"Index {i}");
            }
        }
    }
}
