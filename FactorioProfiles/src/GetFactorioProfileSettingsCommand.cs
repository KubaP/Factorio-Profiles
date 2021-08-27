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
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}