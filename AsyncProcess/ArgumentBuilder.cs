using System.IO;
using System.Text;

namespace AsyncProcess
{
    public class ArgumentBuilder
    {
        public bool IsPath(string argument)
        {
            if (FirstCharacterIsQuote(argument)) return false;

            try
            {
                if (Directory.Exists(argument)) return true;
            }
            catch { }

            try
            {
                var fileInfo = new FileInfo(argument);
                if (fileInfo.Exists) return true;
            }
            catch { }

            var getFullPath = Path.GetFullPath(argument);
            return getFullPath == argument;
        }

        private bool FirstCharacterIsQuote(string input)
        {
            var firstCharacter = input.Substring(0, 1);
            var firstAsciiValue = Encoding.ASCII.GetBytes(input)[0];
            return firstAsciiValue == 34;
        }

        public string Build(params string[] arguments)
        {
            var argumentBuilder = new StringBuilder();

            foreach (var arg in arguments)
            {
                var argument = IsPath(arg) ? arg.ToQuoted() : arg;

                argumentBuilder.Append($"{argument} ");
            }
            return argumentBuilder.ToString().Trim();
        }
    }
}
