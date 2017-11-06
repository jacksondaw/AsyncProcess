namespace AsyncProcess
{
    public static class StringExtensions
    {
        public static string ToQuoted(this string input)
        {
            return $"\"{input}\"";
        }
    }
}
