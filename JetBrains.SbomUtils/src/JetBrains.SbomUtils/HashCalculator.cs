using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils;

public static class HashCalculator
{
  public static Dictionary<ChecksumAlgorithm, byte[]> ComputeHashes(Stream inputStream, IEnumerable<ChecksumAlgorithm> algorithms)
  {
    Dictionary<ChecksumAlgorithm, HashAlgorithm>? hashes = null;
    try
    {
      hashes = algorithms.ToDictionary(a => a, CreateHashAlgorithm);

      byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);

      int bytesRead;
      while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
      {
        foreach (var hashAlgorithm in hashes.Values)
          hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
      }

      foreach (var hashAlgorithm in hashes.Values)
        hashAlgorithm.TransformFinalBlock(buffer, 0, 0);

      ArrayPool<byte>.Shared.Return(buffer, clearArray: true);

      return hashes.ToDictionary(a => a.Key, a => a.Value.Hash);
    }
    finally
    {
      if (hashes != null)
        foreach (var hashAlgorithm in hashes.Values)
          hashAlgorithm.Dispose();
    }
  }

  public static HashAlgorithm CreateHashAlgorithm(ChecksumAlgorithm algorithm) => algorithm switch
  {
    ChecksumAlgorithm.SHA1 => SHA1.Create(),
    ChecksumAlgorithm.SHA256 => SHA256.Create(),
    ChecksumAlgorithm.SHA384 => SHA384.Create(),
    ChecksumAlgorithm.SHA512 => SHA512.Create(),
    _ => throw new NotSupportedException($"Hash algorithm {algorithm} is not supported"),
  };

  public static string ToSbomHash(byte[] hash)
  {
    StringBuilder builder = new();

    foreach (var b in hash)
    {
      builder.Append(b.ToString("x2"));
    }

    return builder.ToString();
  }
}