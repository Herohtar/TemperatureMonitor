using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TemperatureMonitor.Utilities
{
    /// <summary>
    /// Base class for members implementing <see cref="IDisposable"/>.
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        #region Fields

        private Subject<Unit> whenDisposedSubject;

        #endregion

        #region Desctructors

        /// <summary>
        /// Finalizes an instance of the <see cref="Disposable"/> class. Releases unmanaged
        /// resources and performs other cleanup operations before the <see cref="Disposable"/>
        /// is reclaimed by garbage collection. Will run only if the
        /// Dispose method does not get called.
        /// </summary>
        ~Disposable()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the when errors changed observable event. Occurs when the validation errors have changed for a property or for the entire object.
        /// </summary>
        /// <value>
        /// The when errors changed observable event.
        /// </value>
        public IObservable<Unit> WhenDisposed
        {
            get
            {
                if (IsDisposed)
                {
                    return Observable.Return(Unit.Default);
                }
                else
                {
                    if (whenDisposedSubject == null)
                    {
                        whenDisposedSubject = new Subject<Unit>();
                    }

                    return whenDisposedSubject.AsObservable();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Disposable"/> is disposed.
        /// </summary>
        /// <value><c>true</c> if disposed; otherwise, <c>false</c>.</value>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose all managed and unmanaged resources.
            Dispose(true);

            // Take this object off the finalization queue and prevent finalization code for this
            // object from executing a second time.
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Disposes the managed resources implementing <see cref="IDisposable"/>.
        /// </summary>
        protected virtual void DisposeManaged()
        {
        }

        /// <summary>
        /// Disposes the unmanaged resources implementing <see cref="IDisposable"/>.
        /// </summary>
        protected virtual void DisposeUnmanaged()
        {
        }

        /// <summary>
        /// Throws a <see cref="ObjectDisposedException"/> if this instance is disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources, called from the finalizer only.</param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!IsDisposed)
            {
                // If disposing managed and unmanaged resources.
                if (disposing)
                {
                    DisposeManaged();
                }

                DisposeUnmanaged();

                IsDisposed = true;

                if (whenDisposedSubject != null)
                {
                    // Raise the WhenDisposed event.
                    whenDisposedSubject.OnNext(Unit.Default);
                    whenDisposedSubject.OnCompleted();
                    whenDisposedSubject.Dispose();
                }
            }
        }

        #endregion
    }
}
