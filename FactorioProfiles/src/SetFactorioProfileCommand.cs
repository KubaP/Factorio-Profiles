using System;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Linq;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Set, "FactorioProfileOption")]
	[OutputType(typeof(Profile))]
	public class SetFactorioProfileOption : PSCmdlet
	{
		// Parameters for editing a profile.
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Profile")]
		[Alias("Name")]
		public String Profile { get; set; }

		[Parameter(Position = 1, ParameterSetName = "Profile")]
		[AllowNull]
		public String NewName { get; set; }

		[Parameter(Position = 1, ParameterSetName = "Profile")]
		[AllowNull]
		public String NewPath { get; set; }

		[Parameter(Position = 1, ParameterSetName = "Profile")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareConfig { get; set; }

		[Parameter(Position = 1, ParameterSetName = "Profile")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareMods { get; set; }

		[Parameter(Position = 1, ParameterSetName = "Profile")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareSaves { get; set; }

		[Parameter(Position = 1, ParameterSetName = "Profile")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareScenarios { get; set; }

		[Parameter(Position = 1, ParameterSetName = "Profile")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareBlueprints { get; set; }

		// Parameters for editing the general settings.
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "General")]
		public SwitchParameter GeneralSetting { get; set; }

		[Parameter(Position = 1, ParameterSetName = "General")]
		[AllowNull]
		public String DefaultPathForNewProfiles { get; set; }

		[Parameter(Position = 1, ParameterSetName = "General")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareConfigByDefault { get; set; }

		[Parameter(Position = 1, ParameterSetName = "General")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareModsByDefault { get; set; }

		[Parameter(Position = 1, ParameterSetName = "General")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareSavesByDefault { get; set; }

		[Parameter(Position = 1, ParameterSetName = "General")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareScenariosByDefault { get; set; }

		[Parameter(Position = 1, ParameterSetName = "General")]
		[ValidateSet("true", "false")]
		[AllowNull]
		public String ShareBlueprintsByDefault { get; set; }

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
			// Different logic depending on whether a specific profile is being modified, or general settings.
			switch (ParameterSetName)
			{
				case "Profile":
					// Validate that a profile with the given name exists.
					if (!Data.GetNames().Contains(Profile, StringComparer.OrdinalIgnoreCase))
					{
						ThrowTerminatingError(
							new ErrorRecord(
								new PSArgumentException($"There is no profile with the name '{Profile}'!"),
								"1",
								ErrorCategory.InvalidArgument,
								null));
					}

					var profile = Data.GetProfile(Profile);

					if (!String.IsNullOrWhiteSpace(NewName))
					{
						// Renaming the profile to a new name.

						// Validate that the new name isn't taken.
						while (Data.GetNames().Contains(NewName, StringComparer.OrdinalIgnoreCase))
						{
							var result = this.Host.UI.PromptForChoice(
								"\n",
								$"The name '{NewName}' is already taken. Do you want to:",
								new System.Collections.ObjectModel.Collection<ChoiceDescription> {
									new ChoiceDescription("&Provide a new name",
										"This will request a new name as a string input."),
									new ChoiceDescription("&Overwrite the existing profile",
										"This will delete the existing profile and rename the current one to the new name."),
									new ChoiceDescription("&Cancel",
										"This will stop the execution of this cmdlet.")},
								2);

							switch (result)
							{
								case 0:
									// New name option, so ask for a new string.
									var res = this.Host.UI.Prompt(
										"\n",
										$"Please enter a new name which is not '{NewName}'",
										new System.Collections.ObjectModel.Collection<FieldDescription> {
											new FieldDescription("New Name")
											});

									// Retrieve the inputted value.
									PSObject input;
									res.TryGetValue("New Name", out input);
									// I'm not sure if this could fail somehow, but just in case, do a sanity check.
									// If for some reason 'input' is null, the 'Name' won't be changed and the loop
									// will re-run once again.
									if (input != null)
									{
										NewName = input.ToString();
									}

									break;
								case 1:
									// Overwrite option, so delete the existing profile.
									Data.GetProfile(NewName).Destroy(this);

									break;
								case 2:
									// Cancel option, so return out of the cmdlet, i.e. stop it.
									return;
								default:
									break;
							}
						}

						// Rename the profile.
						profile.Rename(NewName, this);
					}

					if (!String.IsNullOrWhiteSpace(NewPath))
					{
						// Moving the location of the profile to a new path.

						// Ensure that the path isn't already taken.
						// TODO: Make this a prompt instead.
						while (System.IO.Directory.Exists(NewPath))
						{
							var result = this.Host.UI.PromptForChoice(
								"\n",
								$"The path '{NewPath}' already exists. Do you want to:",
								new System.Collections.ObjectModel.Collection<ChoiceDescription> {
									new ChoiceDescription("&Provide a new path",
										"This will request a new path as a string input."),
									new ChoiceDescription("&Overwrite the item at the path",
										"This will delete the existing folder/file and move the profile to the new path."),
									new ChoiceDescription("&Cancel",
										"This will stop the execution of this cmdlet.")},
								2);

							switch (result)
							{
								case 0:
									// New path option, so ask for a new string.
									var res = this.Host.UI.Prompt(
										"\n",
										$"Please enter a new path which is not '{NewPath}'",
										new System.Collections.ObjectModel.Collection<FieldDescription> {
											new FieldDescription("New Path")
											});

									// Retrieve the inputted value.
									PSObject input;
									res.TryGetValue("New Path", out input);
									// I'm not sure if this could fail somehow, but just in case, do a sanity check.
									// If for some reason 'input' is null, the 'Name' won't be changed and the loop
									// will re-run once again.
									if (input != null)
									{
										NewPath = input.ToString();
									}

									break;
								case 1:
									// Overwrite option, so delete the existing item at the path.
									try
									{
										// Call the appropriate method depending on if file or folder.
										if (System.IO.Directory.Exists(NewPath))
										{
											System.IO.Directory.Delete(NewPath, true);
										}
										if (System.IO.File.Exists(NewPath))
										{
											System.IO.File.Delete(NewPath);
										}
									}
									catch (System.Exception e)
									{
										ThrowTerminatingError(
											new ErrorRecord(
												new PSInvalidOperationException(
													$"The folder/file located at '{NewPath}' could not be deleted! Details:\n{e.Message}"),
													"1",
													ErrorCategory.InvalidOperation,
													null));
										throw;
									}

									break;
								case 2:
									// Cancel option, so return out of the cmdlet, i.e. stop it.
									return;
								default:
									break;
							}
						}

						// Move the profile.
						profile.MoveFolder(NewPath, this);
					}

					var modifiedSettings = false;
					var sharingSettings = new ShareSettings(profile.Settings);
					if (!String.IsNullOrWhiteSpace(ShareConfig))
					{
						sharingSettings.ShareConfig = Convert.ToBoolean(ShareConfig);
					}
					if (!String.IsNullOrWhiteSpace(ShareMods))
					{
						sharingSettings.ShareMods = Convert.ToBoolean(ShareMods);
					}
					if (!String.IsNullOrWhiteSpace(ShareSaves))
					{
						sharingSettings.ShareSaves = Convert.ToBoolean(ShareSaves);
					}
					if (!String.IsNullOrWhiteSpace(ShareScenarios))
					{
						sharingSettings.ShareScenarios = Convert.ToBoolean(ShareScenarios);
					}
					if (!String.IsNullOrWhiteSpace(ShareBlueprints))
					{
						sharingSettings.ShareBlueprints = Convert.ToBoolean(ShareBlueprints);
					}

					if (modifiedSettings)
					{
						profile.UpdateSharingSettings(sharingSettings, this);
					}

					WriteObject("\u001b[32mSuccessfully modified the profile\u001b[0m");

					break;
				case "General":

					if (!String.IsNullOrWhiteSpace(DefaultPathForNewProfiles))
					{
						// TODO: Check if the path exists, and if not, put a prompt warning.
						Data.UpdateDefaultPathForNewProfiles(DefaultPathForNewProfiles);
					}

					var defaultSharingSettings = Data.GetDefaultSharingSettings();
					if (!String.IsNullOrWhiteSpace(ShareConfigByDefault))
					{
						defaultSharingSettings.ShareConfig = Convert.ToBoolean(ShareConfigByDefault);
					}
					if (!String.IsNullOrWhiteSpace(ShareModsByDefault))
					{
						defaultSharingSettings.ShareMods = Convert.ToBoolean(ShareModsByDefault);
					}
					if (!String.IsNullOrWhiteSpace(ShareSavesByDefault))
					{
						defaultSharingSettings.ShareSaves = Convert.ToBoolean(ShareSavesByDefault);
					}
					if (!String.IsNullOrWhiteSpace(ShareScenariosByDefault))
					{
						defaultSharingSettings.ShareScenarios = Convert.ToBoolean(ShareScenariosByDefault);
					}
					if (!String.IsNullOrWhiteSpace(ShareBlueprintsByDefault))
					{
						defaultSharingSettings.ShareBlueprints = Convert.ToBoolean(ShareBlueprintsByDefault);
					}
					Data.UpdateDefaultSharingSettings(defaultSharingSettings);

					WriteObject("\u001b[32mSuccessfully modified the general settings\u001b[0m");

					break;
				default:
					ThrowTerminatingError(
						new ErrorRecord(
							new PSArgumentException("The current parameter set has fallen into an invalid set! This should never happen!"),
							"1",
							ErrorCategory.InvalidArgument,
							null));
					break;
			}
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}