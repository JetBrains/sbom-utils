namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// Defines a relationship between two SPDX elements. The SPDX element may be a Package, File, or SpdxDocument.
  /// </summary>
  public class Relationship
  {
    public string SpdxElementId { get; set; } = String.Empty;

    /// <summary>
    /// A related SpdxElement.
    /// </summary>
    public string RelatedSpdxElement { get; set; } = String.Empty;

    /// <summary>
    /// This field provides information about the relationship between two SPDX elements. For example, you can represent a relationship between two different Files, between a Package and a File, between two Packages, or between one SPDXDocument and another SPDXDocument.
    /// </summary>
    public RelationshipType RelationshipType { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record any general comments about the relationship.
    /// </summary>
    public string Comment { get; set; } = String.Empty;
  }
}