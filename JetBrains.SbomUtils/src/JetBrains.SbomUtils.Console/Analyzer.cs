using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Extensions.Logging;

namespace JetBrains.SbomUtils.Console;

class Analyzer
{
  private readonly ILogger _logger;

  public Analyzer(ILogger logger)
  {
    _logger = logger;
  }

  public int Analyze(string sbom, string rootDirectory, string[] rootPackages, string[] exemptions)
  {
    SbomValidator validator = new SbomValidator(_logger);
    SbomReader reader = new SbomReader(_logger);
    var sbomModel = reader.LoadSbom(reader.ReadSbom(sbom));

    ValidationResult validationResult;

    try
    {
      using var softwareProvider = CreateInstalledSoftwareProvider(rootDirectory);
      validationResult = validator.ValidateInstallation(sbomModel, softwareProvider, rootPackages, exemptions);
    }
    catch (Exception ex)
    {
      validationResult = new ValidationResult(
        success: false,
        errorMessage: ex.Message,
        filesChecked: 0,
        filesMissingInSbom: [],
        fileVerificationResults: [],
        ignoredFiles: []);
    }

    PrintValidationResult(validationResult);

    return validationResult.Success ? 0 : 1;
  }

  protected IInstalledSoftwareProvider CreateInstalledSoftwareProvider(string path)
  {
    if (ZipArchiveSoftwareProvider.IsSupported(path)) return new ZipArchiveSoftwareProvider(path);
    if (DirectorySoftwareProvider.IsSupported(path)) return new DirectorySoftwareProvider(path);

    throw new Exception($"Unsupported disk operation: {path}");
  }

  public int Analyze(string sbom, BatchVerificationParams batchVerificationParams)
  {
    SbomValidator validator = new SbomValidator(_logger);
    SbomReader reader = new SbomReader(_logger);
    var sbomModel = reader.LoadSbom(reader.ReadSbom(sbom));
    bool success = true;

    foreach (var product in batchVerificationParams.Products)
    {
      _logger.LogInformation("=============== Checking product {product} =================", product.Name);

      ValidationResult validationResult;
      try
      {
        using var softwareProvider = CreateInstalledSoftwareProvider(product.RootDirectory);
        validationResult = validator.ValidateInstallation(sbomModel, softwareProvider, product.RootPackages, batchVerificationParams.Ignores);
      }
      catch (Exception ex)
      {
        validationResult = new ValidationResult(
          success: false,
          errorMessage: ex.Message,
          filesChecked: 0,
          filesMissingInSbom: [],
          fileVerificationResults: [],
          ignoredFiles: []);
      }

      PrintValidationResult(validationResult);

      success &= validationResult.Success;
    }

    return success ? 0 : 1;
  }

  public void PrintValidationResult(ValidationResult validationResult)
  {
    foreach (var missingFile in validationResult.FilesMissingInSbom)
    {
      string possibleCandidates;
      if (missingFile.PossibleCandidates.Any())
        possibleCandidates =
          $"Files with the same name:\n{string.Join("\n", missingFile.PossibleCandidates.OrderBy(c => c.NormalizedRelativePath).Select(c => $"{c.NormalizedRelativePath} from package {c.Package.Name}"))}";
      else
        possibleCandidates = "No files with the same name in the SBOM";

      _logger.LogWarning("File {file} is missing in SBOM. {candidates}", missingFile.FilePath, possibleCandidates);
    }

    int hashMismatches = 0;
    int filesFromUnreferencedPackages = 0;
    foreach (var fileVerificationResult in validationResult.FileVerificationResults)
    {
      if (fileVerificationResult.HashVerificationFailure != null)
      {
        hashMismatches++;
        _logger.LogWarning("File {file} from package {package} has {count} hash mismatches: {mismatches}",
          fileVerificationResult.HashVerificationFailure.FileInfo.File.FileName,
          fileVerificationResult.HashVerificationFailure.FileInfo.Package.Name,
          fileVerificationResult.HashVerificationFailure.HashMismatches.Count(),
          string.Join(", ",
            fileVerificationResult.HashVerificationFailure.HashMismatches.Select(m =>
              $"{m.FaultedChecksum.Algorithm}")));
      }

      if (fileVerificationResult.FileFromUnreferencedPackage != null)
      {
        filesFromUnreferencedPackages++;
        _logger.LogWarning("File {file} was found in the {package} which is not referenced by the root product",
          fileVerificationResult.FileFromUnreferencedPackage.FileInfo.File.FileName,
          fileVerificationResult.FileFromUnreferencedPackage.FileInfo.Package.Name);
      }
    }

    if (validationResult.Success)
    {
      _logger.LogInformation(
        "Validation of product successfully passed, {present} files were checked, {ignored} files were ignored",
        validationResult.FilesChecked,
        validationResult.IgnoredFiles.Length);
    }
    else
    {
      if (!validationResult.Success && !string.IsNullOrEmpty(validationResult.ErrorMessage))
        _logger.LogWarning("Validation error occured: {error}", validationResult.ErrorMessage);
      else
        _logger.LogWarning(
          "Validation of product failed: {missing} files are missing, {hashMismatches} files has hash mismatches, {unreferenced} files found in unreferenced packages, {total} files were checked, {passed} files passed validation, {ignored} files were ignored",
          validationResult.FilesMissingInSbom.Length,
          hashMismatches,
          filesFromUnreferencedPackages,
          validationResult.FilesChecked,
          validationResult.FileVerificationResults.Count(f => f.Success),
          validationResult.IgnoredFiles.Length);
    }
  }
}