using System;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Switch, "FactorioProfile")]
	[OutputType(typeof(Profile))]
	public class SwitchFactorioProfile : PSCmdlet
	{
		[Parameter(Position = 0)]
		[ArgumentCompleter(typeof(NameCompleter))]
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