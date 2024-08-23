using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;
using Moq;
using File = System.IO.File;

namespace JetBrains.SbomUtils.Tests;

public class ZipArchiveVerificationTests
{
  private (byte[] Sha1, byte[] Sha256) CreateFile(ZipArchive archive, string filename, byte[] content)
  {
    var file1Entry = archive.CreateEntry(filename, CompressionLevel.Optimal);

    using (Stream file1Stream = file1Entry.Open())
    {
      file1Stream.Write(content, 0, content.Length);
    }

    byte[] sha1 = SHA1.Create().WithDispose(h => h.ComputeHash(content));
    byte[] sha256 = SHA256.Create().WithDispose(h => h.ComputeHash(content));

    return (sha1, sha256);
  }

  [Test]
  public void CorrectInstallationShouldPassValidation()
  {
    //Arrange
    var memoryStream = new MemoryStream();
    ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

    var file1Hashes = CreateFile(archive, "test.txt", "This is a sample file"u8.ToArray());
    var file2Hashes = CreateFile(archive, "src/test.cs", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."u8.ToArray());
    var file3Hashes = CreateFile(archive, "lib/test.txt", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo."u8.ToArray());

    var package1 = SbomHelper.MakePackage();

    var file1 = SbomHelper.MakeFile("./test.txt")
      .AddChecksum(ChecksumAlgorithm.SHA1, HashCalculator.ToSbomHash(file1Hashes.Sha1))
      .AddChecksum(ChecksumAlgorithm.SHA256, HashCalculator.ToSbomHash(file1Hashes.Sha256));

    var file2 = SbomHelper.MakeFile("./src/test.cs")
      .AddChecksum(ChecksumAlgorithm.SHA1, HashCalculator.ToSbomHash(file2Hashes.Sha1))
      .AddChecksum(ChecksumAlgorithm.SHA256, HashCalculator.ToSbomHash(file2Hashes.Sha256));

    var file3 = SbomHelper.MakeFile("./lib/test.txt")
      .AddChecksum(ChecksumAlgorithm.SHA1, HashCalculator.ToSbomHash(file3Hashes.Sha1))
      .AddChecksum(ChecksumAlgorithm.SHA256, HashCalculator.ToSbomHash(file3Hashes.Sha256));

    archive.Dispose();
    memoryStream.Rewind();

    var relationships = new List<Relationship>() { "document".Describes(package1), package1.ContainsFile(file1), package1.ContainsFile(file2), package1.ContainsFile(file3) };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = [package1],
      Files = [file1, file2, file3],
      Relationships = relationships,
    };

    var logger = LoggerFactory.Create(builder => { }).CreateLogger("test");
    SbomReader reader = new SbomReader(logger);

    var spdxModel = reader.LoadSbom(document);

    SbomValidator validator = new SbomValidator(logger, new SbomOperations(logger));

    //Act
    ValidationResult validationResult;
    using (var zipSoftwareProvider = new ZipArchiveSoftwareProvider(memoryStream))
    {
      validationResult = validator.ValidateInstallation(spdxModel, zipSoftwareProvider, new List<string>() { package1.Name }, Array.Empty<string>(), new ValidationOptions(){ Threads = 1});
    }

    //Assert
    Assert.That(validationResult.Success, Is.True);
    Assert.That(validationResult.ErrorMessage, Is.Null);
    Assert.That(validationResult.IgnoredFiles, Is.Empty);
    Assert.That(validationResult.FilesMissingInSbom, Is.Empty);
    Assert.That(validationResult.FilesChecked, Is.EqualTo(3));
  }

  [Test]
  public void HashConflictsShouldBeDetected()
  {
    //Arrange
    var memoryStream = new MemoryStream();
    ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

    var file1Hashes = CreateFile(archive, "test.txt", "This is a sample file"u8.ToArray());
    var file2Hashes = CreateFile(archive, "src/test.cs", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."u8.ToArray());
    var file3Hashes = CreateFile(archive, "lib/test.txt", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo."u8.ToArray());

    var package1 = SbomHelper.MakePackage();

    file1Hashes.Sha1[0] = 0;
    file2Hashes.Sha256[0] = 0;
    file3Hashes.Sha1[0] = 0;
    file3Hashes.Sha256[0] = 0;

    var file1 = SbomHelper.MakeFile("./test.txt")
      .AddChecksum(ChecksumAlgorithm.SHA1, HashCalculator.ToSbomHash(file1Hashes.Sha1))
      .AddChecksum(ChecksumAlgorithm.SHA256, HashCalculator.ToSbomHash(file1Hashes.Sha256));

    var file2 = SbomHelper.MakeFile("./src/test.cs")
      .AddChecksum(ChecksumAlgorithm.SHA1, HashCalculator.ToSbomHash(file2Hashes.Sha1))
      .AddChecksum(ChecksumAlgorithm.SHA256, HashCalculator.ToSbomHash(file2Hashes.Sha256));

    var file3 = SbomHelper.MakeFile("./lib/test.txt")
      .AddChecksum(ChecksumAlgorithm.SHA1, HashCalculator.ToSbomHash(file3Hashes.Sha1))
      .AddChecksum(ChecksumAlgorithm.SHA256, HashCalculator.ToSbomHash(file3Hashes.Sha256));

    archive.Dispose();
    memoryStream.Rewind();

    var relationships = new List<Relationship>() { "document".Describes(package1), package1.ContainsFile(file1), package1.ContainsFile(file2), package1.ContainsFile(file3) };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = [package1],
      Files = [file1, file2, file3],
      Relationships = relationships,
    };

    var logger = LoggerFactory.Create(builder => { }).CreateLogger("test");
    SbomReader reader = new SbomReader(logger);

    var spdxModel = reader.LoadSbom(document);

    SbomValidator validator = new SbomValidator(logger, new SbomOperations(logger));

    //Act
    ValidationResult validationResult;
    using (var zipSoftwareProvider = new ZipArchiveSoftwareProvider(memoryStream))
    {
      validationResult = validator.ValidateInstallation(spdxModel, zipSoftwareProvider, new List<string>() { package1.Name }, Array.Empty<string>(), new ValidationOptions(){ Threads = 1});
    }

    //Assert
    Assert.That(validationResult.Success, Is.False);
    Assert.That(validationResult.ErrorMessage, Is.Null);
    Assert.That(validationResult.IgnoredFiles, Is.Empty);
    Assert.That(validationResult.FilesMissingInSbom, Is.Empty);
    Assert.That(validationResult.FilesChecked, Is.EqualTo(3));
    Assert.That(validationResult.FileVerificationResults, Has.Length.EqualTo(3) );

    var file1HashFailure = validationResult.FileVerificationResults.SingleOrDefault(f => f.FilePath == SbomHelper.NormalizePath(file1.FileName));
    var file2HashFailure = validationResult.FileVerificationResults.SingleOrDefault(f => f.FilePath == SbomHelper.NormalizePath(file2.FileName));
    var file3HashFailure = validationResult.FileVerificationResults.SingleOrDefault(f => f.FilePath == SbomHelper.NormalizePath(file3.FileName));

    Assert.NotNull(file1HashFailure);
    Assert.That(file1HashFailure?.HashVerificationFailure?.HashMismatches, Has.Length.EqualTo(1));
    Assert.That(file1HashFailure?.HashVerificationFailure?.HashMismatches.First().FaultedChecksum.Algorithm, Is.EqualTo(ChecksumAlgorithm.SHA1));

    Assert.NotNull(file2HashFailure);
    Assert.That(file2HashFailure?.HashVerificationFailure?.HashMismatches, Has.Length.EqualTo(1));
    Assert.That(file2HashFailure?.HashVerificationFailure?.HashMismatches.First().FaultedChecksum.Algorithm, Is.EqualTo(ChecksumAlgorithm.SHA256));

    Assert.NotNull(file3HashFailure);
    Assert.That(file3HashFailure?.HashVerificationFailure?.HashMismatches, Has.Length.EqualTo(2));
    Assert.That(file3HashFailure?.HashVerificationFailure?.HashMismatches.First().FaultedChecksum.Algorithm, Is.EqualTo(ChecksumAlgorithm.SHA1));
    Assert.That(file3HashFailure?.HashVerificationFailure?.HashMismatches.Last().FaultedChecksum.Algorithm, Is.EqualTo(ChecksumAlgorithm.SHA256));
  }

  [Test]
  public void FilesWithIdenticalNamesInDifferentPackagesShouldBeResolved()
  {
    //Arrange
    var memoryStream = new MemoryStream();
    ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

    var file1Hashes = CreateFile(archive, "test.txt", "This is a sample file"u8.ToArray());

    var package1 = SbomHelper.MakePackage();
    var package2 = SbomHelper.MakePackage();

    var file1 = SbomHelper.MakeFile("./test.txt")
      .AddChecksum(ChecksumAlgorithm.SHA1, HashCalculator.ToSbomHash(file1Hashes.Sha1))
      .AddChecksum(ChecksumAlgorithm.SHA256, HashCalculator.ToSbomHash(file1Hashes.Sha256));

    var file2 = SbomHelper.MakeFile("./test.txt")
      .AddChecksum(ChecksumAlgorithm.SHA1, "0000000000000000000000000000000000000000")
      .AddChecksum(ChecksumAlgorithm.SHA256, "000000000000000000000000000000000000000000000000000000000000");

    archive.Dispose();


    var relationships = new List<Relationship>() { "document".Describes(package1), package1.ContainsFile(file1), package2.ContainsFile(file2), package1.DependsOn(package2) };

    SpdxDocument document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = [package1, package2],
      Files = [file1, file2 ],
      Relationships = relationships,
    };

    memoryStream.Rewind();

    //Act 1
    //Assert 1
    VerifyAndCheckForSuccess(document, memoryStream);

    document = new SpdxDocument()
    {
      SPDXID = "document",
      Packages = [package2, package1],
      Files = [file2, file1 ],
      Relationships = relationships,
    };

    memoryStream.Rewind();

    //Act 2
    //Assert 2
    VerifyAndCheckForSuccess(document, memoryStream);
  }

  private static void VerifyAndCheckForSuccess(SpdxDocument document, MemoryStream memoryStream)
  {
    var logger = LoggerFactory.Create(builder => { }).CreateLogger("test");
    SbomReader reader = new SbomReader(logger);

    var spdxModel = reader.LoadSbom(document);

    SbomValidator validator = new SbomValidator(logger, new SbomOperations(logger));

    //Act
    ValidationResult validationResult;
    using (var zipSoftwareProvider = new ZipArchiveSoftwareProvider(memoryStream, leaveOpen: true))
    {
      validationResult = validator.ValidateInstallation(spdxModel, zipSoftwareProvider, Array.Empty<string>(), Array.Empty<string>(), new ValidationOptions(){ Threads = 1});
    }

    //Assert
    Assert.That(validationResult.Success, Is.True);
    Assert.That(validationResult.ErrorMessage, Is.Null);
    Assert.That(validationResult.IgnoredFiles, Is.Empty);
    Assert.That(validationResult.FilesMissingInSbom, Is.Empty);
  }
}