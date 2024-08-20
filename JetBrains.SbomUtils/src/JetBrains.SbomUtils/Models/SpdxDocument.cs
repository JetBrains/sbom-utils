using Newtonsoft.Json;

namespace JetBrains.SbomUtils.Models
{
  public class SpdxDocument
  {
    /// <summary>
    /// Identify the current SPDX document which may be referenced in relationships by other files, packages internally and documents externally.
    /// </summary>
    [JsonProperty("SPDXID")]
    public string SPDXID { get; set; } = String.Empty;

    /// <summary>
    /// Provide a reference number that can be used to understand how to parse and interpret the rest of the file.
    /// </summary>
    public string SpdxVersion { get; } = String.Empty;

    /// <summary>
    /// Identify name of this document as designated by creator.
    /// </summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// Compliance with the SPDX specification includes populating the SPDX fields therein with data related to such fields ("SPDX-Metadata").
    /// The SPDX specification contains numerous fields where an SPDX document creator may provide relevant explanatory text in SPDX-Metadata.
    /// Without opining on the lawfulness of "database rights" (in jurisdictions where applicable), such explanatory text is copyrightable subject matter in most Berne Convention countries.
    /// By using the SPDX specification, or any portion hereof, you hereby agree that any copyright rights (as determined by your jurisdiction) in any SPDX-Metadata,
    /// including without limitation explanatory text, shall be subject to the terms of the Creative Commons CC0 1.0 Universal license.
    /// </summary>
    public string DataLicense { get; set; } = String.Empty;

    /// <summary>
    /// Provide an SPDX document-specific namespace as a unique absolute Uniform Resource Identifier (URI) as specified in RFC-3986, with the exception of the ‘#’ delimiter.
    /// </summary>
    public string DocumentNamespace { get; set; } = String.Empty;

    /// <summary>
    /// Identify any external SPDX documents referenced within this SPDX document.
    /// </summary>
    public List<ExternalDocumentRef>? ExternalDocumentRefs { get; set; }

    /// <summary>
    /// Identify who (or what, in the case of a tool) created the SPDX document.
    /// </summary>
    public CreationInfo CreationInfo { get; set; } = new();

    /// <summary>
    /// An optional field for creators of the SPDX document content to provide comments to the consumers of the SPDX document.
    /// </summary>
    public string? Comment { get; set; }

    public List<ExtractedLicensingInfo>? HasExtractedLicensingInfos { get; set; }

    public List<Package>? Packages { get; set; }

    public List<File>? Files { get; set; }

    public List<Relationship>? Relationships { get; set; }
  }
}