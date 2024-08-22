using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils;

public interface IDiskOperations
{
  (ICollection<string> Files, ICollection<string> IgnoredFiles) GetFilesFromInstallationDirectory(IEnumerable<string> exemptions);

  Dictionary<ChecksumAlgorithm, byte[]> CalculateHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms);
}