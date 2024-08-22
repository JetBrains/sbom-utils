using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using JetBrains.SbomUtils.Models;
using File = System.IO.File;

namespace JetBrains.SbomUtils;

public class ZipArchiveDiskOperations: IDiskOperations
{
  public string RootPath { get; private set; }

  public static bool IsSupported(string path)
  {
    if (!File.Exists(path))
      return false;

    try
    {
      using (ZipFile.OpenRead(path)) { }
    }
    catch (Exception)
    {
      return false;
    }

    return true;
  }

  public ZipArchiveDiskOperations(string path)
  {
    RootPath = path;
  }

  public (ICollection<string> Files, ICollection<string> IgnoredFiles) GetFilesFromInstallationDirectory(IEnumerable<string> exemptions)
  {
    using ZipArchive zipArchive = ZipFile.OpenRead(RootPath);

    List<string> files = new();
    List<string> ignoredFiles = new();

    var ignoreRegexes = IgnorePatternUtils.CompileIgnorePatterns(exemptions);

    foreach (var zipArchiveEntry in zipArchive.Entries)
    {
      var normalizedPath = Utils.NormalizePath(zipArchiveEntry.FullName);

      if (IgnorePatternUtils.IsIgnored(normalizedPath, ignoreRegexes))
        ignoredFiles.Add(normalizedPath);
      else
        files.Add(normalizedPath);
    }

    return (files, ignoredFiles);
  }

  public Dictionary<ChecksumAlgorithm, byte[]> CalculateHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms)
  {
    using ZipArchive zipArchive = ZipFile.OpenRead(RootPath);

    var entry = zipArchive.GetEntry(installationFile.Replace('\\', '/'));

    if (entry == null)
      throw new FileNotFoundException($"File {installationFile} not found");

    using var stream = entry.Open();

    return HashCalculator.ComputeHashes(stream, algorithms);
  }


}