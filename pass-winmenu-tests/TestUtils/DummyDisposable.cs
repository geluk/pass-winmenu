using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassWinmenu.TestUtils
{
    /// <summary>
    /// Implements a dummy <see cref="IDisposable"/> where the method
    /// <see cref="IDisposable.Dispose"/> is a provided <see cref="Action"/>.
    /// </summary>
    public sealed class DummyDisposable
        : IDisposable
    {
        private readonly Action _action;

        /// <summary>
        /// Creates a new instance of the dummy <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="action">
        /// The method which is to be used as the backing method for the
        /// implementation of <see cref="IDisposable.Dispose"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is null.
        /// </exception>
        public DummyDisposable(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            _action = action;
        }


        void IDisposable.Dispose() => _action();
    }
}
