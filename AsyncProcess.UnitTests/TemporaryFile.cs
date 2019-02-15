using System;
using System.IO;

namespace AsyncProcess.Tests
{
    public class TemporaryFile : IDisposable
    {
        private string _temporaryFileName;

        public TemporaryFile(string extension, string tempDirectory = null)
        {
            var directory = tempDirectory ?? Directory.GetCurrentDirectory();

            _temporaryFileName = Path.Combine(directory, $"{Path.GetRandomFileName()}.{extension}");
        }

        public string FileName => _temporaryFileName;

        private void Initialize()
        {
            Remove();
        }

        private void Remove()
        {
            if (File.Exists(_temporaryFileName))
                File.Delete(_temporaryFileName);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Remove();
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
