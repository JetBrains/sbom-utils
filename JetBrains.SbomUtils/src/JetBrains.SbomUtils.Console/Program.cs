using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JetBrains.SbomUtils.Console;

using System;

class Program
{
    enum ExitCodes
    {
        Ok = 0,
        SbomInvalid = 1,
        ArgumentsError = 2,
    }

    static async Task<int> Main(string[] args)
    {
        RootCommand rootCommand = new RootCommand(
            description: "Analyze that the installed software complies with the SBOM.");

        var analyzeSingle = new Command("analyze", "Analyze that the installed software complies with the SBOM.");
        var analyzeBatch = new Command("analyze-batch", "Analyze that all installed software in the provided json complies with the SBOM.");

        rootCommand.AddCommand(analyzeSingle);
        rootCommand.AddCommand(analyzeBatch);

        var programRootDirectory = new Option<string>(
            aliases: ["--path", "-p"],
            description: "The path to the installed software.")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        var sbomFileOption = new Option<string>(
            aliases: ["--sbom", "-s"],
            description: "The path to the SBOM in SPDX 2.3 format.")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        var rootPackage = new Option<string[]>(
            aliases: ["--root-package", "-r"],
            description: "One or multiple root packages.")
        {
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        var exemptionsOption = new Option<string[]>(
            aliases: ["--ignore", "-i"],
            description: "Files and file patterns to ignore.")
        {
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        var jsonConfigOption = new Option<string>(
            aliases: ["--json-config", "-j"],
            description: "Projects configuration in json file")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };

        var verboseOption = new Option<bool>(
            aliases: ["--verbose", "-v"],
            description: "Write verbose logs")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };
        analyzeBatch.AddOption(jsonConfigOption);

        rootCommand.AddGlobalOption(sbomFileOption);
        rootCommand.AddGlobalOption(verboseOption);

        analyzeSingle.AddOption(programRootDirectory);
        analyzeSingle.AddOption(rootPackage);
        analyzeSingle.AddOption(exemptionsOption);

        analyzeSingle.SetHandler((rootDirectory, sbom, rootPackages, exemptions, verbose) => Analyze(sbom, rootDirectory, rootPackages, exemptions, verbose),
            programRootDirectory,
            sbomFileOption,
            rootPackage,
            exemptionsOption,
            verboseOption);

        analyzeBatch.SetHandler((sbom, jsonConfig, verbose) => AnalyzeBatch(sbom, jsonConfig, verbose),
            sbomFileOption,
            jsonConfigOption,
            verboseOption);

 return await rootCommand.InvokeAsync(args);
    }

    private static Task<int> AnalyzeBatch(string sbom, string jsonConfig, bool verbose)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddFilter(level => level >= LogLevel.Information || verbose).AddConsole());
        var logger = factory.CreateLogger<SbomValidator>();

        if (!File.Exists(jsonConfig))
        {
            logger.LogError("Provided path {path} to the json configuration file doesn't exist", jsonConfig);
            return Task.FromResult((int)ExitCodes.ArgumentsError);
        }

        BatchVerificationParams? verificationParams = null;

        try
        {
            using (StreamReader file = System.IO.File.OpenText(jsonConfig))
            {
                JsonSerializer serializer = new JsonSerializer();
                verificationParams = serializer.Deserialize<BatchVerificationParams>(new JsonTextReader(file));
            }

            if (verificationParams == null)
            {
                logger.LogError("Failed to parse SBOM");
                return Task.FromResult((int)ExitCodes.ArgumentsError);
            }
        }
        catch (Exception e)
        {
            throw new SbomValidationException($"Failed to parse SBOM: {e.Message}", e);
        }

        var analyzer = new Analyzer(logger);

        return Task.FromResult(analyzer.Analyze(sbom, verificationParams));
    }

    static Task<int> Analyze(string sbom, string rootDirectory, string[] rootPackages, string[] exemptions, bool verbose)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddFilter(level => level >= LogLevel.Information || verbose).AddConsole());
        var logger = factory.CreateLogger<SbomValidator>();
        var analyzer = new Analyzer(logger);

        return Task.FromResult(analyzer.Analyze(sbom, rootDirectory, rootPackages, exemptions));
    }
}