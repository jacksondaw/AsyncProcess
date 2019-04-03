# Async Process

A simple adapter that allows a process to be executed asynchronously. Developed with .NET Standard 2.0.

## Installation
   
   PS> Install-Package AsyncProcess.Net 

   PS> dotnet add package AsyncProcess.Net

# Example

More examples can be found in the test project. 

```CSharp
    var tokenSource = new CancellationTokenSource();
    var filename = "C:\Users\jack\Desktop\AwesomeProgram.exe";
    var processTask = new ProcessTask(filename, tokenSource.Token);

    processTask.OutputReceived += (sender, obj) => {
        var args = (DataReceivedEventArgs)obj;
        Console.WriteLine(args.Data);
    };
    
    var exitCode = await processTask.RunAsync();
```



