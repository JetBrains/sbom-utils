using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace JetBrains.SbomUtils;

public static class IgnorePatternUtils
{
  public static bool IsIgnored(string entryName, IEnumerable<Regex> ignorePatterns)
  {
    var entryWithProperSeparator = entryName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    foreach (Regex pattern in ignorePatterns)
    {
      if (pattern.IsMatch(entryWithProperSeparator))
        return true;
    }

    return false;
  }

  public static List<Regex> CompileIgnorePatterns(IEnumerable<string> ignorePatterns)
  {
    RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.Singleline;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      regexOptions |= RegexOptions.IgnoreCase;

    List<Regex> ignorePatternRegexes = new();

    foreach (string pattern in ignorePatterns)
    {
      var patternWithProperSeparator = pattern.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

      var currentDirectory = $".{Path.DirectorySeparatorChar}";
      if (patternWithProperSeparator.StartsWith(currentDirectory))
        patternWithProperSeparator = patternWithProperSeparator.Substring(currentDirectory.Length);

      var escapedRegex = Regex.Escape(patternWithProperSeparator);

      string regexPattern = "^" + escapedRegex
        .Replace(@"\*\*", ".*")
        .Replace(@"\*", "[^\\\\/]*")
        .Replace(@"\?", ".") + "$";

      ignorePatternRegexes.Add(new Regex(regexPattern, regexOptions));
    }

    return ignorePatternRegexes;
  }
}