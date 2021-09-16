# Database Formats
## 0.1.1
### General
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Data Version="0.1.0">
  <Config>
    <NewProfileSavePath>%APPDATA%\Powershell\FactorioProfiles\Profiles</DefaultPath>
    <NewProfileSharingSettings Config="False" Mods="False" Saves="False" Scenarios="False" Blueprints="False" />
  </Config>
  <ActiveProfile>...</ActiveProfile> <!-- NEW -->
  <Profiles>
    <Profile>
        ...
    </Profile>
  </Profiles>
</Data>
```

`ActiveProfile` holds the currently active profile. This is used for tracking so that the `Sync-` cmdlet doesn't need a profile name specified.
- This is defined when the `Switch-FactorioProfile` cmdlet is ran.
- Migration from earlier database version:
  - If the `Switch-` cmdlet is ran, this value will be created and filled in.
  - If a cmdlet which doesn't write to the database, such as `Get-`, this value will not exist.
  - Any other cmdlet will create the element but leave the value empty.

## 0.1.0
### General
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Data Version="0.1.0">
  <Config>
    <NewProfileSavePath>%APPDATA%\Powershell\FactorioProfiles\Profiles</DefaultPath>
    <NewProfileSharingSettings Config="False" Mods="False" Saves="False" Scenarios="False" Blueprints="False" />
  </Config>
  <Profiles>
    <Profile>
        ...
    </Profile>
  </Profiles>
</Data>
```

`NewProfileSavePath` defines where a new profile is created by default.
`NewProfileSharingSettings` defines what the settings are by default when a new profile is created.

### Profile
```xml
<Profile>
  <Name>test</Name>
  <Path>C:\Users\Kuba\AppData\Roaming\Powershell\FactorioProfiles\Profiles\test</Path>
  <SharingSettings Config="True" Mods="False" Saves="False" Scenarios="False" Blueprints="False" />
</Profile>
```

Current parts of a profile which can be shared:
- Configuration file
- Mods
- Savegames
- Scenarios
- Blueprints
