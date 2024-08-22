using System.Collections.ObjectModel;
using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using File = JetBrains.SbomUtils.Models.File;

namespace JetBrains.SbomUtils
{
  public class SbomReader
  {
    private readonly ILogger _logger;

    public SbomReader(ILogger logger)
    {
      _logger = logger;
    }

    public SpdxDocument ReadSbom(string path)
    {
      if (!System.IO.File.Exists(path))
        throw new SbomValidationException("Provided path to the SBOM doesn't exist");

      using (var stream = System.IO.File.OpenRead(path))
      {
        return ReadSbom(stream);
      }
    }

    public SpdxDocument ReadSbom(Stream stream)
    {
      SpdxDocument? spdxDocument;
      try
      {
        using (StreamReader file = new StreamReader(stream))
        {
          JsonSerializer serializer = new JsonSerializer();
          spdxDocument = serializer.Deserialize<SpdxDocument>(new JsonTextReader(file));
        }

        if (spdxDocument == null)
          throw new SbomValidationException("Failed to parse the SBOM");
      }
      catch (Exception e)
      {
        throw new SbomValidationException($"Failed to parse the SBOM: {e.Message}", e);
      }

      spdxDocument.Packages ??= new List<Package>();
      spdxDocument.Files ??= new List<File>();
      spdxDocument.Relationships ??= new List<Relationship>();

      _logger.LogDebug("Loaded {packages} packages, {files} files and {relationships} relationships",
        spdxDocument.Packages.Count,
        spdxDocument.Files.Count,
        spdxDocument.Relationships.Count);

      return spdxDocument;
    }

    public SbomModel LoadSbom(SpdxDocument spdxDocument)
    {
      var packagesDict = spdxDocument.Packages!.ToDictionary(p => p.SPDXID);
      var filesDict = spdxDocument.Files!.ToDictionary(f => f.SPDXID);
      var relationshipsBySourceElement = new Dictionary<string, List<Relationship>>();
      var relationshipsByDestinationElement = new Dictionary<string, List<Relationship>>();

      foreach (var relationship in spdxDocument.Relationships!)
      {
        if (relationshipsBySourceElement.TryGetValue(relationship.SpdxElementId, out var relationshipsBySourceList))
          relationshipsBySourceList.Add(relationship);
        else
          relationshipsBySourceElement.Add(relationship.SpdxElementId, new List<Relationship>() { relationship });

        if (relationshipsByDestinationElement.TryGetValue(relationship.RelatedSpdxElement,
              out var relationshipsByDestinationList))
          relationshipsByDestinationList.Add(relationship);
        else
          relationshipsByDestinationElement.Add(relationship.RelatedSpdxElement,
            new List<Relationship>() { relationship });
      }

      var filesDictionaryByRelativePath = new Dictionary<string, List<FileInfo>>();
      var filesDictionaryByFileName = new Dictionary<string, List<FileInfo>>();

      var files = GetAllFilesWithPackages(filesDict, packagesDict, relationshipsBySourceElement,
        relationshipsByDestinationElement);
      foreach (var file in files)
      {
        string temp = Path.GetTempPath();
        string fullPath = Path.GetRelativePath(temp, Path.GetFullPath(file.File.FileName, temp));
        string filename = Path.GetFileName(file.File.FileName);

        if (filesDictionaryByRelativePath.TryGetValue(fullPath, out var existingFile))
          existingFile.Add(file);
        else
          filesDictionaryByRelativePath.Add(fullPath, new List<FileInfo>(2) { file });

        if (filesDictionaryByFileName.TryGetValue(filename, out var existingFiles))
          existingFiles.Add(file);
        else
          filesDictionaryByFileName.Add(filename, new List<FileInfo>() { file });
      }

      foreach (var file in filesDictionaryByRelativePath)
      {
        if (file.Value.Count > 1)
          _logger.LogDebug("File {file} has duplicates in {count} following packages:\n   {packages}", file.Key,
            file.Value.Count, string.Join("\n   ", file.Value.Select(v => v.Package.Name)));
      }

      var sbomModel = new SbomModel(
        spdxDocument,
        new ReadOnlyDictionary<string, Package>(packagesDict),
        new ReadOnlyDictionary<string, File>(filesDict),
        new ReadOnlyDictionary<string, List<Relationship>>(relationshipsBySourceElement),
        new ReadOnlyDictionary<string, List<Relationship>>(relationshipsByDestinationElement),
        new ReadOnlyDictionary<string, List<FileInfo>>(filesDictionaryByRelativePath),
        new ReadOnlyDictionary<string, List<FileInfo>>(filesDictionaryByFileName));

      return sbomModel;
    }

    public IReadOnlyCollection<FileInfo> GetAllFilesWithPackages(Dictionary<string, File> filesDict, Dictionary<string, Package> packagesDict, Dictionary<string, List<Relationship>> relationshipsBySourceElement, Dictionary<string, List<Relationship>> relationshipsByDestinationElement)
    {
      List<FileInfo> files = new List<FileInfo>(filesDict.Count);


      foreach (var file in filesDict.Values)
      {
        if (relationshipsByDestinationElement.TryGetValue(file.SPDXID, out var leavesToRootRelationships))
        {
          foreach (var relationship in leavesToRootRelationships)
          {
            if (SbomRelationshipTypes.RootToLeavesRelationshipTypes.Contains(relationship.RelationshipType))
            {
              if (packagesDict.TryGetValue(relationship.SpdxElementId, out var package))
              {
                files.Add(new FileInfo(file, package, NormalizeRelativePath(file.FileName)));
              }
            }
            else if (!SbomRelationshipTypes.LeavesToRootRelationshipTypes.Contains(relationship.RelationshipType))
            {
              _logger.LogWarning("Relationship type {relationshipType} (between {element} and {relatedElement}) is not supported",
                relationship.RelationshipType,
                relationship.SpdxElementId,
                relationship.RelatedSpdxElement);
            }
          }
        }

        if (relationshipsBySourceElement.TryGetValue(file.SPDXID, out var rootToLeavesRelationships))
        {
          foreach (var relationship in rootToLeavesRelationships)
          {
            if (SbomRelationshipTypes.LeavesToRootRelationshipTypes.Contains(relationship.RelationshipType))
            {
              if (packagesDict.TryGetValue(relationship.RelatedSpdxElement, out var package))
              {
                files.Add(new FileInfo(file, package, NormalizeRelativePath(file.FileName)));
              }
            }
            else if (!SbomRelationshipTypes.RootToLeavesRelationshipTypes.Contains(relationship.RelationshipType))
            {
              _logger.LogWarning("Relationship type {relationshipType} (between {element} and {relatedElement}) is not supported",
                relationship.RelationshipType,
                relationship.SpdxElementId,
                relationship.RelatedSpdxElement);
            }
          }
        }
      }

      return files;
    }

    private string NormalizeRelativePath(string path)
    {
      string tempDir = Path.GetTempPath();
      return Path.GetRelativePath(tempDir, Path.GetFullPath(path, tempDir));
    }
  }
}