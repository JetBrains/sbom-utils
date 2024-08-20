namespace JetBrains.SbomUtils.Models
{
  public class File
  {
    /// <summary>
    /// Uniquely identify any element in an SPDX document which might be referenced by other elements.
    /// These might be referenced internally and externally with the addition of the SPDX document identifier.
    /// </summary>
    public string SPDXID { get; set; } = String.Empty;

    /// <summary>
    /// Identify the full path and filename that corresponds to the file information in this section.
    /// </summary>
    public string FileName { get; set; } = String.Empty;

    /// <summary>
    /// This field provides information about the type of file identified. File Type is intrinsic to the file, independent of how the file is being used.
    /// </summary>
    public List<FileType>? FileTypes { get; set; }

    /// <summary>
    /// This field provides the result from some specific checksum algorithm.
    /// Checksum algorithms consume data from the file and produce a short, fix-sized summary that is sensitive to changes in the file's data.
    /// Any random change to the file's data will (with high likelihood) result in a different checksum value.
    /// </summary>
    public List<Checksum> Checksums { get; set; } = new();

    /// <summary>
    /// This field contains the license the SPDX document creator has concluded as governing the file or alternative values if the governing license cannot be determined.
    /// </summary>
    public string? LicenseConcluded { get; set; }

    /// <summary>
    /// This field contains the license information actually found in the file, if any.
    /// This information is most commonly found in the header of the file, although it might be in other areas of the actual file.
    /// Any license information not actually in the file, e.g., “COPYING.txt” file in a top-level directory, should not be reflected in this field.
    /// </summary>
    public List<string>? LicenseInfoInFiles { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record any relevant background references or analysis that went in to arriving at the Concluded License for a file.
    /// If the Concluded License does not match the License Information in File, this should be explained by the SPDX document creator.
    /// It is also preferable to include an explanation here when the Concluded License is NOASSERTION.
    /// </summary>
    public string? LicenseComments { get; set; }

    /// <summary>
    /// Identify the copyright holder of the file, as well as any dates present. This shall be a free-form text field extracted from the actual file.
    /// </summary>
    public string? CopyrightText { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record any general comments about the file.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record license notices or other such related notices found in the file. This might or might not include copyright statements.
    /// </summary>
    public string? NoticeText { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record file contributors.
    /// Contributors could include names of copyright holders and/or authors who might not be copyright holders, yet contributed to the file content.
    /// </summary>
    public List<string>? FileContributors { get; set; }

    /// <summary>
    /// This field provides a place for the SPDX document creator to record, at the file level, acknowledgements that might be required to be communicated in some contexts.
    /// This is not meant to include the file's actual complete license text (see LicenseConcluded and LicenseInfoInFile), and might or might not include copyright notices (see also FileCopyrightText).
    /// The SPDX document creator might use this field to record other acknowledgements, such as particular clauses from license texts, which might be necessary or desirable to reproduce.
    /// </summary>
    public List<string>? AttributionTexts { get; set; }
  }
}