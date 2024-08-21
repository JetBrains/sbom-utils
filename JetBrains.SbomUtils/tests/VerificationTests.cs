using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetBrains.SbomUtils.Tests;

public class VerificationTests
{
  [Test]
  public void VerifySimpleInstallation()
  {
    //Arrange
    var package1 = SbomHelper.MakePackage();

    var file1 = SbomHelper.MakeFile("./test.txt").AddChecksum(ChecksumAlgorithm.SHA1, "0011223344");

    var relationships = new List<Relationship>() { "document".Describes(package1), package1.ContainsFile(file1) };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = [package1],
      Files = [file1],
      Relationships = relationships,
    };

    var logger = LoggerFactory.Create(builder => { }).CreateLogger("test");
    SbomReader reader = new SbomReader(logger);

    var spdxModel = reader.LoadSbom(document);
    Mock<IDiskOperations> diskOperationsMock = new Mock<IDiskOperations>(MockBehavior.Strict);
    var rootPath = Path.GetTempPath();
    diskOperationsMock.Setup(d => d.GetFilesFromInstallationDirectory(It.Is<string>(p => p == rootPath), It.IsAny<IEnumerable<string>>()))
      .Returns(([Path.Combine(rootPath,file1.FileName)], Array.Empty<string>()));

    diskOperationsMock
      .Setup(d => d.CalculateHashes(
        It.Is<string>(p => p == Path.Combine(rootPath, file1.FileName)),
        It.IsAny<IEnumerable<ChecksumAlgorithm>>()))
      .Returns(new Dictionary<ChecksumAlgorithm, byte[]>()
      {
        { ChecksumAlgorithm.SHA1, [0x00, 0x11, 0x22, 0x33, 0x44] }
      });

    SbomValidator validator = new SbomValidator(logger, new SbomOperations(logger), diskOperationsMock.Object);

    //Act
    var validationResult = validator.ValidateInstallation(spdxModel, rootPath, new List<string>() { package1.Name }, Array.Empty<string>());

    //Assert
    Assert.That(validationResult.Success, Is.True);
    Assert.That(validationResult.ErrorMessage, Is.Null);
    Assert.That(validationResult.IgnoredFiles, Is.Empty);
    Assert.That(validationResult.FilesMissingInSbom, Is.Empty);
    Assert.That(validationResult.FilesChecked, Is.EqualTo(1));
  }
}