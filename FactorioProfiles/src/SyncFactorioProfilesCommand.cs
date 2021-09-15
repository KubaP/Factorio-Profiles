using System;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	[Cmdlet(VerbsData.Sync, "FactorioProfiles")]
	[OutputType(typeof(Profile))]
	public class SyncFactorioProfiles : PSCmdlet
	{

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
			// Get the currently active profile name.
			var name = Data.GetActiveProfile();

			// Validate that the profile exists.
			if (!Data.GetNames().Contains(name, StringComparer.OrdinalIgnoreCase))
			{
				ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException($"The currently active profile on record is '{name}' but it does not exist!"),
						"1",
						ErrorCategory.InvalidData,
						null));
			}

			var profile = Data.GetProfile(name);

			profile.Sync(this);

			WriteObject("\u001b[32mSuccessfully synced profiles\u001b[0m");
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}