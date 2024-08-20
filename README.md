# JetBrains SBOM Utils [![official JetBrains project](https://jb.gg/badges/official.svg)](https://confluence.jetbrains.com/display/ALL/JetBrains+on+GitHub)

[![Build and run tests](https://github.com/JetBrains/sbom-utils/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/JetBrains/sbom-utils/actions/workflows/build-and-test.yml)

<!---[![NuGet Badge](https://buildstats.info/nuget/JetBrains.SbomUtils)](https://www.nuget.org/packages/JetBrains.SbomUtils)-->

Tool to check that the installed software complies with the provided SBOM document (SPDX 2.3 format is suported).

JetBrains sbom-utils library is applicable for:

- Check that all files in the software installation directory are present in the SBOM document

- Check file checksums (SHA-1, SHA-256, SHA-384, SHA-512 algorithms are supported)

- Verify one or several applications at once


## How to use the console version:

### Verify one application

`./JetBrains.SbomUtils.Console analyze`

Parameters

| Full name        | Short | Explanation                                                                           | Example                                                                                                                                                                                               |
|:-----------------|:------|:--------------------------------------------------------------------------------------|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `--sbom`         | `-s`  | Path to the SBOM document                                                             |                                                                                                                                                                                                       |
| `--path`         | `-p`  | Path to the installed software                                                        |                                                                                                                                                                                                       |
| `--root-package` | `-r`  | Optional list of root packages of the software. If ommited, all packages will be used | `--root-package "JetBrains.dotCover"`                                                                                                                                                                                     |
| `--ignore`       | `-i`  | Files and file patterns to ignore                                                     | `--ignore "uninstall"`: ignore specific file<br/> `--ignore "test/*.deps.json"`: ignore file pattern in specific directory<br/> `--ignore "*/*.deps.json"`: ignore file pattern in all subdirectories |
| `--verbose`     | `-v`  | Write verbose logs                           |                                                                                                                                                                                                       |

### Verify multiple application at once

`./JetBrains.SbomUtils.Console analyze-batch`


| Full name       | Short | Explanation                                  |
|:----------------|:------|:---------------------------------------------|
| `--sbom`        | `-s`  | Path to the SBOM document                    |
| `--json-config` | `-j`  | Path to the analysis configuration json file |
| `--verbose`     | `-v`  | Write verbose logs                           |

Example of the json analysis configuration:

```json
{
  "Products": [
    {
      "Name": "Rider from the dotUltimate",
      "RootDirectory": "C:\\Program Files\\JetBrains\\Rider",
      "RootPackages": [
        "JetBrains.Rider.RiderProduct"
      ]
    }
  ],
  "Ignores": [
    "*/*.deps.json",
    "*/*ThirdPartyLibraries.html",        
    "packages",
    "uninstall"
  ]
}
```