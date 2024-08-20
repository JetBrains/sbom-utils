namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// Identify who (or what, in the case of a tool) created the SPDX document.
  /// If the SPDX document was created by an individual, indicate the person's name.
  /// If the SPDX document was created on behalf of a company or organization, indicate the entity name.
  /// If the SPDX document was created using a software tool, indicate the name and version for that tool.
  /// If multiple participants or tools were involved, use multiple instances of this field.
  /// Person name or organization name may be designated as “anonymous” if appropriate.
  /// </summary>
  public class CreationInfo
  {
    /// <summary>
    /// Identify when the SPDX document was originally created. The date is to be specified according to combined date and time in UTC format as specified in ISO 8601 standard.
    /// </summary>
    public string Created { get; set; } = String.Empty;

    /// <summary>
    /// Single line of text with the following keywords:
    /// "Person: person name" and optional "(email)"
    /// "Organization: organization" and optional "(email)"
    /// "Tool: toolidentifier-version"
    /// </summary>
    public List<string> Creators { get; set; } = new();

    /// <summary>
    /// An optional field for creators of the SPDX document to provide the version of the SPDX License List used when the SPDX document was created.
    /// </summary>
    public string? LicenseListVersion { get; set; }

    /// <summary>
    /// An optional field for creators of the SPDX document to provide general comments about the creation of the SPDX document or any other relevant comment not included in the other fields.
    /// </summary>
    public string? Comment { get; set; }
  }
}