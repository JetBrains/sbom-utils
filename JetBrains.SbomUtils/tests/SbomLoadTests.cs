using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;
using File = JetBrains.SbomUtils.Models.File;

namespace JetBrains.SbomUtils.Tests;

public class SbomLoadTests
{
  [Test]
  public void SimpleSbomShouldBeLoaded()
  {
    var package = new Package()
    {
      SPDXID = "package-1",
      Name = "Test package",
    };

    var file = new File()
    {
      SPDXID = "file-1",
      FileName = "test.txt"
    };

    var documentDescribesPackage = new Relationship()
    {
      SpdxElementId = "document",
      RelatedSpdxElement = package.SPDXID,
      RelationshipType = RelationshipType.DESCRIBES,
    };

    var packageContainsFile = new Relationship()
    {
      SpdxElementId = package.SPDXID,
      RelatedSpdxElement = file.SPDXID,
      RelationshipType = RelationshipType.CONTAINS,
    };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = new List<Package>()
      {
        package
      },
      Files = new List<File>()
      {
        file
      },
      Relationships = new List<Relationship>() { documentDescribesPackage, packageContainsFile },
    };

    SbomReader reader = new SbomReader(LoggerFactory.Create(builder => { }).CreateLogger("test"));
    var spdxModel = reader.LoadSbom(document);

    Assert.That(spdxModel.Packages, Has.Count.EqualTo(1));
    Assert.That(spdxModel.Files, Has.Count.EqualTo(1));
  }
}