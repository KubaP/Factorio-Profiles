using System;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Linq;

namespace FactorioProfiles
{
	/// <summary>
	/// <para type="synopsis">Activates the specified Factorio profile.</para>
	/// <para type="description">The `Switch-FactorioProfile` cmdlet switches to an existing Factorio profile, allowing you to launch the game with said profile.</para>
	/// <example>
	/// 	<code>PS C:\> Switch-FactorioProfile -Name "vanilla"</code>
	/// 	<para>Switches to the profile named "vanilla". The next time you launch the game this profile will be in use.</para>
	/// 	<para></para>
	/// 	<para></para>
	/// </example>
	/// <list type="alertSet">
	/// 	<item>
	/// 		<term></term>
	/// 		<description>
	/// 			<para>Usage</para>
	/// 			<para></para>
	/// 			<para>If this profile is sharing blueprints globally, once you finish and exit out of the game, you must run the `Sync-FactorioProfile` cmdlet. This is unfortunately a requirement due to the limitations of the game. For more information see the `about_FactorioProfiles` help page.</para>
	/// 		</description>
	/// 	</item>
	/// </list>
	/// <para type="link">about_FactorioProfiles</para>
	/// </summary>
	[Cmdlet(VerbsCommon.Switch, "FactorioProfile")]
	[OutputType(typeof(Profile))]
	public class SwitchFactorioProfile : PSCmdlet
	{
		/// <summary>
		/// <para type="description">Specifies the name of the existing profile to switch to.</para>
		/// <para type="description">[!] This value supports auto-completion.</para>
		/// </summary>
		[Parameter(Position = 0, Mandatory = true)]
		[ArgumentCompleter(typeof(NameCompleter))]
		[Alias("Profile")]
		public String Name { get; set; }

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

			// Validate that the profile exists.
			if (!Data.GetNames().Contains(Name, StringComparer.OrdinalIgnoreCase))
			{
				ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException($"There is no profile named '{Name}'!"),
						"1",
						ErrorCategory.InvalidData,
						null));
			}

			// Check if 'factorio.exe' is running, and if it is, present a prompt.
			var process = System.Diagnostics.Process.GetProcessesByName("factorio");
			if (process.Length > 0)
			{
				// The process is running, so prompt the user.
				var result = this.Host.UI.PromptForChoice(
					"\n",
					"Detected 'factorio.exe' process currently running. Do you want to:",
					new System.Collections.ObjectModel.Collection<ChoiceDescription> {
									new ChoiceDescription("&Switch anyway",
										"This will switch the profile. !!! WARNING: This may break the game or your save data. Proceed at own risk. !!!"),
									new ChoiceDescription("&Cancel",
										"This will stop the execution of this cmdlet.")},
					1);

				switch (result)
				{
					case 0:
						// Continue option, so just exit out of the switch statement.
						break;
					case 1:
						// Cancel option, so return out of the cmdlet, i.e. stop it.
						return;
					default:
						break;
				}
			}

			var profile = Data.GetProfile(Name);

			// Validate that the profile folder exists.
			if (!System.IO.Directory.Exists(profile.Path))
			{
				ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException($"The profile folder at '{profile.Path}' could not be located!"),
						"1",
						ErrorCategory.InvalidData,
						null));
			}

			// Switch to the profile.
			profile.Switch(this);

			WriteObject("\u001b[32mSuccessfully switched profiles\u001b[0m");
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}