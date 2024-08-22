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

    public SbomValidator(ILogger logger): this(logger, new SbomOperations(logger)) { }

    internal SbomValidator(ILogger logger, SbomOperations sbomOperations)
    {
      _logger = logger;
      _sbomOperations = sbomOperations;
    }

    public ValidationResult ValidateInstallation(SbomModel sbomModel, IDiskOperations diskOperations, ICollection<string> rootPackageNames, ICollection<string> exceptions)
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

      _logger.LogDebug("{count} dependent packages were collected", productPackages.Count);

      var packagesHashSet = productPackages.ToHashSet();
      var (installationFiles, ignoredFiles) = diskOperations.GetFilesFromInstallationDirectory(exceptions);

      ConcurrentBag<MissingFile> missingFiles = new ConcurrentBag<MissingFile>();
      ConcurrentBag<FileVerificationResult> fileVerificationResults = new ConcurrentBag<FileVerificationResult>();

      ParallelOptions parallelOptions = new ParallelOptions()
      {
        MaxDegreeOfParallelism = 1,
      };

      Parallel.ForEach(installationFiles, parallelOptions, (installationFile, token) =>
      {
        if (sbomModel.FilesDictionaryByRelativePath.TryGetValue(installationFile, out var sbomFiles))
        {
          var fileValidationResult = ValidateFile(diskOperations, installationFile, sbomFiles, packagesHashSet);

          fileVerificationResults.Add(fileValidationResult);
        }
        else
        {
          var fileName = Path.GetFileName(installationFile);

          if (sbomModel.FilesDictionaryByFileName.TryGetValue(fileName, out var filesFromAnotherPackages))
            missingFiles.Add(new MissingFile(installationFile, filesFromAnotherPackages));
          else
            missingFiles.Add(new MissingFile(installationFile, Array.Empty<FileInfo>()));
        }
      });

      var validationResult = new ValidationResult(
        success: !(missingFiles.Any() || fileVerificationResults.Any(f => !f.Success)),
        errorMessage: null,
        filesChecked: installationFiles.Count,
        ignoredFiles: ignoredFiles,
        filesMissingInSbom: missingFiles.ToArray(),
        fileVerificationResults: fileVerificationResults.ToArray());

      return validationResult;
    }

    private FileVerificationResult ValidateFile(IDiskOperations diskOperations, string installationFile, List<FileInfo> sbomFiles, HashSet<Package> packagesHashSet)
    {
      var filesFromReferencedPackages = sbomFiles.Where(f => packagesHashSet.Contains(f.Package)).ToList();
      var hashAlgorithms = sbomFiles.SelectMany(f => f.File.Checksums).Select(c => c.Algorithm).Distinct();

      var hashesDictionary = diskOperations.CalculateHashes(installationFile, hashAlgorithms);

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
  }
}