using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncProcess
{
    public delegate void TaskOutputHandler(object sender, DataReceivedEventArgs args);

    public class ProcessTask : IDisposable
    {
        private static object _processLock = new object();

        private Process _process;

        private CancellationToken _cancellationToken;

        public ProcessTask(string fileName, CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(fileName);

            var startInfo = new ProcessStartInfo(fileName)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                WorkingDirectory = fileInfo.DirectoryName,
                //LoadUserProfile = true
            };

            _process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = startInfo
            };

            _cancellationToken = cancellationToken;
        }

        public ProcessTask(ProcessStartInfo startInfo, CancellationToken cancellationToken)
        {
            _process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = startInfo
            };

            _cancellationToken = cancellationToken;
        }


        public event TaskOutputHandler ErrorReceived;

        public event TaskOutputHandler OutputReceived;

        /// <summary>
        /// Runs process and returns the exit code of said process
        /// </summary>
        /// <param name="arguments"></param>
        /// <exception cref="InvalidOperationException">Thrown in process fails to start</exception>
        /// <returns></returns>
        public virtual Task<int> RunAsync(params string[] arguments)
        {
            if (arguments != null)
            {
                var argumentBuilder = new ArgumentBuilder();
                _process.StartInfo.Arguments = argumentBuilder.Build(arguments);
            }

            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;

            var taskCompletedSource = new TaskCompletionSource<int>();
            _process.Exited += (sender, args) =>
            {
                taskCompletedSource.SetResult(_process.ExitCode);
            };

            var started = _process.Start();

            if (!started)
            {
                throw new InvalidOperationException($"Failed to start process {_process.StartInfo.FileName}");
            }

            if (_process.StartInfo.RedirectStandardOutput)
                _process.BeginOutputReadLine();

            if (_process.StartInfo.RedirectStandardError)
                _process.BeginErrorReadLine();

            return taskCompletedSource.Task;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_processLock)
            {
                if (_cancellationToken.IsCancellationRequested && !_process.HasExited)
                {
                    _process.Kill();
                    return;
                }
            }

            ErrorReceived?.Invoke(sender, e);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_processLock)
            {
                if (_cancellationToken.IsCancellationRequested && !_process.HasExited)
                {
                    _process.Kill();
                    return;
                }
            }

            OutputReceived?.Invoke(sender, e);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_process != null)
                    {
                        _process.OutputDataReceived -= Process_OutputDataReceived;
                        _process.ErrorDataReceived -= Process_ErrorDataReceived;
                        _process.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
