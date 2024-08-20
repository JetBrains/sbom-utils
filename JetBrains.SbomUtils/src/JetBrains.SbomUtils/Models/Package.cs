using Newtonsoft.Json;

namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// If the SPDX information describes a package
  /// </summary>
  public class Package
  {
    /// <summary>
    /// Uniquely identify any element in an SPDX document which may be referenced by other elements.
    /// </summary>
    [JsonProperty("SPDXID")]
    public string SPDXID { get; set; } = String.Empty;

    /// <summary>
    /// Identify the full name of the package as given by the Package Originator (7.6).
    /// </summary>
    public string Name { get; set; } = String.Empty;

    /// <summary>
    /// Identify the version of the package.
    /// </summary>
    public string? VersionInfo { get; set; }

    /// <summary>
    /// Provide the actual file name of the package, or path of the directory being treated as a package. This may include the packaging and compression methods used as part of the file name, if appropriate.
    /// </summary>
    public string? PackageFileName { get; set; }

    /// <summary>
    /// Identify the actual distribution source for the package/directory identified in the SPDX document.
    /// This might or might not be different from the originating distribution source for the package.
    /// The name of the Package Supplier shall be an organization or recognized author and not a web site.
    /// </summary>
    public string? Supplier { get; set; }

    /// <summary>
    /// If the package identified in the SPDX document originated from a different person or organization than identified as Package Supplier (see 7.5 above), this field identifies from where or whom the package originally came.
    /// In some cases, a package may be created and originally distributed by a different third party than the Package Supplier of the package.
    /// </summary>
    public string? Originator { get; set; }

    /// <summary>
    /// This section identifies the download Uniform Resource Locator (URL), or a specific location within a version control system (VCS) for the package at the time that the SPDX document was created.
    /// Where and how to download the exact package being referenced is critical verification and tracking data.
    /// </summary>
    public string DownloadLocation { get; set; } = String.Empty;
    /// <summary>
    /// Indicates whether the file content of this package has been available for or subjected to analysis when creating the SPDX document.
    /// If false, indicates packages that represent metadata or URI references to a project, product, artifact, distribution or a component. If false, the package shall not contain any files.
    /// If omitted, the default value of true is assumed.
    /// </summary>
    public bool? FilesAnalyzed { get; set; }

    /// <summary>
    /// This field provides an independently reproducible mechanism identifying specific contents of a package based on the actual files (except the SPDX document itself, if it is included in the package)
    /// that make up each package and that correlates to the data in this SPDX document.
    /// This identifier enables a recipient to determine if any file in the original package (that the analysis was done on) has been changed and permits inclusion of an SPDX document as part of a package.
    /// </summary>
    public PackageVerificationCode? PackageVerificationCode { get; set; }

    /// <summary>
    /// Provide an independently reproducible mechanism that permits unique identification of a specific package that correlates to the data in this SPDX document.
    /// This identifier enables a recipient to determine if any file in the original package has been changed.
    /// If the SPDX document is to be included in a package, this value should not be calculated. The SHA1 algorithm shall be used to provide the checksum by default.
    /// </summary>
    public List<Checksum>? Checksums { get; set; }

    /// <summary>
    /// Provide a place for the SPDX document creator to record a web site that serves as the package's home page.
    /// This link can also be used to reference further information about the package referenced by the SPDX document creator.
    /// </summary>
    public string? Homepage { get; set; }

    /// <summary>
    /// Provide a place for the SPDX document creator to record any relevant background information or additional comments about the origin of the package.
    /// For example, this field might include comments indicating whether the package was pulled from a source code management system or has been repackaged.
    /// </summary>
    public string? SourceInfo { get; set; }

    /// <summary>
    /// Contain the license the SPDX document creator has concluded as governing the package or alternative values, if the governing license cannot be determined.
    /// </summary>
    public string? LicenseConcluded { get; set; }

    /// <summary>
    /// This field is to contain a list of all licenses found in the package. The relationship between licenses (i.e., conjunctive, disjunctive) is not specified in this field – it is simply a listing of all licenses found.
    /// </summary>
    public List<string>? LicenseInfoFromFiles { get; set; }

    /// <summary>
    /// List the licenses that have been declared by the authors of the package.
    /// Any license information that does not originate from the package authors, e.g. license information from a third-party repository, should not be included in this field.
    /// </summary>
    public string? LicenseDeclared { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record any relevant background information or analysis that went in to arriving at the Concluded License for a package.
    /// If the Concluded License does not match the Declared License or License Information from Files, this should be explained by the SPDX document creator.
    /// It is also preferable to include an explanation here when the Concluded License is NOASSERTION.
    /// </summary>
    public string? LicenseComments { get; set; }

    /// <summary>
    /// Identify the copyright holders of the package, as well as any dates present. This will be a free form text field extracted from package information files.
    /// </summary>
    public string? CopyrightText { get; set; }

    /// <summary>
    /// This field is a short description of the package.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// This field is a more detailed description of the package. It may also be extracted from the packages itself.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record any general comments about the package being described.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// An External Reference allows a Package to reference an external source of additional information, metadata, enumerations, asset identifiers, or downloadable content believed to be relevant to the Package.
    /// </summary>
    public List<ExternalRef>? ExternalRefs { get; set; }
  }
}