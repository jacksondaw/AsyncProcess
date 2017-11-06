using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;

namespace AsyncProcess.Tests
{
    [TestClass]
    public class ProcessTaskTests
    {
        [TestMethod]
        public async Task TestProcessTaskReturnsErrorCode()
        {

            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var filePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.bat");
            try
            {
                var batFile = $"@echo off {System.Environment.NewLine} exit /B 1";

                File.WriteAllText(filePath, batFile);

                var processTask = new ProcessTask(filePath, cancellationToken);
                var result = await processTask.RunAsync();

                Assert.AreEqual(1, result);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public async Task TestNestedBatsReturnsErrorCode()
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            using (var temp = new TemporaryFile("bat"))
            {
                using (var innerFile = new TemporaryFile("bat"))
                {
                    var batFile = $"@echo off {System.Environment.NewLine} exit /B 1";

                    File.WriteAllText(innerFile.FileName, batFile);
                    var builder = new StringBuilder();

                    builder.AppendLine("@echo off");
                    builder.AppendLine($"call {innerFile.FileName}");
                    builder.AppendLine($"@if errorlevel 1 goto err");
                    builder.AppendLine("@GOTO DONE ");
                    builder.AppendLine(":ERR ");
                    builder.AppendLine("EXIT/B 1");
                    builder.AppendLine("@GOTO COMPLETE");
                    builder.AppendLine(":DONE");
                    builder.AppendLine(":COMPLETE");

                    var parentBat = builder.ToString();

                    File.WriteAllText(temp.FileName, parentBat);

                    var processTask = new ProcessTask(temp.FileName, cancellationToken);
                    var result = await processTask.RunAsync();

                    Assert.AreEqual(1, result);
                }
            }
        }

        [TestMethod]
        public async Task TestNestedBatsSuccessful()
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            using (var temp = new TemporaryFile("bat"))
            {
                using (var innerFile = new TemporaryFile("bat"))
                {
                    var batFile = $"@echo off {System.Environment.NewLine} echo Testing";

                    File.WriteAllText(innerFile.FileName, batFile);
                    var builder = new StringBuilder();

                    builder.AppendLine("@echo off");
                    builder.AppendLine($"call {innerFile.FileName}");
                    builder.AppendLine($"@if errorlevel 1 goto err");
                    builder.AppendLine("@GOTO DONE ");
                    builder.AppendLine(":ERR ");
                    builder.AppendLine("EXIT/B 1");
                    builder.AppendLine(":DONE");

                    var parentBat = builder.ToString();

                    File.WriteAllText(temp.FileName, parentBat);

                    var processTask = new ProcessTask(temp.FileName, cancellationToken);
                    var result = await processTask.RunAsync();

                    Assert.AreEqual(0, result);
                }
            }
        }


        [TestMethod]
        public async Task TestProcessTaskReturnsSuccesfulErrorCode()
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var filePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.bat");
            try
            {
                var batFile = $"@echo off {System.Environment.NewLine} EXIT /B 0";

                File.WriteAllText(filePath, batFile);

                var processTask = new ProcessTask(filePath, cancellationToken);
                var result = await processTask.RunAsync();

                Assert.AreEqual(0, result);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public async Task TestProcessTaskReturnsErrorCode1()
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var filePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.bat");
            try
            {
                var batFile = $"@echo off {System.Environment.NewLine} SET %ERRORLEVEL% = 1 {System.Environment.NewLine} EXIT /B %ERRORLEVEL%";

                File.WriteAllText(filePath, batFile);

                var processTask = new ProcessTask(filePath, cancellationToken);
                var result = await processTask.RunAsync();

                Assert.AreEqual(0, result);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }


    }
}
