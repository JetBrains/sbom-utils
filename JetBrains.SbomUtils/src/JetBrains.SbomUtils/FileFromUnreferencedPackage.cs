namespace JetBrains.SbomUtils;

public class FileFromUnreferencedPackage
{
  public FileInfo FileInfo { get; }

  public FileFromUnreferencedPackage(FileInfo fileInfo)
  {
    FileInfo = fileInfo;
  }
}