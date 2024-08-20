namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// A manifest based verification code (the algorithm is defined in section 4.7 of the full specification) of the SPDX Item.
  /// This allows consumers of this data and/or database to determine if an SPDX item they have in hand is identical to the SPDX item from which the data was produced.
  /// This algorithm works even if the SPDX document is included in the SPDX item.
  /// </summary>
  public class PackageVerificationCode
  {
    /// <summary>
    /// A file that was excluded when calculating the package verification code. This is usually a file containing SPDX data regarding the package.
    /// If a package contains more than one SPDX file all SPDX files must be excluded from the package verification code.
    /// If this is not done it would be impossible to correctly calculate the verification codes in both files.
    /// </summary>
    public List<string>? PackageVerificationCodeExcludedFiles { get; set; }

    /// <summary>
    /// The actual package verification code as a hex encoded value.
    /// </summary>
    public string PackageVerificationCodeValue { get; set; } = String.Empty;
  }
}