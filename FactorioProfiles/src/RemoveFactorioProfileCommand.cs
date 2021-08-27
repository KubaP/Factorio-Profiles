using System;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	[Cmdlet(VerbsCommon.Remove, "FactorioProfile")]
	public class RemoveFactorioProfile : PSCmdlet
	{
		[Parameter(Position = 0, Mandatory = true)]
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

			// Warn if the profile folder doesn't exist.
			if (!System.IO.Directory.Exists(profile.Path))
			{
				WriteWarning($"The profile folder at '{profile.Path}' could not be located.");
			}

			// Delete the profile.
			Data.GetProfile(Name).Destroy(this);

			WriteObject("\u001b[32mSuccessfully removed the profile\u001b[0m");
		}

		// END Block - Runs at the end of this cmdlet.
		protected override void EndProcessing() { }
	}
}