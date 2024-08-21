using System.Security.Cryptography;
using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils;

public class DiskOperations : IDiskOperations
{
  public (string[] Files, ICollection<string> IgnoredFiles) GetFilesFromInstallationDirectory(string installationPath, IEnumerable<string> exemptions)
  {
    var allFiles = Directory.GetFiles(installationPath, "*", new EnumerationOptions() { RecurseSubdirectories = true })
      .Select(Path.GetFullPath).ToHashSet();
    var ignoredFiles = new List<string>();

    string[] recursiveDirectoryPatterns = new string[] { "*/", "*\\", "**/", "**\\" };

    foreach (var exemptionPattern in exemptions)
    {
      var pattern = exemptionPattern;
      bool recurseSubdirectories = false;

      foreach (var recursiveDirectoryPattern in recursiveDirectoryPatterns)
      {
        if (exemptionPattern.StartsWith(recursiveDirectoryPattern))
        {
          recurseSubdirectories = true;
          pattern = exemptionPattern.Substring(recursiveDirectoryPattern.Length);
          break;
        }
      }

      var exemptedFiles = Directory.GetFiles(installationPath, pattern,
        new EnumerationOptions() { RecurseSubdirectories = recurseSubdirectories });

      foreach (var exemptedFile in exemptedFiles)
      {
        var fullPath = Path.GetFullPath(exemptedFile);
        if (allFiles.Remove(fullPath))
          ignoredFiles.Add(Path.GetRelativePath(installationPath, fullPath));
      }
    }

    return (allFiles.ToArray(), ignoredFiles);
  }

  public Dictionary<ChecksumAlgorithm, byte[]> CalculateHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms)
  {
    Dictionary<ChecksumAlgorithm, byte[]> hashes = new Dictionary<ChecksumAlgorithm, byte[]>();

    using (var file = System.IO.File.OpenRead(installationFile))
    {
      foreach (var algorithm in algorithms)
      {
        var hashOnDisk = CreateHashAlgorithm(algorithm).WithDispose(a => a.ComputeHash(file.Rewind()));
        hashes.Add(algorithm, hashOnDisk);
      }
    }

    return hashes;
  }

  protected static HashAlgorithm CreateHashAlgorithm(ChecksumAlgorithm algorithm) => algorithm switch
  {
    ChecksumAlgorithm.SHA1 => SHA1.Create(),
    ChecksumAlgorithm.SHA256 => SHA256.Create(),
    ChecksumAlgorithm.SHA384 => SHA384.Create(),
    ChecksumAlgorithm.SHA512 => SHA512.Create(),
    _ => throw new NotSupportedException($"Hash algorithm {algorithm} is not supported"),
  };
}