namespace JetBrains.SbomUtils
{
  public class ValidationResult
  {
    public bool Success { get; }

    public string? ErrorMessage { get; }

    public int FilesChecked { get; }

    public string[] IgnoredFiles { get; }

    public MissingFile[] FilesMissingInSbom { get; }

    public FileVerificationResult[] FileVerificationResults { get; }

    public ValidationResult(bool success, string? errorMessage, int filesChecked, string[] ignoredFiles, MissingFile[] filesMissingInSbom, FileVerificationResult[] fileVerificationResults)
    {
      Success = success;
      FilesChecked = filesChecked;
      IgnoredFiles = ignoredFiles;
      FilesMissingInSbom = filesMissingInSbom;
      FileVerificationResults = fileVerificationResults;
    }
  }
}