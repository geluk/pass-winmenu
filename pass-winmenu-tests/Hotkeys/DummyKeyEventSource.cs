using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PassWinmenu.Hotkeys
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
        /// Triggers a <see cref="KeyDown"/> followed by a <see cref="KeyPress"/>
        /// event for the specified key.
        /// </summary>
        public void Actuate(Key key)
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

            this.KeyDown?.Invoke(this, kea);
            this.KeyPress?.Invoke(this, kea);
        }
        /// <summary>
        /// Triggers a <see cref="KeyDown"/> followed by a <see cref="KeyPress"/>
        /// event for each of the specified keys.
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
        public event KeyEventHandler KeyPress;
        public event KeyEventHandler KeyUp;
    }
}
