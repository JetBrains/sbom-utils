using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;
using File = JetBrains.SbomUtils.Models.File;

namespace JetBrains.SbomUtils.Tests;

public class SbomLoadTests
{
  [Test]
  public void SimpleSbomShouldBeLoaded()
  {
    //Arrange
    var package1 = new Package() {  SPDXID = "package-1", Name = "Test package", };
    var package2 = new Package() { SPDXID = "package-2", Name = "Test package 2", };

    var file1 = new File() { SPDXID = "file-1", FileName = "./test.txt" };
    var file2 = new File() { SPDXID = "file-2", FileName = "./tmp/test.txt" };
    var file3 = new File() { SPDXID = "file-3", FileName = "./test.txt" };

    var documentDescribesPackage1 = new Relationship() { SpdxElementId = "document", RelatedSpdxElement = package1.SPDXID, RelationshipType = RelationshipType.DESCRIBES, };
    var documentDescribesPackage2 = new Relationship() { SpdxElementId = "document", RelatedSpdxElement = package2.SPDXID, RelationshipType = RelationshipType.DESCRIBES, };

    var package1ContainsFile1 = new Relationship() { SpdxElementId = package1.SPDXID, RelatedSpdxElement = file1.SPDXID, RelationshipType = RelationshipType.CONTAINS, };
    var package1ContainsFile2 = new Relationship() { SpdxElementId = package1.SPDXID, RelatedSpdxElement = file2.SPDXID, RelationshipType = RelationshipType.CONTAINS, };
    var package2ContainsFile3 = new Relationship() { SpdxElementId = package2.SPDXID, RelatedSpdxElement = file3.SPDXID, RelationshipType = RelationshipType.CONTAINS, };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = new List<Package>() { package1, package2 },
      Files = new List<File>() { file1, file2, file3 },
      Relationships = new List<Relationship>() { documentDescribesPackage1, documentDescribesPackage2, package1ContainsFile1, package1ContainsFile2, package2ContainsFile3 },
    };

    SbomReader reader = new SbomReader(LoggerFactory.Create(builder => { }).CreateLogger("test"));

    //Act
    var spdxModel = reader.LoadSbom(document);

    //Assert
    Assert.That(spdxModel.Packages, Has.Count.EqualTo(2));
    Assert.That(spdxModel.Files, Has.Count.EqualTo(3));

    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(file1.SPDXID, out var relationshipsToFile1), Is.True);
    Assert.That(relationshipsToFile1, Has.Count.EqualTo(1));
    Assert.That(relationshipsToFile1[0].SpdxElementId, Is.EqualTo(package1.SPDXID));

    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(file2.SPDXID, out var relationshipsToFile2), Is.True);
    Assert.That(relationshipsToFile2, Has.Count.EqualTo(1));
    Assert.That(relationshipsToFile2[0].SpdxElementId, Is.EqualTo(package1.SPDXID));

    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(package1.SPDXID, out var relationshipsToPackage1), Is.True);
    Assert.That(relationshipsToPackage1, Has.Count.EqualTo(1));
    Assert.That(relationshipsToPackage1[0].SpdxElementId, Is.EqualTo(document.SPDXID));

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(document.SPDXID, out var relationshipsFromDocument), Is.True);
    Assert.That(relationshipsFromDocument, Has.Count.EqualTo(2));
    Assert.That(relationshipsFromDocument[0].RelatedSpdxElement, Is.EqualTo(package1.SPDXID));
    Assert.That(relationshipsFromDocument[1].RelatedSpdxElement, Is.EqualTo(package2.SPDXID));

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(package1.SPDXID, out var relationshipsFromPackage), Is.True);
    Assert.That(relationshipsFromPackage, Has.Count.EqualTo(2));
    Assert.That(relationshipsFromPackage[0].RelatedSpdxElement, Is.EqualTo(file1.SPDXID));
    Assert.That(relationshipsFromPackage[1].RelatedSpdxElement, Is.EqualTo(file2.SPDXID));

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(file1.SPDXID, out _), Is.False);
    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(file2.SPDXID, out _), Is.False);

    Assert.That(spdxModel.FilesDictionaryByFileName.TryGetValue("test.txt", out var filesWithTheSameName), Is.True);
    Assert.That(filesWithTheSameName, Has.Count.EqualTo(2));
    Assert.That(filesWithTheSameName[0].File.SPDXID, Is.EqualTo(file1.SPDXID));
    Assert.That(filesWithTheSameName[1].File.SPDXID, Is.EqualTo(file3.SPDXID));
  }
}