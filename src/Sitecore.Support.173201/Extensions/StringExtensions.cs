namespace Sitecore.Support.Extensions
{
  internal static class StringExtensions
  {
    public static string EmptyToNull([CanBeNull] this string value)
    {
      return string.IsNullOrEmpty(value) ? null : value;
    }
  }
}