namespace JetBrains.SbomUtils.Models
{
  public class ExtractedLicensingInfo
  {
    /// <summary>
    /// Provide a locally unique identifier to refer to licenses that are not found on the SPDX License List.
    /// </summary>
    public string LicenseId { get; set; } = String.Empty;

    /// <summary>
    /// Provide a copy of the actual text of the license reference extracted from the package, file or snippet that is associated with the License Identifier to aid in future analysis.
    /// </summary>
    public string ExtractedText { get; set; } = String.Empty;

    /// <summary>
    /// Provide a common name of the license that is not on the SPDX list. If the License Name field is not present for a license, it implies an equivalent meaning to NOASSERTION.
    /// </summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// Provide a pointer to the official source of a license that is not included in the SPDX License List, that is referenced by the License Identifier.
    /// </summary>
    public List<CrossRef>? CrossRefs { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record any general comments about the license.
    /// </summary>
    public string Comment { get; set; } = String.Empty;
  }
}