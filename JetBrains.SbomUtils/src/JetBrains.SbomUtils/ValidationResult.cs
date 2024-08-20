namespace JetBrains.SbomUtils
{
  public class ValidationResult
  {
    public bool Success { get; }

    public string? ErrorMessage { get; }

    public int FilesChecked { get; }

    public ICollection<string> IgnoredFiles { get; }

    public ICollection<MissingFile> FilesMissingInSbom { get; }

    public ICollection<FileVerificationResult> FileVerificationResults { get; }

    public ValidationResult(bool success, string? errorMessage, int filesChecked, ICollection<string> ignoredFiles, ICollection<MissingFile> filesMissingInSbom, ICollection<FileVerificationResult> fileVerificationResults)
    {
      Success = success;
      FilesChecked = filesChecked;
      IgnoredFiles = ignoredFiles;
      FilesMissingInSbom = filesMissingInSbom;
      FileVerificationResults = fileVerificationResults;
    }
  }
}