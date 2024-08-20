using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;

namespace JetBrains.SbomUtils
{
  public class SbomOperations
  {
    private readonly ILogger _logger;

    public SbomOperations(ILogger logger)
    {
      _logger = logger;
    }

    public ICollection<Package> FindRootPackageByName(SbomModel sbomModel, IEnumerable<string> rootPackageNames)
    {
      var describesRelationships = sbomModel.SpdxDocument.Relationships!.Where(r => r.RelationshipType == RelationshipType.DESCRIBES);
      var rootPackagesSet = rootPackageNames.ToHashSet();
      List<Package> result = new List<Package>();

      foreach (var relationship in describesRelationships)
      {
        if (sbomModel.Packages.TryGetValue(relationship.RelatedSpdxElement, out var rootPackage))
        {
          if (rootPackagesSet.Contains(rootPackage.Name))
            result.Add(rootPackage);
        }
      }

      return result;
    }

    public ICollection<Package> GetAllDependantPackages(SbomModel sbomModel, IEnumerable<Package> rootPackages)
    {
      Dictionary<string, Package> seenPackages = new Dictionary<string, Package>();
      Queue<Package> queue = new Queue<Package>();

      foreach (var rootPackage in rootPackages)
      {
        seenPackages.Add(rootPackage.SPDXID, rootPackage);
        queue.Enqueue(rootPackage);
      }

      while (queue.Count > 0)
      {
        var package = queue.Dequeue();

        if (sbomModel.RelationshipsBySourceElement.TryGetValue(package.SPDXID, out var rootToLeavesRelationships))
        {
          foreach (var relationship in rootToLeavesRelationships)
          {
            if (SbomRelationshipTypes.RootToLeavesRelationshipTypes.Contains(relationship.RelationshipType))
            {
              if (sbomModel.Packages.TryGetValue(relationship.RelatedSpdxElement, out var relatedPackage) && !seenPackages.ContainsKey(relationship.RelatedSpdxElement))
              {
                seenPackages.Add(relatedPackage.SPDXID, relatedPackage);
                queue.Enqueue(relatedPackage);
              }
            }
            else if (!SbomRelationshipTypes.LeavesToRootRelationshipTypes.Contains(relationship.RelationshipType))
            {
              _logger.LogWarning("Relationship type {relationshipType} (between package {element} and package {relatedElement}) is not supported",
                relationship.RelationshipType,
                relationship.SpdxElementId,
                relationship.RelatedSpdxElement);
            }
          }
        }

        if (sbomModel.RelationshipsByDestinationElement.TryGetValue(package.SPDXID, out var leavesToRootRelationships))
        {
          foreach (var relationship in leavesToRootRelationships)
          {
            if (SbomRelationshipTypes.LeavesToRootRelationshipTypes.Contains(relationship.RelationshipType))
            {
              if (sbomModel.Packages.TryGetValue(relationship.SpdxElementId, out var relatedPackage) && !seenPackages.ContainsKey(relationship.SpdxElementId))
              {
                seenPackages.Add(relatedPackage.SPDXID, relatedPackage);
                queue.Enqueue(relatedPackage);
              }
            }
            else if (!SbomRelationshipTypes.RootToLeavesRelationshipTypes.Contains(relationship.RelationshipType))
            {
              _logger.LogWarning("Relationship type {relationshipType} (between package {element} and package {relatedElement}) is not supported",
                relationship.RelationshipType,
                relationship.SpdxElementId,
                relationship.RelatedSpdxElement);
            }
          }
        }
      }

      return seenPackages.Values;
    }
  }
}