using System;
using System.Management.Automation;
using System.Collections.Generic;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Get, "FactorioProfile", DefaultParameterSetName = "Profile")]
	[OutputType(typeof(Profile))]
	public class GetFactorioProfile : PSCmdlet
	{
		[Parameter(Position = 0, ParameterSetName = "Profile")]
		[ArgumentCompleter(typeof(NameCompleter))]
		[Alias("Profile", "Name", "Names")]
		public String[] Profiles { get; set; }

		[Parameter(Position = 0, ParameterSetName = "General")]
		public SwitchParameter GeneralSettings { get; set; }

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
			switch (ParameterSetName)
			{
				case "Profile":
					var profiles = new List<Profile>();

					// If no name is provided, display all profiles.
					if (Profiles == null)
					{
						profiles = Data.GetProfiles();
					}
					else
					{
						// Iterate through all the given in names, and try to retrieve the corresponding profile.
						foreach (String name in Profiles)
						{
							var profile = Data.GetProfile(name);
							// If the profile doesn't exist, warn the user.
							if (profile == null)
							{
								WriteWarning($"There is no profile named '{name}'.");
								continue;
							}

							profiles.Add(profile);
						}
					}

					// Sort the list by alphabetical order of the profile names.
					profiles.Sort((x, y) => String.Compare(x.Name, y.Name));
					WriteObject(profiles);

					break;
				case "General":
					// Display the general settings information.
					WriteObject($"Path for new profiles: {Data.GetNewProfileSavePath()}");
					var settings = Data.GetNewProfileSharingSettings();
					var str = "";
					if (settings.ShareConfig)
					{
						str += "Config, ";
					}
					if (settings.ShareMods)
					{
						str += "Mods, ";
					}
					if (settings.ShareSaves)
					{
						str += "Saves, ";
					}
					if (settings.ShareScenarios)
					{
						str += "Scenarios, ";
					}
					if (settings.ShareBlueprints)
					{
						str += "Blueprints, ";
					}
					str = str.Remove(str.Length - 2, 2);
					WriteObject($"Sharing settings for new profiles:\n\t{str}");

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