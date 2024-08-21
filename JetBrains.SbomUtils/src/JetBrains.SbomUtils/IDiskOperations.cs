using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils;

public interface IDiskOperations
{
  (string[] Files, ICollection<string> IgnoredFiles) GetFilesFromInstallationDirectory(string installationPath, IEnumerable<string> exemptions);

  Dictionary<ChecksumAlgorithm, byte[]> CalculateHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms);
}