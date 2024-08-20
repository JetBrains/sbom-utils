namespace JetBrains.SbomUtils
{
  public class MissingFile
  {
    public string FilePath { get; }
    public IEnumerable<FileInfo> PossibleCandidates { get; }

    public MissingFile(string filePath, IEnumerable<FileInfo> possibleCandidates)
    {
      FilePath = filePath;
      PossibleCandidates = possibleCandidates;
    }
  }
}