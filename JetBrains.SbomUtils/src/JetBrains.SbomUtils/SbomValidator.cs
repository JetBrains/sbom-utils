using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using JetBrains.SbomUtils.Models;
using Microsoft.Extensions.Logging;
using Range = System.Range;

namespace JetBrains.SbomUtils
{
  public class SbomValidator
  {
    private readonly ILogger _logger;
    private readonly SbomOperations _sbomOperations;

    public SbomValidator(ILogger logger)
    {
      _logger = logger;
      _sbomOperations = new SbomOperations(_logger);
    }

    public ValidationResult ValidateInstallation(SbomModel sbomModel, string pathToInstallation, ICollection<string> rootPackageNames, ICollection<string> exceptions)
    {
      if (sbomModel == null)
        throw new InvalidOperationException("Load an spdx document first");

      ICollection<Package> productPackages;

      if (rootPackageNames.Any())
      {
        var rootPackages = _sbomOperations.FindRootPackageByName(sbomModel, rootPackageNames);
        if (!rootPackages.Any()) throw new InvalidOperationException($"None of root packages {string.Join(", ", rootPackageNames)} wasn't found in the SBOM");

        _logger.LogDebug("Found {count} root packages in the SBOM: {packages}", rootPackages.Count(), string.Join(", ", rootPackages.Select(p => p.Name)));

        productPackages = _sbomOperations.GetAllDependantPackages(sbomModel, rootPackages);
      }
      else
      {
        productPackages = sbomModel.Packages.Values;
      }

      _logger.LogDebug("{count} dependant packages were collected", productPackages.Count);

      var packagesHashSet = productPackages.ToHashSet();
      var installationFiles = GetFilesFromInstallationDirectory(pathToInstallation, exceptions, out var ignoredFiles);

      ConcurrentBag<MissingFile> missingFiles = new ConcurrentBag<MissingFile>();
      ConcurrentBag<FileVerificationResult> fileVerificationResults = new ConcurrentBag<FileVerificationResult>();

      ParallelOptions parallelOptions = new ParallelOptions()
      {
        MaxDegreeOfParallelism = 10,
      };

      Parallel.ForEach(installationFiles, parallelOptions, (installationFile, token) =>
      {
        var relativeFilePath = Path.GetRelativePath(pathToInstallation, installationFile);

        if (sbomModel.FilesDictionaryByRelativePath.TryGetValue(relativeFilePath, out var sbomFiles))
        {
          var fileValidationResult = ValidateFile(installationFile, sbomFiles, packagesHashSet);

          fileVerificationResults.Add(fileValidationResult);
        }
        else
        {
          var fileName = Path.GetFileName(installationFile);
          var relativePath = Path.GetRelativePath(pathToInstallation, installationFile);

          if (sbomModel.FilesDictionaryByFileName.TryGetValue(fileName, out var filesFromAnotherPackages))
            missingFiles.Add(new MissingFile(relativePath, filesFromAnotherPackages));
          else
            missingFiles.Add(new MissingFile(relativePath, Array.Empty<FileInfo>()));
        }
      });

      var validationResult = new ValidationResult(
        success: !(missingFiles.Any() || fileVerificationResults.Any(f => !f.Success)),
        errorMessage: null,
        filesChecked: installationFiles.Length,
        ignoredFiles: ignoredFiles,
        filesMissingInSbom: missingFiles.ToArray(),
        fileVerificationResults: fileVerificationResults.ToArray());

      return validationResult;
    }

    private FileVerificationResult ValidateFile(string installationFile, List<FileInfo> sbomFiles, HashSet<Package> packagesHashSet)
    {
      var filesFromReferencedPackages = sbomFiles.Where(f => packagesHashSet.Contains(f.Package)).ToList();
      var hashAlgorithms = sbomFiles.SelectMany(f => f.File.Checksums).Select(c => c.Algorithm).Distinct();

      var hashesDictionary = CalculateHashes(installationFile, hashAlgorithms);

      if (filesFromReferencedPackages.Any())
      {
        HashVerificationFailure? hashVerificationFailure = null;

        foreach (var sbomFile in filesFromReferencedPackages)
        {
          hashVerificationFailure = ValidateHashes(hashesDictionary, sbomFile);
          if (hashVerificationFailure == null)
            break;
        }

        return new FileVerificationResult(installationFile, hashVerificationFailure == null, hashVerificationFailure, null);
      }
      else
      {
        HashVerificationFailure? hashVerificationFailure = null;
        FileFromUnreferencedPackage? fileFromUnreferencedPackage = null;

        foreach (var sbomFile in sbomFiles)
        {
          hashVerificationFailure = ValidateHashes(hashesDictionary, sbomFile);
          if (hashVerificationFailure == null)
          {
            fileFromUnreferencedPackage = new FileFromUnreferencedPackage(sbomFile);
            break;
          }
        }

        return new FileVerificationResult(
          installationFile,
          hashVerificationFailure == null && fileFromUnreferencedPackage == null,
          hashVerificationFailure,
          fileFromUnreferencedPackage);
      }
    }

    protected Dictionary<ChecksumAlgorithm, byte[]> CalculateHashes(string installationFile, IEnumerable<ChecksumAlgorithm> algorithms)
    {
      Dictionary<ChecksumAlgorithm, byte[]> hashes = new Dictionary<ChecksumAlgorithm, byte[]>();

      using (var file = System.IO.File.OpenRead(installationFile))
      {
        foreach (var algorithm in algorithms)
        {
          var hashOnDisk = CreateHashAlgorithm(algorithm).WithDispose(a => a.ComputeHash(file.Rewind()));
          hashes.Add(algorithm, hashOnDisk);
        }
      }

      return hashes;
    }

    protected HashVerificationFailure? ValidateHashes(Dictionary<ChecksumAlgorithm, byte[]> fileHashes, FileInfo fileInfo)
    {
      List<HashMismatch> mismatches = new List<HashMismatch>(fileInfo.File.Checksums.Count);

      foreach (var fileChecksum in fileInfo.File.Checksums)
      {
        var hashOnDisk = fileHashes[fileChecksum.Algorithm];

        if (fileChecksum.ChecksumValue.Length % 2 != 0)
          throw new InvalidOperationException($"Hash length for function {fileChecksum.Algorithm} for file {fileInfo.File.SPDXID} has odd length");

        var hashFromSbom = ParseHashValue(fileChecksum.ChecksumValue);

        if (!Utils.ByteArraysEqual(hashOnDisk, hashFromSbom))
          mismatches.Add(new HashMismatch(fileChecksum, hashOnDisk));
      }

      return mismatches.Any() ? new HashVerificationFailure(fileInfo, mismatches) : null;
    }

    protected byte[] ParseHashValue(string hash)
    {
      if (hash.Length % 2 != 0)
        throw new InvalidOperationException($"Hash length of value {hash} is {hash.Length}, but even length is expected");

      byte[] result = new byte[hash.Length / 2];

      for (int i = 0; i < result.Length; i++)
        result[i] = byte.Parse(hash[new Range(i * 2, i * 2 + 2)], NumberStyles.HexNumber);

      return result;
    }

    protected HashAlgorithm CreateHashAlgorithm(ChecksumAlgorithm algorithm) => algorithm switch
    {
      ChecksumAlgorithm.SHA1 => SHA1.Create(),
      ChecksumAlgorithm.SHA256 => SHA256.Create(),
      ChecksumAlgorithm.SHA384 => SHA384.Create(),
      ChecksumAlgorithm.SHA512 => SHA512.Create(),
      _ => throw new NotSupportedException($"Hash algorithm {algorithm} is not supported"),
    };

    protected string[] GetFilesFromInstallationDirectory(string installationPath, IEnumerable<string> exemptions, out ICollection<string> ignoredFiles)
    {
      var allFiles = Directory.GetFiles(installationPath, "*", new EnumerationOptions() { RecurseSubdirectories = true }).Select(Path.GetFullPath).ToHashSet();
      ignoredFiles = new List<string>();

      string[] recursiveDirectoryPatterns = new string[] { "*/", "*\\", "**/", "**\\" };

      foreach (var exemptionPattern in exemptions)
      {
        var pattern = exemptionPattern;
        bool recurseSubdirectories = false;

        foreach (var recursiveDirectoryPattern in recursiveDirectoryPatterns)
        {
          if (exemptionPattern.StartsWith(recursiveDirectoryPattern))
          {
            recurseSubdirectories = true;
            pattern = exemptionPattern.Substring(recursiveDirectoryPattern.Length);
            break;
          }
        }

        var exemptedFiles = Directory.GetFiles(installationPath, pattern, new EnumerationOptions() { RecurseSubdirectories = recurseSubdirectories });

        foreach (var exemptedFile in exemptedFiles)
        {
          var fullPath = Path.GetFullPath(exemptedFile);
          if (allFiles.Remove(fullPath))
            ignoredFiles.Add(Path.GetRelativePath(installationPath, fullPath));
        }
      }

      return allFiles.ToArray();
    }
  }
}