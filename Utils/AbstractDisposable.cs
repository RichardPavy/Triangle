namespace Triangle.Utils
{
    using System;

    public abstract class AbstractDisposable : IDisposable
    {
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    DisposeManagedDependencies();
                }

                DisposeUnmanagedDependencies();
                this.disposedValue = true;
            }
        }

        protected virtual void DisposeUnmanagedDependencies()
        {
        }

        protected virtual void DisposeManagedDependencies()
        {
        }

        ~AbstractDisposable()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
