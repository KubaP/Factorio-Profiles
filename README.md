![Banner](./img/logo.png#gh-light-mode-only)
![Banner](./img/logo_dark.png#gh-dark-mode-only)

<br>
<br>

<p align="center">
<a href="https://www.powershellgallery.com/packages/FactorioProfiles">
    <img src="https://img.shields.io/powershellgallery/v/FactorioProfiles?logo=powershell&logoColor=white">
</a>
<a href="">
    <img src="https://img.shields.io/powershellgallery/p/FactorioProfiles">
</a>
<a href="./LICENSE">
    <img src="https://img.shields.io/badge/license-GPLv3-blue">
</a>
</p>

<br>

Factorio-Profiles is a module designed to help manage multiple factorio profiles, by providing a set of simple and self-explanatory cmdlets to use inside of powershell.

This module is aimed at anyone who has multiple factorio instances they play with, but still wants to play the game through steam rather than manually keeping track of many portable factorio installations.

### Table of Contents
1. [Getting Started](#getting-started)
2. [Information & Features](#information--features)
3. [FAQ](#faq)
4. [Build Instructions](#build-instructions)
5. [Support](#support)
6. [Contributing](#contributing)
7. [License](#license)

## Getting Started
### Installation
In order to get started with the latest version, simply download the module from the [PSGallery](https://www.powershellgallery.com/packages/FactorioProfiles), or install it from powershell by running:
```powershell
Install-Module FactorioProfiles
```
Alternatively, download the `.zip` package from the [Releases](https://github.com/KubaP/Factorio-Profiles/releases) page, extract the contents, and copy over the `FactorioProfiles` folder to either:
- `~\Documents\Powershell\Modules` if you're using `pwsh >= 6.0`, or
- `~\Documents\WindowsPowershell\Modules` if you're using `Windows Powershell 5.1`

### Requirements
This module requires minimum `Powershell 5.1`.

This module works on **Windows** only, due to the windows-specific symbolic-link creation.

### Creating a new Profile
To create a new profile, simply run:
```powershell
New-FactorioProfile -Name <PROFILE_NAME>
```
This will create a new profile at the default location.

### Editing the contents of a Profile
To copy over existing mod/save files, modify the profile's contents, or do anything else, run:
```powershell
Open-FactorioProfileFolder -Name <PROFILE_NAME>
```
This will open `explorer.exe` at the profile folder.

### Switching to a Profile
To active a profile, run:
```powershell
Switch-FactorioProfile -Name <PROFILE_NAME>
```
This will switch to the specified profile. You can now launch the game through Steam or a standalone installation and this profile will be in use.

⚠It may be a good idea to disable the *Steam Cloud Sync* feature for the game if you are launching it through Steam.


## Information & Features
### Documentation
For a detailed rundown and explanation of all the features in this module, view the **help page** by running:
```powershell
Import-Module FactorioProfiles
Get-Help about_FactorioProfiles
```
For detailed help about a specific cmdlet, run:
```powershell
Get-Help <COMMAND NAME> -Full
```

### Extra features

#### Tab completion
The `Name`/`Names` parameter supports tab-completion of **existing** `Profile` names in the following cmdlets:
- `Get-FactorioProfile`
- `Set-FactorioProfileOption`
- `Remove-FactorioProfile`
- `Switch-FactorioProfile`
- `Open-FactorioProfileFolder`

#### Prompt support
The cmdlets do not support the `-WhatIf` nor the `-Confirm` switches. However, if any ambiguous or dangerous operations arise, a prompt will be displayed asking for user input.

#### Formatting
The `Profile` object within this module has custom formatting rules for all views. Simply pipe the output of the `Get-FactorioProfile` cmdlet to one of:
| Cmdlet        | Alias |
| ------------- | ----- |
| Format-List   | fl    |
| Format-Table  | ft    |
| Format-Custom | fc    |
| Format-Wide   | fw    |

The `Format-Custom` & `Format-List` views contain the largest amount of information regarding a `Profile`.

## FAQ
**Q**. Why not just have multiple independent portable factorio installations?

**A**. Portable factorio installations will not run through Steam. This is an issue if you like or rely on features Steam provides. Plus, even with portable installations, you would have to manually link files if you wanted to share "things" across multiple installations, such as sharing the config file.

**Q**. If running through Steam is so important, why not just modify the `--config` and `--mod-directory` launch parameters?

**A**. That requires manual editing each time you want to change the values, plus, there is no way to separate saves, scenarios, or other "things" in a factorio profile; only the configuration file and mods.

## Build Instructions
#### Prerequisites
Install the following:
- Powershell 5.1+
- dotnet 5.0+
- Pester **4.10.1**
- PSScriptAnalyzer 1.18.0+

#### Clone the git repo
```bash
git clone https://github.com/KubaP/Factorio-Profiles.git
```

#### Run the build scripts
Navigate to the root repository folder and run the following commands:
```powershell
& .\build\vsts-prerequisites.ps1
& .\build\vsts-validate.ps1
& .\build\vsts-build-prerequisites
& .\build\vsts-build.ps1 -WorkingDirectory .\ -SkipPublish
```
The built module will be located in the `.\publish` folder.

## Support
⚠If you need help regarding the usage of the module, please **first see the help page** by running:
```powershell
Import-Module FactorioProfiles
Get-Help about_FactorioProfiles
```

If something is still unclear, or there is a bug or problem, please create a new **Github Issue**.

## Contributing
If you have a suggestion, create a new **Github Issue** detailing the idea.

Feel free to make pull requests if you have an improvement. Only submit a single feature at a time, and make sure that the code is cleanly formatted, readable, and well commented.

## License 
This project is licensed under the **GPLv3** license - see [.\LICENSE](./LICENSE) for details.
