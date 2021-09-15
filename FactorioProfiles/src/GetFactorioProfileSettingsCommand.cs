using System.Management.Automation;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Get, "FactorioProfileSettings")]
	public class GetFactorioProfileSettings : PSCmdlet
	{
		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
			// Display the general settings information.
			WriteObject($"Path for new profiles: {Data.GetNewProfileSavePath()}");

			var settings = Data.GetNewProfileSharingSettings();
			var str = "\n\t";
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
			if (str.Length > 2)
			{
				// If something is being shared, remove the trailing comma.
				str = str.Remove(str.Length - 2, 2);
			}
			else
			{
				// If nothing is being shared, write N/A.
				str = "N/A";
			}
			WriteObject($"Sharing settings for new profiles: {str}");

			WriteObject($"Currently active profile: {Data.GetActiveProfile()}");
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}