using System;
using System.Management.Automation;
using System.Collections.Generic;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Get, "FactorioProfile")]
	[OutputType(typeof(Profile))]
	public class GetFactorioProfile : PSCmdlet
	{
		[Parameter(Position = 0)]
		[ArgumentCompleter(typeof(NameCompleter))]
		[Alias("Profile", "Name", "Names")]
		public String[] Profiles { get; set; }

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
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
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}