using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;
using File = JetBrains.SbomUtils.Models.File;

namespace JetBrains.SbomUtils.Tests;

public class SbomOperationsTests
{
  [Test]
  public void DependentPackagedWithSingleRootShouldBeCollected()
  {
    var package1 = SbomHelper.MakePackage();
    var package2 = SbomHelper.MakePackage();
    var package3 = SbomHelper.MakePackage();
    var package4 = SbomHelper.MakePackage();
    var package5 = SbomHelper.MakePackage();

    var relationships = new List<Relationship>
    {
      package1.RelatedTo(package2).As(RelationshipType.DEPENDS_ON),
      package2.RelatedTo(package3).As(RelationshipType.DEPENDS_ON),
      package1.RelatedTo(package4).As(RelationshipType.DEPENDS_ON),
      package5.RelatedTo(package1).As(RelationshipType.DEPENDENCY_OF),
    };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = new List<Package>() { package1, package2, package3, package4, package5 },
      Files = new List<File>() {},
      Relationships = relationships
    };

    var logger = LoggerFactory.Create(builder => { }).CreateLogger("test");
    SbomReader reader = new SbomReader(logger);
    var spdxModel = reader.LoadSbom(document);
    var sbomOperations = new SbomOperations(logger);

    //Act
    var dependantPackages = sbomOperations.GetAllDependantPackages(spdxModel, new[] { package1 });

    Assert.That(dependantPackages, Has.Count.EqualTo(5));
    Assert.That(dependantPackages, Is.EquivalentTo(new[] { package1, package2, package3, package4, package5 }));
  }

  [Test]
  public void DependentPackagedWithMultipleRootsShouldBeCollected()
  {
    var package1 = SbomHelper.MakePackage();
    var package2 = SbomHelper.MakePackage();
    var package3 = SbomHelper.MakePackage();
    var package4 = SbomHelper.MakePackage();
    var package5 = SbomHelper.MakePackage();
    var package6 = SbomHelper.MakePackage();

    var relationships = new List<Relationship>
    {
      package1.RelatedTo(package2).As(RelationshipType.DEPENDS_ON),
      package2.RelatedTo(package3).As(RelationshipType.DEPENDS_ON),
      package4.RelatedTo(package5).As(RelationshipType.DEPENDS_ON),
      package6.RelatedTo(package5).As(RelationshipType.DEPENDENCY_OF),
    };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = new List<Package>() { package1, package2, package3, package4, package5, package6 },
      Files = new List<File>() {},
      Relationships = relationships
    };

    var logger = LoggerFactory.Create(builder => { }).CreateLogger("test");
    SbomReader reader = new SbomReader(logger);

    var spdxModel = reader.LoadSbom(document);
    var sbomOperations = new SbomOperations(logger);

    //Act 1
    var dependantPackages = sbomOperations.GetAllDependantPackages(spdxModel, new[] { package1 });

    //Assert 1
    Assert.That(dependantPackages, Is.EquivalentTo(new[] { package1, package2, package3 }));

    //Act 2
    var dependantPackages2 = sbomOperations.GetAllDependantPackages(spdxModel, new[] { package1, package4 });

    //Assert 2
    Assert.That(dependantPackages2, Is.EquivalentTo(new[] { package1, package2, package3, package4, package5, package6 }));
  }

  [Test]
  public void CircularDependenciesShouldNotBrake()
  {
    var package1 = SbomHelper.MakePackage();
    var package2 = SbomHelper.MakePackage();
    var package3 = SbomHelper.MakePackage();
    var package4 = SbomHelper.MakePackage();
    var package5 = SbomHelper.MakePackage();
    var package6 = SbomHelper.MakePackage();

    var relationships = new List<Relationship>
    {
      package1.RelatedTo(package2).As(RelationshipType.DEPENDS_ON),
      package2.RelatedTo(package3).As(RelationshipType.DEPENDS_ON),
      package3.RelatedTo(package1).As(RelationshipType.DEPENDS_ON),
      package2.RelatedTo(package4).As(RelationshipType.DEPENDS_ON),
    };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = new List<Package>() { package1, package2, package3, package4, package5, package6 },
      Files = new List<File>() {},
      Relationships = relationships
    };

    var logger = LoggerFactory.Create(builder => { }).CreateLogger("test");
    SbomReader reader = new SbomReader(logger);

    var spdxModel = reader.LoadSbom(document);
    var sbomOperations = new SbomOperations(logger);

    //Act 1
    var dependantPackages = sbomOperations.GetAllDependantPackages(spdxModel, new[] { package1 });

    //Assert 1
    Assert.That(dependantPackages, Is.EquivalentTo(new[] { package1, package2, package3, package4 }));
  }
}