using JetBrains.SbomUtils.Models;
using File = JetBrains.SbomUtils.Models.File;

namespace JetBrains.SbomUtils
{
  public class FileInfo
  {

    public File File { get; }
    public Package Package { get; }
    public string NormalizedRelativePath { get; }

    public FileInfo(File file, Package package, string normalizedRelativePath)
    {
      File = file;
      Package = package;
      NormalizedRelativePath = normalizedRelativePath;
    }
  }
}
