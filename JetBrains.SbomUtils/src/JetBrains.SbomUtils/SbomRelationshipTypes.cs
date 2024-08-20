using JetBrains.SbomUtils.Models;

namespace JetBrains.SbomUtils
{
  public static class SbomRelationshipTypes
  {
    public static readonly ISet<RelationshipType> RootToLeavesRelationshipTypes = new HashSet<RelationshipType>()
    {
      RelationshipType.CONTAINS,
      RelationshipType.DESCRIBES,
      RelationshipType.DEPENDS_ON,
      RelationshipType.DYNAMIC_LINK,
      RelationshipType.DISTRIBUTION_ARTIFACT,
      RelationshipType.GENERATES,
      RelationshipType.HAS_PREREQUISITE,
      RelationshipType.PACKAGE_OF,
      RelationshipType.STATIC_LINK,
    };

    public static readonly ISet<RelationshipType> LeavesToRootRelationshipTypes = new HashSet<RelationshipType>()
    {
      RelationshipType.AMENDS,
      RelationshipType.ANCESTOR_OF,
      RelationshipType.BUILD_DEPENDENCY_OF,
      RelationshipType.BUILD_TOOL_OF,
      RelationshipType.CONTAINED_BY,
      RelationshipType.COPY_OF,
      RelationshipType.DATA_FILE_OF,
      RelationshipType.DEPENDENCY_OF,
      RelationshipType.DEPENDENCY_MANIFEST_OF,
      RelationshipType.DESCENDANT_OF,
      RelationshipType.DESCRIBED_BY,
      RelationshipType.DEV_DEPENDENCY_OF,
      RelationshipType.DEV_TOOL_OF,
      RelationshipType.DOCUMENTATION_OF,
      RelationshipType.EXAMPLE_OF,
      RelationshipType.EXPANDED_FROM_ARCHIVE,
      RelationshipType.FILE_ADDED,
      RelationshipType.FILE_DELETED,
      RelationshipType.FILE_MODIFIED,
      RelationshipType.GENERATED_FROM,
      RelationshipType.METAFILE_OF,
      RelationshipType.OPTIONAL_COMPONENT_OF,
      RelationshipType.OPTIONAL_DEPENDENCY_OF,
      RelationshipType.PATCH_FOR,
      RelationshipType.PATCH_APPLIED,
      RelationshipType.PREREQUISITE_FOR,
      RelationshipType.PROVIDED_DEPENDENCY_OF,
      RelationshipType.RUNTIME_DEPENDENCY_OF,
      RelationshipType.TEST_CASE_OF,
      RelationshipType.TEST_DEPENDENCY_OF,
      RelationshipType.TEST_OF,
      RelationshipType.TEST_TOOL_OF,
      RelationshipType.VARIANT_OF,
    };
  }
}