using JetBrains.SbomUtils.Models;
using File = JetBrains.SbomUtils.Models.File;

namespace JetBrains.SbomUtils.Tests;

public static class SbomHelper
{
  private static int _packageId = 1;
  private static int _fileId = 1;

  public static Package MakePackage()
  {
    int packageId = Interlocked.Increment(ref _packageId);
    return new Package()
    {
      SPDXID = $"package-{packageId}",
      Name = $"Test package {packageId}",
    };
  }

  public static File MakeFile(string filename)
  {
    int fileId = Interlocked.Increment(ref _fileId);
    return new File()
    {
      SPDXID = $"file-{fileId}",
      FileName = filename,
      Checksums = new List<Checksum>(),
    };
  }

  public static File AddChecksum(this File file, ChecksumAlgorithm algorithm, string value)
  {
    file.Checksums.Add(new Checksum()
    {
      Algorithm = algorithm,
      ChecksumValue = value,
    });

    return file;
  }

  public record RelationshipTemplate(string FromId, string ToId);

  public static RelationshipTemplate RelatedTo(this Package fromPackage, Package toPackage) =>
    new RelationshipTemplate(fromPackage.SPDXID, toPackage.SPDXID);

  public static Relationship As(this RelationshipTemplate template, RelationshipType relationshipType) =>
    new Relationship()
    {
      SpdxElementId = template.FromId,
      RelatedSpdxElement = template.ToId,
      RelationshipType = relationshipType,
    };

  public static Relationship Describes(this string documentId, Package package) =>
    new Relationship()
  {
    SpdxElementId = documentId,
    RelatedSpdxElement = package.SPDXID,
    RelationshipType = RelationshipType.DESCRIBES,
  };

  public static Relationship ContainsFile(this Package fromPackage, File file) =>
    new Relationship()
    {
      SpdxElementId = fromPackage.SPDXID,
      RelatedSpdxElement = file.SPDXID,
      RelationshipType = RelationshipType.CONTAINS,
    };
}