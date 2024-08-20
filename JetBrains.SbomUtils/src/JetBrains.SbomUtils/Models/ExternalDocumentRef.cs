namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// Identify any external SPDX documents referenced within this SPDX document. Intent: SPDX elements may be related to other SPDX elements. These elements may be in external SPDX documents. An SPDX element could be a file, package, or SPDX document.
  /// </summary>
  public class ExternalDocumentRef
  {
    /// <summary>
    /// externalDocumentId is a string containing letters, numbers, ., - and/or + which uniquely identifies an external document within this document.
    /// </summary>
    public string ExternalDocumentId { get; set; } = String.Empty;

    public Checksum Checksum { get; set; } = new();

    public string SpdxDocument { get; set; } = String.Empty;
  }
}