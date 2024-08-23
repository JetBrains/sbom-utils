using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils
{
  public class HashMismatch
  {
    public Checksum FaultedChecksum { get; }
    public byte[] ActualHash { get; }

    public HashMismatch(Checksum faultedChecksum, byte[] actualHash)
    {
      FaultedChecksum = faultedChecksum;
      ActualHash = actualHash;
    }
  }

  public class HashVerificationFailure
  {
    public FileInfo FileInfo { get; }
    public HashMismatch[] HashMismatches { get; }

    public HashVerificationFailure(FileInfo fileInfo, HashMismatch[] hashMismatches)
    {
      FileInfo = fileInfo;
      HashMismatches = hashMismatches;
    }
  }
}

