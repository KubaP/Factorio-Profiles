# Changelog
## 0.1.1 (2021-09-15)
### Added
 - `Sync-FactorioProfiles` cmdlet for syncing profiles if they are sharing blueprint data. For more information see the help page and the issue below.
 - Explanation of the blueprint sharing issue in the `about_FactorioProfiles` help page.
 - Store the currently active profile, and display this information in the `Get-FactorioProfileSettings` cmdlet.
 - A `-GlobalProfile` switch to the `Open-FactorioProfileFolder` cmdlet to open the "global" profile which contains the shared content.

### Changed
 - Database version bump to `0.1.1` to hold the new data, and data migration logic from `0.1.0`.

### Fixed
 - Blueprint data not being correctly shared between profiles and the "global" profile. However, due to the limitations of factorio's behaviour, this has introduced the necessity for a new cmdlet, `Sync-FactorioProfiles`. For more information about this, see the help page.
 - The `-Name` parameter for the `Switch-FactorioProfile` cmdlet not being mandatory.

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