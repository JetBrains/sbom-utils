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
    var package1 = SbomHelper.MakePackage();
    var package2 = SbomHelper.MakePackage();

    var file1 = SbomHelper.MakeFile("./test.txt");
    var file2 = SbomHelper.MakeFile("./tmp/test.txt");
    var file3 = SbomHelper.MakeFile("./test.txt");

    var documentDescribesPackage1 = "document".Describes(package1);
    var documentDescribesPackage2 = "document".Describes(package2);

    var package1ContainsFile1 = package1.ContainsFile(file1);
    var package1ContainsFile2 = package1.ContainsFile(file2);
    var package2ContainsFile3 = package2.ContainsFile(file3);

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
    Assert.That(filesWithTheSameName, Has.Count.EqualTo(3));
    Assert.That(filesWithTheSameName[0].File.SPDXID, Is.EqualTo(file1.SPDXID));
    Assert.That(filesWithTheSameName[1].File.SPDXID, Is.EqualTo(file2.SPDXID));
    Assert.That(filesWithTheSameName[2].File.SPDXID, Is.EqualTo(file3.SPDXID));

    Assert.That(spdxModel.FilesDictionaryByRelativePath.TryGetValue("test.txt", out var filesTest), Is.True);
    Assert.That(filesTest, Has.Count.EqualTo(2));
    Assert.That(filesTest[0].File.SPDXID, Is.EqualTo(file1.SPDXID));
    Assert.That(filesTest[1].File.SPDXID, Is.EqualTo(file3.SPDXID));

    Assert.That(spdxModel.FilesDictionaryByRelativePath.TryGetValue(SbomHelper.NormalizePath("tmp/test.txt"), out var filesTmpTest), Is.True);
    Assert.That(filesTmpTest, Has.Count.EqualTo(1));
    Assert.That(filesTmpTest[0].File.SPDXID, Is.EqualTo(file2.SPDXID));
  }

  [Test]
  public void RelationshipDirectionsShouldBeRecognized()
  {
    //Arrange
    var package1 = SbomHelper.MakePackage();
    var package2 = SbomHelper.MakePackage();
    var package3 = SbomHelper.MakePackage();
    var package4 = SbomHelper.MakePackage();
    var package5 = SbomHelper.MakePackage();


    // @formatter:off
    var documentDescribesPackage1       = "document".Describes(package1);
    var package1DependsOnPackage2       = package1.RelatedTo(package2).As(RelationshipType.DEPENDS_ON);
    var package3IsDependencyOfPackage2  = package3.RelatedTo(package2).As(RelationshipType.DEPENDENCY_OF);
    var package3HasPrerequisitePackage4 = package3.RelatedTo(package4).As(RelationshipType.HAS_PREREQUISITE);
    var package5IsATestToolOfPackage4   = package5.RelatedTo(package4).As(RelationshipType.TEST_TOOL_OF);
    // @formatter:on

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = new List<Package>() { package1, package2, package3, package4, package5 },
      Files = new List<File>() { },
      Relationships = new List<Relationship>() { documentDescribesPackage1, package1DependsOnPackage2, package3IsDependencyOfPackage2, package3HasPrerequisitePackage4, package5IsATestToolOfPackage4 },
    };

    SbomReader reader = new SbomReader(LoggerFactory.Create(builder => { }).CreateLogger("test"));

    //Act
    var spdxModel = reader.LoadSbom(document);

    //Assert
    Assert.That(spdxModel.Packages, Has.Count.EqualTo(5));

    //Package 1
    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(package1.SPDXID, out var relationshipsToPackage1), Is.True);
    Assert.That(relationshipsToPackage1, Has.Count.EqualTo(1));
    Assert.That(relationshipsToPackage1[0].SpdxElementId, Is.EqualTo(document.SPDXID));

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(package1.SPDXID, out var relationshipsFromPackage1), Is.True);
    Assert.That(relationshipsFromPackage1, Has.Count.EqualTo(1));
    Assert.That(relationshipsFromPackage1[0].RelatedSpdxElement, Is.EqualTo(package2.SPDXID));

    //Package 2
    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(package2.SPDXID, out var relationshipsToPackage2), Is.True);
    Assert.That(relationshipsToPackage2, Has.Count.EqualTo(2));
    Assert.That(relationshipsToPackage2[0].SpdxElementId, Is.EqualTo(package1.SPDXID));
    Assert.That(relationshipsToPackage2[1].SpdxElementId, Is.EqualTo(package3.SPDXID));

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(package2.SPDXID, out _), Is.False);

    //Package 3
    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(package3.SPDXID, out var relationshipsToPackage3), Is.False);

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(package3.SPDXID, out var relationshipsFromPackage3), Is.True);
    Assert.That(relationshipsFromPackage3, Has.Count.EqualTo(2));
    Assert.That(relationshipsFromPackage3[0].RelatedSpdxElement, Is.EqualTo(package2.SPDXID));
    Assert.That(relationshipsFromPackage3[1].RelatedSpdxElement, Is.EqualTo(package4.SPDXID));

    //Package 4
    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(package4.SPDXID, out var relationshipsToPackage4), Is.True);
    Assert.That(relationshipsToPackage4, Has.Count.EqualTo(2));
    Assert.That(relationshipsToPackage4[0].SpdxElementId, Is.EqualTo(package3.SPDXID));
    Assert.That(relationshipsToPackage4[1].SpdxElementId, Is.EqualTo(package5.SPDXID));

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(package4.SPDXID, out var relationshipsFromPackage4), Is.False);

    //Package 5
    Assert.That(spdxModel.RelationshipsByDestinationElement.TryGetValue(package5.SPDXID, out var relationshipsToPackage5), Is.False);

    Assert.That(spdxModel.RelationshipsBySourceElement.TryGetValue(package5.SPDXID, out var relationshipsFromPackage5), Is.True);
    Assert.That(relationshipsFromPackage5, Has.Count.EqualTo(1));
    Assert.That(relationshipsFromPackage5[0].RelatedSpdxElement, Is.EqualTo(package4.SPDXID));
  }
}