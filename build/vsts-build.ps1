<#
.SYNOPSIS
	Builds the source code into a module package.
	
.DESCRIPTION
	Compiles all the module source code into a single package. All code gets
	inserted into the main '.psm1' file.

.PARAMETER WorkingDirectory
	The root folder for the whole project, containing the git files, build
	files, module files, etc.
 [!]If running on Azure, don't specify any value.

.PARAMETER Repository
	The repository to publish to. Defauls to "PSGallery".
	The value "TESTING" will perform a test publish to the testing repository.

.PARAMETER ApiKey
	The key to publish to a repository.

.PARAMETER SkipPublish
	Don't publish the module to a repository.

.PARAMETER SkipArtifact
	Don't package the module into a zipped file.
	
.EXAMPLE
	PS C:\> .\build\vsts-build.ps1 -WorkingDirectory .\ -SkipPublish
	
	This is to just build and package the module locally.
	
.EXAMPLE
	PS C:\> .\build\vsts-build.ps1 -WorkingDirectory .\ -SkipArtifact
				-Repository 'TESTING' -ApiKey ...
	
	This is to build and package the module to the TESTING PSGallery. Use this
	for testing purposes.
	
.EXAMPLE
	PS C:\> .\build\vsts-build.ps1 -ApiKey ...
	
	This is to build and package the module to the real PSGallery, and to 
	package the module as a zip (for later use, in the azure task, for
	uploading to the Github Releases page).
	
.NOTES
	
#>
param
(
	[string]
	$ApiKey, 

	[string]
	[ValidateSet("PSGallery", "TESTING")]
	$Repository = "PSGallery",

	[string]
	$WorkingDirectory,

	[switch]
	$SkipPublish,
	
	[switch]
	$SkipArtifact
)

# Import helper functions.
. "$PSScriptRoot\vsts-helpers.ps1"

# Handle Working Directory paths within Azure pipelines.
if (-not $WorkingDirectory) {
	if ($env:RELEASE_PRIMARYARTIFACTSOURCEALIAS) {
		$WorkingDirectory = Join-Path -Path $env:SYSTEM_DEFAULTWORKINGDIRECTORY -ChildPath $env:RELEASE_PRIMARYARTIFACTSOURCEALIAS
	}
	else {
		$WorkingDirectory = $env:SYSTEM_DEFAULTWORKINGDIRECTORY
	}
}

# Make any error stop this build process.
$ErrorActionPreference = 'Stop'

# Import required modules.
Write-Header -Message "Importing PowershellGet Module" -Colour Cyan
Import-Module "PowershellGet" -RequiredVersion "2.2.2" -Verbose
# Print the loaded module information to make it easier for error diagnostics on the azure shell.
Get-Module -Verbose

# Create the publish folder.
Write-Header -Message "Creating publishing directory" -Colour Cyan
# Delete any potentially existing 'publish' folder.
if (Test-Path "$WorkingDirectory\publish") {
	Remove-Item -Path "$WorkingDirectory\publish" -Force -Recurse | Out-Null
}
$publishDir = New-Item -Path $WorkingDirectory -Name "publish" -ItemType Directory -Force -Verbose
New-Item -Path $publishDir.FullName -Name "FactorioProfiles" -ItemType Directory -Force | Out-Null
New-Item -Path "$($publishDir.FullName)\FactorioProfiles\bin" -ItemType Directory -Force | Out-Null

# Build the release configuration.
Write-Header -Message "Building the c# binary" -Colour Cyan
dotnet.exe build --configuration Release "$WorkingDirectory\FactorioProfiles"

# Copy the module files from the root git repository to the publish folder.
# This is mainly files such as the documentation, formatting files, module files, changelog.
Write-Header -Message "Copying over files for publishing" -Colour Cyan
Copy-Item -Path "$WorkingDirectory\FactorioProfiles\*" -Destination "$($publishDir.FullName)\FactorioProfiles\" `
	-Recurse -Force -Exclude "bin", "obj", "src", "tests", "FactorioProfiles.csproj" -Verbose

# Copy the release .dll file.
Copy-Item -Path "$WorkingDirectory\FactorioProfiles\bin\Release\netstandard2.0\FactorioProfiles.dll" `
	-Destination "$($publishDir.FullName)\FactorioProfiles\bin" -Force -Verbose

# Copy the .dll documentation file.
Copy-Item -Path "$WorkingDirectory\FactorioProfiles\bin\Release\netstandard2.0\FactorioProfiles.dll-Help.xml" `
	-Destination "$($publishDir.FullName)\FactorioProfiles\en-us" -Force -Verbose

# Modify a string in the .psm1 file to tell it that it is a packaged release, and to load the 
# binary .dll from the correct directory.
Write-Header -Message "Updating & Replacing text" -Colour Cyan
$fileData = Get-Content -Path "$($publishDir.FullName)\FactorioProfiles\FactorioProfiles.psm1" -Raw
$fileData = $fileData.Replace('"<was not built>"', '"<was built>"')
[System.IO.File]::WriteAllText("$($publishDir.FullName)\FactorioProfiles\FactorioProfiles.psm1", $fileData, [System.Text.Encoding]::UTF8)

# Insert the current year into the module manifest.
$fileData = Get-Content -Path "$($publishDir.FullName)\FactorioProfiles\FactorioProfiles.psd1" -Raw
$fileData = $fileData.Replace("__CURRENT_YEAR__", "$((Get-Date).Year)")
[System.IO.File]::WriteAllText("$($publishDir.FullName)\FactorioProfiles\FactorioProfiles.psd1", $fileData, [System.Text.Encoding]::UTF8)

if (-not $SkipPublish) {
	if ($Repository -eq "TESTING") {
		# Publish to the TESTING PSGallery.
		Write-Header -Message "Publishing to the TEST PSGallery" -Colour Green

		# Register testing repository.
		Register-PSRepository -Name "test-repo" -SourceLocation "https://www.poshtestgallery.com/api/v2" `
			-PublishLocation "https://www.poshtestgallery.com/api/v2/package" -InstallationPolicy Trusted `
			-Verbose -ErrorAction 'Continue'
		Publish-Module -Path "$($publishDir.FullName)\FactorioProfiles" -NuGetApiKey $ApiKey -Force `
			-Repository "test-repo" -Verbose
		
		Write-Header -Message "Waiting 60 seconds before testing download" -Colour Green
		Start-Sleep -Seconds 60
		
		# Uninstall the module if it already exists, to then test the installation of the module from the
		# test PSGallery.
		Write-Header -Message "Installing module from PSGallery" -Colour Green
		Uninstall-Module -Name "FactorioProfiles" -Force -Verbose -ErrorAction 'Continue'
		Install-Module -Name "FactorioProfiles" -Repository "test-repo" -Force `
			-AcceptLicense -SkipPublisherCheck -Verbose
		Write-Host "FactorioProfiles module installed." -ForegroundColor Green
		
		# Test if the module has been downloaded and imported correctly.
		Write-Header -Message "Importing module" -Colour Green
		Import-Module -Name "FactorioProfiles" -Verbose
		Get-Module -Verbose
		
		# Remove the testing repository.
		Unregister-PSRepository -Name "test-repo" -Verbose
	}
	else {
		# Publish to the real repository.
		Write-Header -Message "Publishing to $Repository" -Colour Green
		Publish-Module -Path "$($publishDir.FullName)\FactorioProfiles" -NuGetApiKey $ApiKey -Force `
			-Repository $Repository -Verbose
	}
}

if (-not $SkipArtifact) {
	# Get the module version number for file labelling.
	$moduleVersion = (Import-PowerShellDataFile -Path "$PSScriptRoot\..\FactorioProfiles\FactorioProfiles.psd1").ModuleVersion
	
	# Move the module contents to a version-labelled subfolder.
	Write-Header -Message "Creating Artifact. Moving content to version subfolder" -Colour Magenta
	New-Item -ItemType Directory -Path "$($publishDir.FullName)\FactorioProfiles\" -Name "$moduleVersion" -Force | Out-Null
	Move-Item -Path "$($publishDir.FullName)\FactorioProfiles\*" `
		-Destination "$($publishDir.FullName)\FactorioProfiles\$moduleVersion\" -Exclude "*$moduleVersion*" -Force -Verbose
	
	# Create a packaged zip file of the module folder.
	Write-Header -Message "Packaging module into archive" -Colour Magenta
	Compress-Archive -Path "$($publishDir.FullName)\FactorioProfiles" `
		-DestinationPath "$($publishDir.FullName)\FactorioProfiles-v$($moduleVersion).zip" -Verbose

	# Write out the module number as a azure pipeline variable for the later run publish task.
	Write-Host "##vso[task.setvariable variable=version;isOutput=true]$moduleVersion"
}
