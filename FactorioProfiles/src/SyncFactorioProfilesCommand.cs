using System;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	/// <summary>
	/// <para type="synopsis">Syncs the specified Factorio profile.</para>
	/// <para type="description">The `Sync-FactorioProfile` cmdlet syncs the blueprint data from the last active Factorio profile to other profiles which share blueprint data.</para>
	/// <example>
	/// 	<code>PS C:\> Sync-FactorioProfile</code>
	/// 	<para>Syncs the last active profile, whatever that may be.</para>
	/// 	<para></para>
	/// 	<para></para>
	/// </example>
	/// <list type="alertSet">
	/// 	<item>
	/// 		<term></term>
	/// 		<description>
	/// 			<para>Usage</para>
	/// 			<para></para>
	/// 			<para>If a profile is sharing blueprints globally, this cmdlet must be run once you finish and exit out of the game. This is unfortunately a requirement due to the limitations of the game. For more information see the `about_FactorioProfiles` help page.</para>
	/// 		</description>
	/// 	</item>
	/// </list>
	/// <para type="link">about_FactorioProfiles</para>
	/// </summary>
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