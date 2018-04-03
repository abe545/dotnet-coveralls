namespace Dotnet.Coveralls
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
