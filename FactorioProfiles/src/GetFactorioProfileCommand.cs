using System;
using System.Management.Automation;
using System.Collections.Generic;

namespace FactorioProfiles
{
	/// <summary>
	/// <para type="synopsis">Retrieves the specified Factorio profile.</para>
	/// <para type="description">The `Get-FactorioProfile` cmdlet retrieves an existing Factorio profile and output it to the screen. By default, the output is presented in List view.</para>
	/// <example>
	/// 	<code>PS C:\> Get-FactorioProfile -Name "vanilla"</code>
	/// 	<para>Retrieves the profile named "vanilla" and prints it.</para>
	/// 	<para></para>
	/// 	<para></para>
	/// </example>
	/// <list type="alertSet">
	/// 	<item>
	/// 		<term></term>
	/// 		<description>
	/// 			<para>Change settings</para>
	/// 			<para></para>
	/// 			<para>To change any of the profile settings, run the `Set-FactorioProfile` cmdlet with the `-Profile` switch.</para>
	/// 		</description>
	/// 	</item>
	/// </list>
	/// <para type="link">about_FactorioProfiles</para>
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "FactorioProfile")]
	[OutputType(typeof(Profile))]
	public class GetFactorioProfile : PSCmdlet
	{
		/// <summary>
		/// <para type="description">Specifies the name of the existing profile to retrieve.</para>
		/// <para type="description">[!] This value supports auto-completion.</para>
		/// </summary>
		[Parameter(Position = 0)]
		[ArgumentCompleter(typeof(NameCompleter))]
		[Alias("Name", "Profile", "Profiles")]
		public String[] Names { get; set; }

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
			var profiles = new List<Profile>();

			// If no name is provided, display all profiles.
			if (Names == null)
			{
				profiles = Data.GetProfiles();
			}
			else
			{
				// Iterate through all the given in names, and try to retrieve the corresponding profile.
				foreach (String name in Names)
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