using System.Collections.ObjectModel;
using JetBrains.SbomUtils.Models;
using File = JetBrains.SbomUtils.Models.File;

namespace JetBrains.SbomUtils
{
  public class SbomModel
  {
    public SpdxDocument SpdxDocument { get; }
    public ReadOnlyDictionary<string, Package> Packages { get; }
    public ReadOnlyDictionary<string, File> Files { get; }
    public ReadOnlyDictionary<string, List<Relationship>> RelationshipsBySourceElement { get; }
    public ReadOnlyDictionary<string, List<Relationship>> RelationshipsByDestinationElement { get; }
    public ReadOnlyDictionary<string, List<FileInfo>> FilesDictionaryByRelativePath { get; }

    /// <summary>
    /// Dictionary that helps to find another locations for a file
    /// </summary>
    public ReadOnlyDictionary<string, List<FileInfo>> FilesDictionaryByFileName { get; }

    public SbomModel(SpdxDocument spdxDocument, ReadOnlyDictionary<string, Package> packages, ReadOnlyDictionary<string, File> files, ReadOnlyDictionary<string, List<Relationship>> relationshipsBySourceElement, ReadOnlyDictionary<string, List<Relationship>> relationshipsByDestinationElement, ReadOnlyDictionary<string, List<FileInfo>> filesDictionaryByRelativePath, ReadOnlyDictionary<string, List<FileInfo>> filesDictionaryByFileName)
    {
      SpdxDocument = spdxDocument;
      Packages = packages;
      Files = files;
      RelationshipsBySourceElement = relationshipsBySourceElement;
      RelationshipsByDestinationElement = relationshipsByDestinationElement;
      FilesDictionaryByRelativePath = filesDictionaryByRelativePath;
      FilesDictionaryByFileName = filesDictionaryByFileName;
    }
  }
}