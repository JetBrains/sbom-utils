using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils;

public interface IInstalledSoftwareProvider: IDisposable
{
  (ICollection<string> Files, ICollection<string> IgnoredFiles) GetFiles(IEnumerable<string> ignorePatterns);

  Dictionary<ChecksumAlgorithm, byte[]> CalculateFileHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms);
}