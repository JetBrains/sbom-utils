using System.IO.Compression;
using JetBrains.SbomUtils.Models;
using File = System.IO.File;

namespace JetBrains.SbomUtils;

public class ZipArchiveSoftwareProvider: IInstalledSoftwareProvider
{
  private readonly ZipArchive _zipArchive;

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

  public ZipArchiveSoftwareProvider(Stream stream)
  {
    _zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
  }

  public ZipArchiveSoftwareProvider(Stream stream, bool leaveOpen)
  {
    _zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
  }

  public ZipArchiveSoftwareProvider(string path)
  {
    _zipArchive = ZipFile.OpenRead(path);
  }

  public (ICollection<string> Files, ICollection<string> IgnoredFiles) GetFiles(IEnumerable<string> ignorePatterns)
  {
    List<string> files = new();
    List<string> ignoredFiles = new();

    var ignoreRegexes = IgnorePatternUtils.CompileIgnorePatterns(ignorePatterns);

    foreach (var zipArchiveEntry in _zipArchive.Entries)
    {
      var normalizedPath = Utils.NormalizePath(zipArchiveEntry.FullName);

      if (IgnorePatternUtils.IsIgnored(normalizedPath, ignoreRegexes))
        ignoredFiles.Add(normalizedPath);
      else
        files.Add(normalizedPath);
    }

    return (files, ignoredFiles);
  }

  public Dictionary<ChecksumAlgorithm, byte[]> CalculateFileHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms)
  {
    var entry = _zipArchive.GetEntry(installationFile.Replace('\\', '/'));

    if (entry == null)
      throw new FileNotFoundException($"File {installationFile} not found");

    using var stream = entry.Open();

    return HashCalculator.ComputeHashes(stream, algorithms);
  }

  public void Dispose()
  {
    _zipArchive.Dispose();
  }
}