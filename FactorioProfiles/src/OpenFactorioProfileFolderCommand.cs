using System;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Open, "FactorioProfileFolder")]
	[OutputType(typeof(Profile))]
	public class OpenFactorioProfileFolder : PSCmdlet
	{
		[Parameter(Position = 0)]
		[ArgumentCompleter(typeof(NameCompleter))]
		public String Name { get; set; }

		// BEGIN Block - Runs at the beginning of this cmdlet.
		protected override void BeginProcessing() { }

		// PROCESS Block - Runs once for each pipeline input.
		protected override void ProcessRecord()
		{
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

			// Open 'explorer.exe' at the profile folder location.
			System.Diagnostics.Process.Start("explorer.exe", $"{profile.Path}");
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}