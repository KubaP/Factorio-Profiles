@{
	# Script module or binary module file associated with this manifest
	RootModule = 'FactorioProfiles.psm1'
	
	# Version number of this module.
	ModuleVersion = '0.1.0'
	
	# ID used to uniquely identify this module
	GUID = '41c85188-07ea-4949-bfb8-510b5cdcdf72'
	
	# Author of this module
	Author = 'KubaP'
	
	# Company or vendor of this module
	CompanyName = ' '
	
	# Copyright statement for this module
	Copyright = 'Copyright (c) 2021 KubaP'
	
	# Description of the functionality provided by this module
	Description = 'A set of commands for easily managing & switching between different factorio profiles, (sets of saves, mods, configurations, etc).'
	
	# Minimum version of the Windows PowerShell engine required by this module
	PowerShellVersion = '6.0'
	
	# Modules that must be imported into the global environment prior to importing
	# this module
	<#!
	RequiredModules = @(
		@{ ModuleName='name'; ModuleVersion='1.0.0' }
	)#>

	# Assemblies that are part of this module.
	#NestedModules = @('bin\Debug\netstandard2.0\FactorioProfiles.dll')
	
	# Assemblies that must be loaded prior to importing this module
	# RequiredAssemblies = @('bin\FactorioProfiles.dll')
	
	# Type files (.ps1xml) to be loaded when importing this module
	# TypesToProcess = @('xml\FactorioProfiles.Types.ps1xml')
	
	# Format files (.ps1xml) to be loaded when importing this module
	FormatsToProcess = @('xml\FactorioProfiles.Format.ps1xml')
	
	# Functions to export from this module
	FunctionsToExport = @(
	)
	
	# Cmdlets to export from this module
	CmdletsToExport = @(
		'New-FactorioProfile',
		'Get-FactorioProfile',
		'Set-FactorioProfileOption',
		'Remove-FactorioProfile',
		'Switch-FactorioProfile'
	)
	
	# Variables to export from this module
	VariablesToExport = ''
	
	# Aliases to export from this module
	AliasesToExport = ''
	
	# List of all modules packaged with this module
	ModuleList = @()
	
	# List of all files packaged with this module
	FileList = @()
	
	# Private data to pass to the module specified in ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
	PrivateData = @{
		
		#Support for PowerShellGet galleries.
		PSData = @{
			
			# Tags applied to this module. These help with module discovery in online galleries.
			Tags = @("Windows", "PSEdition_Core")
			
			# A URL to the license for this module.
			LicenseUri = 'https://www.gnu.org/licenses/gpl-3.0.en.html'
			
			# A URL to the main website for this project.
			ProjectUri = 'https://github.com/KubaP/Factorio-Profiles'
			
			# A URL to an icon representing this module.
			# IconUri = ''
			
			# ReleaseNotes of this module
			ReleaseNotes = 'https://github.com/KubaP/Factorio-Profiles/blob/master/FactorioProfiles/changelog.md'
			
		} # End of PSData hashtable
		
	} # End of PrivateData hashtable
}