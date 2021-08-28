using System;
using System.Management.Automation;
using System.Linq;
using System.Management.Automation.Host;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.New, "FactorioProfile")]
	[OutputType(typeof(Profile))]
	public class NewFactorioProfile : PSCmdlet
	{
		[Parameter(Position = 0, Mandatory = true)]
		public String Name { get; set; }

		[Parameter(Position = 1)]
		[AllowNull]
		[ValidateSet("true", "false")]
		public String ShareConfig { get; set; }

		[Parameter(Position = 2)]
		[AllowNull]
		[ValidateSet("true", "false")]
		public String ShareMods { get; set; }

		[Parameter(Position = 3)]
		[AllowNull]
		[ValidateSet("true", "false")]
		public String ShareSaves { get; set; }

		[Parameter(Position = 4)]
		[AllowNull]
		[ValidateSet("true", "false")]
		public String ShareScenarios { get; set; }

		[Parameter(Position = 5)]
		[AllowNull]
		[ValidateSet("true", "false")]
		public String ShareBlueprints { get; set; }

		[Parameter(Position = 6)]
		public String Path { get; set; }

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
			// Validate that the name isn't empty.
			if (String.IsNullOrWhiteSpace(Name))
			{
				ThrowTerminatingError(
					new ErrorRecord(
						new PSArgumentException("The name cannot be blank or empty!"),
						"1",
						ErrorCategory.InvalidArgument,
						null));
			}

			// Validate that the name isn't already taken. This loop will run until there is no name clash.
			while (Data.GetNames().Contains(Name, StringComparer.OrdinalIgnoreCase))
			{
				var result = this.Host.UI.PromptForChoice(
					"\n",
					$"The name '{Name}' is already taken. Do you want to:",
					new System.Collections.ObjectModel.Collection<ChoiceDescription> {
						new ChoiceDescription("&Provide a new name",
							"This will request a new name as a string input."),
						new ChoiceDescription("&Overwrite the existing profile",
							"This will delete the existing profile to create this new one."),
						new ChoiceDescription("&Cancel",
							"This will stop the execution of this cmdlet.")},
					2);

				switch (result)
				{
					case 0:
						// New name option, so ask for a new string.
						var res = this.Host.UI.Prompt(
							"\n",
							$"Please enter a new name which is not '{Name}'",
							new System.Collections.ObjectModel.Collection<FieldDescription> {
								new FieldDescription("Name")
							}
						);
						// Retrieve the inputted value.
						PSObject newName;
						res.TryGetValue("Name", out newName);
						// I'm not sure if this could fail somehow, but just in case, do a sanity check.
						// If for some reason 'newName' is null, the 'Name' won't be changed and the loop
						// will re-run once again.
						if (newName != null)
						{
							Name = newName.ToString();
						}

						break;
					case 1:
						// Overwrite option, so delete the existing profile.
						Data.GetProfile(Name).Destroy(this);
						break;
					case 2:
						// Cancel option, so return out of the cmdlet, i.e. stop it.
						return;
					default:
						break;
				}
			}

			// If a path is specified, then use that. Otherwise, get the current default save path.
			// ℹ If a path is specified, there is no need to check if the parent directory exists,
			// since the 'CreateDirectory()' method will create any missing subdirectories in the process.
			if (String.IsNullOrWhiteSpace(Path))
			{
				Path = Data.GetNewProfileSavePath();
				// Append the name of the profile to the path. This is only done for the default case.
				// If the user has specified a path, the leaf is the folder containing the profile contents.
				// Sanitise the name of the profile, since it will be used as the *name* of the folder,
				// and folder/file names have a few illegal characters.
				Path = System.IO.Path.Combine(Path, Sanitise.FolderName(Name));
			}

			// Expand any environment variables.
			Path = System.Environment.ExpandEnvironmentVariables(Path);

			// Ensure that the path isn't already taken.
			if (System.IO.Directory.Exists(Path))
			{
				ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException($"The desired profile path '{Path}' already exists!"),
						"1",
						ErrorCategory.InvalidOperation,
						null));
			}

			// Get the default settings for a profile, and then overwrite any of them if 
			// they have been specified in this cmdlet invocation.
			var settings = Data.GetNewProfileSharingSettings();
			if (!String.IsNullOrWhiteSpace(ShareConfig))
			{
				settings.ShareConfig = Convert.ToBoolean(ShareConfig);
			}
			if (!String.IsNullOrWhiteSpace(ShareMods))
			{
				settings.ShareMods = Convert.ToBoolean(ShareMods);
			}
			if (!String.IsNullOrWhiteSpace(ShareSaves))
			{
				settings.ShareSaves = Convert.ToBoolean(ShareSaves);
			}
			if (!String.IsNullOrWhiteSpace(ShareScenarios))
			{
				settings.ShareScenarios = Convert.ToBoolean(ShareScenarios);
			}
			if (!String.IsNullOrWhiteSpace(ShareBlueprints))
			{
				settings.ShareBlueprints = Convert.ToBoolean(ShareBlueprints);
			}

			// Create the new 'Profile' object, instantiate any necessary folders, and add it to the database.
			Profile profile = new Profile(Name, Path, settings);
			profile.Create(this);

			WriteObject("\u001b[32mSuccessfully created a new profile\u001b[0m");
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}