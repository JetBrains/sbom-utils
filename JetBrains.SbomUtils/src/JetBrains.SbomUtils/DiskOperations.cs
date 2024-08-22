using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils;

public class DiskOperations : IDiskOperations
{
  public string RootPath { get; private set; }

  public static bool IsSupported(string path) => Directory.Exists(path);

  public DiskOperations(string path)
  {
    RootPath = path;
  }

  public (ICollection<string> Files, ICollection<string> IgnoredFiles) GetFilesFromInstallationDirectory(IEnumerable<string> exemptions)
  {
    var allFiles = Directory.GetFiles(RootPath, "*", new EnumerationOptions() { RecurseSubdirectories = true });

    List<string> files = new();
    List<string> ignoredFiles = new();

    var ignoreRegexes = IgnorePatternUtils.CompileIgnorePatterns(exemptions);

    foreach (var file in allFiles)
    {
      var normalizedPath = Utils.NormalizePath(file);

      if (IgnorePatternUtils.IsIgnored(normalizedPath, ignoreRegexes))
        ignoredFiles.Add(normalizedPath);
      else
        files.Add(normalizedPath);
    }

    return (files, ignoredFiles);
  }

  public Dictionary<ChecksumAlgorithm, byte[]> CalculateHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms)
  {
    using var file = System.IO.File.OpenRead(Path.Combine(RootPath, installationFile));
    return HashCalculator.ComputeHashes(file, algorithms);
  }
}