# Create module-wide variables.
$script:ModuleRoot = $PSScriptRoot
$script:ModuleVersion = (Import-PowerShellDataFile -Path "$ModuleRoot\FactorioProfiles.psd1").ModuleVersion
$script:Folder = "$env:APPDATA\Powershell\FactorioProfiles"
$script:DataPath = "$env:APPDATA\Powershell\FactorioProfiles\database.xml"
# NOTE: These variables are redefined in the C# 'Data' class for use in native code.
# There technically exists a way to access these variables from within the c# code:
#    this.MyInvocation.MyCommand.Module.SessionState.PSVariable.GetValue("DataPath");
# This must be done within a class which inherits from 'PSCmdlet' rather than just 'Cmdlet'.

# For the debug output to be displayed, '$DebugPreference' must be set to 'Continue' within the current session.
Write-Debug "`e[4mMODULE-WIDE VARIABLES`e[0m"
Write-Debug "Module root folder: $ModuleRoot"
Write-Debug "Module version: $ModuleVersion"
Write-Debug "Data folder: $Folder"
Write-Debug "Database file: $DataPath"

if (-not (Test-Path -Path "$env:APPDATA\Powershell\FactorioProfiles" -ErrorAction Ignore)) {
	# Create the module data-storage folder if it doesn't exist.
	New-Item -ItemType Directory -Path "$env:APPDATA" -Name "Powershell\FactorioProfiles" -Force -ErrorAction Stop
	Write-Debug "Created the data storage folder!"
}

if (-not (Test-Path -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles" -ErrorAction Ignore)) {
	# Create the profile storage folder if it doesn't exist.
	New-Item -ItemType Directory -Path "$env:APPDATA\Powershell\FactorioProfiles" -Name "Profiles" -Force `
		-ErrorAction Stop
}

if (-not (Test-Path -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles\Global" -ErrorAction Ignore)) {
	# Create the folder which will contain the "global" factorio profile.
	New-Item -ItemType Directory -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles" -Name "Global" `
		-Force -ErrorAction Stop
	# Create the items which factorio uses.
	New-Item -ItemType Directory -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles\Global" `
		-Name "config" -Force -ErrorAction Stop	
	New-Item -ItemType Directory -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles\Global" `
		-Name "mods" -Force -ErrorAction Stop	
	New-Item -ItemType Directory -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles\Global" `
		-Name "saves" -Force -ErrorAction Stop	
	New-Item -ItemType Directory -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles\Global" `
		-Name "scenarios" -Force -ErrorAction Stop	
	New-Item -ItemType File -Path "$env:APPDATA\Powershell\FactorioProfiles\Profiles\Global" `
		-Name "blueprint-storage.dat" -Force -ErrorAction Stop	
}

# The "<was not built>" string will be replaced by the build script to "<was built>".
# However, the one with the (') quotes will not, which makes this logic work.
if ("<was not built>" -eq '<was not built>') {
	# This module is not the built package; it's part of the development tree.
	Write-Debug "Importing debug .dll"
	Import-Module "$script:ModuleRoot\bin\Debug\netstandard2.0\FactorioProfiles.dll"
}
else {
	# This module has been built.
	Write-Debug "Importing release .dll"
	Import-Module "$script:ModuleRoot\bin\FactorioProfiles.dll"
}

# DATA MIGRATION (Example)
# -----------------------
<#
Write-Debug "Checking for databse migration"
$databaseVersion = [Regex]::Match((Get-Item -Path "$Folder\database.*.xml" -ErrorAction Ignore), ".*?FactorioProfiles\\database.(.*).xml").Groups[1].Value
if ($databaseVersion -eq "0.1.0") {
	Write-Debug "`e[4mDetected database version 0.1.0!`e[0m"
	Rename-Item -Path "$Folder\database.0.1.0.xml" -NewName "database.0.2.0.xml" -Force -WhatIf:$false -Confirm:$false | Out-Null
	# Perform the actual migration of content.
}
#>