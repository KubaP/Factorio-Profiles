using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace FactorioProfiles
{
	class Profile
	{
		// The unique name of this profile.
		public string Name;
		// The path at which this profile exists on disk.
		public string Path;

		// Each flag determines if this profile shares anything with the "global" profile.
		// This can be used, for example, to share blueprints between many profiles.
		public bool UseGlobalConfig;
		public bool UseGlobalMods;
		public bool UseGlobalSaves;
		public bool UseGlobalScenarios;
		public bool UseGlobalBlueprints;

		public Profile(String name, String path, Dictionary<String, Boolean> globalSettings)
		{
			Name = name;
			Path = path;
			// Set the config values from the dictionary.
			UseGlobalConfig = globalSettings["Config"];
			UseGlobalMods = globalSettings["Mods"];
			UseGlobalSaves = globalSettings["Saves"];
			UseGlobalScenarios = globalSettings["Scenarios"];
			UseGlobalBlueprints = globalSettings["Blueprints"];
		}

		public void Create(Cmdlet cmdlet)
		{
			// Create the profile directory.
			try
			{
				System.IO.Directory.CreateDirectory(Path);
			}
			catch (System.Exception e)
			{
				cmdlet.ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException(
							$"The profile folder located at '{Path}' could not be created! Details:\n{e.Message}"),
						"1",
						ErrorCategory.InvalidOperation,
						null));
			}

			// Get the full expanded path pointing to the "global" profile.
			var globalProfilePath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
				Data.globalProfilePath);

			// Create any symlinks to the "global" profile depending on what settings are enabled.
			if (UseGlobalConfig)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "config"),
					System.IO.Path.Combine(globalProfilePath, "config"),
					SymlinkType.Directory);
			}
			if (UseGlobalMods)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "mods"),
					System.IO.Path.Combine(globalProfilePath, "mods"),
					SymlinkType.Directory);
			}
			if (UseGlobalSaves)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "saves"),
					System.IO.Path.Combine(globalProfilePath, "saves"),
					SymlinkType.Directory);
			}
			if (UseGlobalScenarios)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "scenarios"),
					System.IO.Path.Combine(globalProfilePath, "scenarios"),
					SymlinkType.Directory);
			}
			if (UseGlobalBlueprints)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "blueprint-storage.dat"),
					System.IO.Path.Combine(globalProfilePath, "blueprint-storage.dat"),
					SymlinkType.File);
			}

			// Add this profile to the database.
			Data.Add(this);
		}

		public void Destroy(Cmdlet cmdlet)
		{
			// Delete the profile directory.
			try
			{
				System.IO.Directory.Delete(Path, true);
			}
			catch (System.Exception e)
			{
				cmdlet.ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException(
							$"The profile folder located at '{Path}' could not be deleted! Details:\n{e.Message}"),
						"1",
						ErrorCategory.InvalidOperation,
						null));
			}

			// Remove this profile from the database.
			Data.Remove(this);
		}

		// FFI necessary to create a symbolic link. C# has no native support for this.
		// lpSymlinkFileName - The path at which to create the symlink; this includes the name of the symlink.
		// lpTargetFileName - The path which will be the target of the symlink.
		// dwFlags - Whether the symlink is a file or directory type. ⚠ If this doesn't match, the behaviour
		//  will be incorrect and explorer behaves weirdly.
		[DllImport("kernel32.dll")]
		static extern bool CreateSymbolicLink(
			string lpSymlinkFileName,
			string lpTargetFileName,
			SymlinkType dwFlags);
		enum SymlinkType
		{
			File = 0,
			Directory = 1,
		}
	}
}