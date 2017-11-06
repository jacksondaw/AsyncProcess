using System;
using System.IO;

namespace AsyncProcess.Tests
{
    public class TemporaryDirectory : IDisposable
    {
        private string _tempDirectoryPath;

        public TemporaryDirectory(string startingDirectory = null)
        {
            var baseDirectory = string.IsNullOrEmpty(startingDirectory) ? Path.GetTempPath() : startingDirectory;

            _tempDirectoryPath = Path.Combine(baseDirectory, Path.GetRandomFileName());

            Initialize();
        }

        public string TempPath => _tempDirectoryPath;

        private void Initialize()
        {
            RemoveDirectory();

            Directory.CreateDirectory(_tempDirectoryPath);
        }

        private void RemoveDirectory()
        {
            if (Directory.Exists(_tempDirectoryPath))
                Directory.Delete(_tempDirectoryPath, true);
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
                    RemoveDirectory();
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
