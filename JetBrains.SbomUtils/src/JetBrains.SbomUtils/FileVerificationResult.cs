namespace JetBrains.SbomUtils;

public class FileVerificationResult
{
  public string FilePath { get; }
  public bool Success { get; }
  public HashVerificationFailure? HashVerificationFailure { get; }
  public FileFromUnreferencedPackage? FileFromUnreferencedPackage { get; }

  public FileVerificationResult(string filePath, bool success, HashVerificationFailure? hashVerificationFailure, FileFromUnreferencedPackage? fileFromUnreferencedPackage)
  {
    FilePath = filePath;
    Success = success;
    HashVerificationFailure = hashVerificationFailure;
    FileFromUnreferencedPackage = fileFromUnreferencedPackage;
  }
}