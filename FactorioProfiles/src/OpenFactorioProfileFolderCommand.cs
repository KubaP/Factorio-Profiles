using System;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	/// <summary>
	/// <para type="synopsis">Opens the file explorer at the specified Factorio profile folder.</para>
	/// <para type="description">The `Open-FactorioProfileFolder` cmdlet opens the file explorer at the location of an existing Factorio profile, allowing you to modify the files manually.</para>
	/// <example>
	/// 	<code>PS C:\> Open-FactorioProfileFolder -Name "vanilla"</code>
	/// 	<para>Opens the file explorer at the location of the path of the profile named "vanilla", e.g. '%APPDATA%\Powershell\FactorioProfiles\Profiles\vanilla'.</para>
	/// 	<para></para>
	/// 	<para></para>
	/// </example>
	/// <example>
	/// 	<code>PS C:\> Open-FactorioProfileFolder -GlobalProfile</code>
	/// 	<para>Opens the file explorer at the location of the path of the global profile; this is the profile in which data is shared to/from other profiles. It is located at '%APPDATA%\Powershell\FactorioProfiles\Profiles\Global'.</para>
	/// 	<para></para>
	/// 	<para></para>
	/// </example>
	/// <para type="link">about_FactorioProfiles</para>
	/// </summary>
	[Cmdlet(VerbsCommon.Open, "FactorioProfileFolder", DefaultParameterSetName = "Profile")]
	[OutputType(typeof(Profile))]
	public class OpenFactorioProfileFolder : PSCmdlet
	{
		/// <summary>
		/// <para type="description">Specifies the name of the existing profile to open.</para>
		/// <para type="description">[!] This value supports auto-completion.</para>
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Profile")]
		[ArgumentCompleter(typeof(NameCompleter))]
		[Alias("Profile")]
		public String Name { get; set; }

		/// <summary>
		/// <para type="description">Specifies to open the "global" profile.</para>
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Global")]
		public SwitchParameter GlobalProfile { get; set; }

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
			String path = "";

			switch (ParameterSetName)
			{
				case "Profile":
					// Validate that a profile with the given name exists.
					if (!Data.GetNames().Contains(Name, StringComparer.OrdinalIgnoreCase))
					{
						ThrowTerminatingError(
							new ErrorRecord(
								new PSArgumentException($"There is no profile with the name '{Name}'!"),
								"1",
								ErrorCategory.InvalidArgument,
								null));
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

					path = profile.Path;
					break;
				case "Global":
					// Get the "global" profile path.
					path = Data.GetGlobalProfilePath();
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

			// Open 'explorer.exe' at the profile folder location.
			System.Diagnostics.Process.Start("explorer.exe", $"{path}");
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}