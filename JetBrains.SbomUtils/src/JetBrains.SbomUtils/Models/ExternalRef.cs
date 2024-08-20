namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// An External Reference allows a Package to reference an external source of additional information, metadata, enumerations, asset identifiers, or downloadable content believed to be relevant to the Package.
  /// </summary>
  public class ExternalRef
  {
    /// <summary>
    /// Category for the external reference
    /// </summary>
    public ExternalRefCategory ReferenceCategory { get; set; }

    /// <summary>
    /// The unique string with no spaces necessary to access the package-specific information, metadata, or content within the target location.
    /// The format of the locator is subject to constraints defined by the type.
    /// </summary>
    public string ReferenceLocator { get; set; } = String.Empty;

    /// <summary>
    /// Type of the external reference. These are definined in an appendix in the SPDX specification.
    /// </summary>
    public string ReferenceType { get; set; } = String.Empty;

    public string? Comment { get; set; }
  }
}