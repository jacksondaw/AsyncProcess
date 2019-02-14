using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;
using AsyncProcess;
using System.Diagnostics;

namespace AsyncProcess.Tests
{
    [TestClass]
    public class ProcessTaskTests
    {
        private CancellationToken cancellationToken;

        [TestInitialize]
        public void TestInitialize()
        {
            var tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
        }

        [TestMethod]
        public async Task RunAsync_ErrorCodePropregated()
        {
            //Arrange
            using (var temp = new TemporaryFile("bat"))
            {
                using (var innerFile = new TemporaryFile("bat"))
                {
                    var batchScript = @"
                        @echo off
                        exit /B 1
                    ";

                    File.WriteAllText(innerFile.FileName, batchScript);

                    var outerScript = $@"
                        @echo off
                        call {innerFile.FileName}
                        @if errorlevel 1 goto err
                        @GOTO DONE 
                        :ERR 
                        EXIT/B 1
                        @GOTO COMPLETE
                        :DONE
                        :COMPLETE
                    ";

                    File.WriteAllText(temp.FileName, outerScript);

                    var processTask = new ProcessTask(temp.FileName, cancellationToken);

                    //Act
                    var result = await processTask.RunAsync();

                    //Assert
                    Assert.AreEqual(1, result);
                }
            }
        }

        [TestMethod]
        public async Task RunAsync_HandlesNestedScripts()
        {
            //Arrange
            using (var temp = new TemporaryFile("bat"))
            {
                using (var innerFile = new TemporaryFile("bat"))
                {
                    var batchScript = $@"
                        @echo off 
                        echo Testing
                    ";

                    File.WriteAllText(innerFile.FileName, batchScript);

                    var outerScript = $@"
                        @echo off
                        call {innerFile.FileName}
                        @if errorlevel 1 goto err
                        @GOTO DONE 
                        :ERR 
                        EXIT/B 1
                        :DONE
                    ";

                    File.WriteAllText(temp.FileName, outerScript);

                    var processTask = new ProcessTask(temp.FileName, cancellationToken);

                    //Act
                    var result = await processTask.RunAsync();

                    //Assert
                    Assert.AreEqual(0, result);
                }
            }
        }

        [TestMethod]
        public async Task RunAsync_ReturnsSuccessCode()
        {
            //Arrange
            using(var tempFile = new TemporaryFile("bat"))
            {
                var batchScript = @"
                    @echo off
                    EXIT /B 0
                ";

                File.WriteAllText(tempFile.FileName, batchScript);

                var processTask = new ProcessTask(tempFile.FileName, cancellationToken);

                
                //Act
                var result = await processTask.RunAsync();

                //Assert
                Assert.AreEqual(0, result);
            }
        }

        [TestMethod]
        public async Task RunAsync_ReturnErrorCode()
        {
            using(var tempFile = new TemporaryFile("bat"))
            {
                var batchScript = @"
                    @echo off 
                    EXIT /B 1
                ";

                File.WriteAllText(tempFile.FileName, batchScript);

                var processTask = new ProcessTask(tempFile.FileName, cancellationToken);
                var result = await processTask.RunAsync();

                Assert.AreEqual(1, result);
            }
        }

        [TestMethod]
        public async Task RunAsync_RaisesOutputEvent()
        {
            //Arrange
            using(var tempFile = new TemporaryFile("bat"))
            {
                var batchScript = @"
                    @echo off
                    ECHO 'Test Output'
                    EXIT /B 0
                ";

                File.WriteAllText(tempFile.FileName, batchScript);

                var processTask = new ProcessTask(tempFile.FileName, cancellationToken);
                processTask.OutputReceived += (sender, args) => {
                    var e = args as DataReceivedEventArgs;
                    Assert.IsNotNull(e);
                    Assert.AreEqual("Test", e.Data);
                };
                
                //Act
                var result = await processTask.RunAsync();

                //Assert
                Assert.AreEqual(0, result);
            }
        }
    }
}
