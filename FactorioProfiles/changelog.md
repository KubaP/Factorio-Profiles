# Changelog
## 0.1.0 (2021-08-29)
### Added
 - `New-FactorioProfile` cmdlet for creating a new profile folder.
 - `Get-FactorioProfile` cmdlet for retrieving the details of an existing profile.
 - `Set-FactorioProfileOption` cmdlet for setting an option for an existing profile, or setting a global module-wide option.
 - `Remove-FactorioProfile` cmdlet for deleting an existing profile.
 - `Switch-FactorioProfileOption` cmdlet for making a certain profile be the active one.
 - `Open-FactorioProfileFolder` cmdlet for opening `explorer.exe` at the location of a profile folder. This is useful if you want to manually edit/modify the files.
 - `Get-FactorioProfileSettings` cmdlet for retrieving the global settings for this module.
 - The `[Profile]` object for grouping relevant information, with appropriate public getters/setters.
 - Formatting styles for the `[Profile]` object, supporting all 4 formatting views.
 - Tab completion of profile names in the relevant cmdlets.
 - Main `about_FactorioProfiles` help page, with an overview of the module, how to use it, and any important notes.