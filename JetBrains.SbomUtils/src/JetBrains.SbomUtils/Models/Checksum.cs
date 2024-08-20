namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// A Checksum is value that allows the contents of a file to be authenticated.
  /// Even small changes to the content of the file will change its checksum.
  /// This class allows the results of a variety of checksum and cryptographic message digest algorithms to be represented.
  /// </summary>
  public class Checksum
  {
    /// <summary>
    /// Algorighm for Checksums.
    /// </summary>
    public ChecksumAlgorithm Algorithm { get; set; }

    /// <summary>
    /// The checksumValue property provides a lower case hexidecimal encoded digest value produced using a specific algorithm.
    /// </summary>
    public string ChecksumValue { get; set; } = String.Empty;
  }
}