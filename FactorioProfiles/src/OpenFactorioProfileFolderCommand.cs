using System;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Open, "FactorioProfileFolder", DefaultParameterSetName = "Profile")]
	[OutputType(typeof(Profile))]
	public class OpenFactorioProfileFolder : PSCmdlet
	{
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Profile")]
		[ArgumentCompleter(typeof(NameCompleter))]
		[Alias("Profile")]
		public String Name { get; set; }

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