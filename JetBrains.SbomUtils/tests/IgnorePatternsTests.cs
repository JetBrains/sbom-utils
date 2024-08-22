namespace JetBrains.SbomUtils.Tests;

public class IgnorePatternsTests
{
  [TestCase("test.txt", "test.txt")]
  [TestCase("test.txt", "*.txt")]
  [TestCase("test.txt", "./test.txt")]
  [TestCase("test.txt", "./*.txt")]
  [TestCase("test.txt", ".\\*.txt")]
  [TestCase("test.txt", ".\\test.txt")]
  [TestCase("dir/test.txt", "dir/*")]
  [TestCase("dir/test.txt", "dir\\*")]
  [TestCase("dir/test.txt", "dir/*.txt")]
  [TestCase("dir/test.txt", "dir\\test.*")]
  [TestCase("dir/dir2/test.txt", "dir/**")]
  [TestCase("dir/dir2/test.txt", "dir/**.txt")]
  [TestCase("dir/dir2/test.txt", "dir/dir2**")]
  [TestCase("dir/dir2/test.txt", "dir/dir2\\*")]
  [TestCase("dir/dir2/test.txt", "**\\test.txt")]
  [TestCase("dir/dir2/dir3/test.txt", "dir/**/*.txt")]
  public void FilesThatMatchIgnorePatternShouldBeIgnored(string path, string ignorePattern)
  {
    var patterns = IgnorePatternUtils.CompileIgnorePatterns([ignorePattern]);

    bool isIgnored = IgnorePatternUtils.IsIgnored(path, patterns);

    Assert.True(isIgnored);
  }

  [TestCase("test.txt", "test.dat")]
  [TestCase("test.txt", "*.dat")]
  [TestCase("test.txt", "./test.dat")]
  [TestCase("test.txt", "./*.dat")]
  [TestCase("test.txt", ".\\*.dat")]
  [TestCase("dir/test.txt", "dir/*.dat")]
  [TestCase("dir/test.txt", "dir\\test2.*")]
  [TestCase("dir/dir2/test.txt", "dir/*.txt")]
  [TestCase("dir/test.txt", "dir/**/*.txt")]
  public void FilesThatDontMatchIgnorePatternShouldntBeIgnored(string path, string ignorePattern)
  {
    var patterns = IgnorePatternUtils.CompileIgnorePatterns([ignorePattern]);

    bool isIgnored = IgnorePatternUtils.IsIgnored(path, patterns);

    Assert.False(isIgnored);
  }
}