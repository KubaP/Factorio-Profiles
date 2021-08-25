using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace FactorioProfiles
{
	class ShareSettings
	{
		// Each flag determines if this profile shares anything with the "global" profile.
		// This can be used, for example, to share blueprints between many profiles.
		public bool ShareConfig;
		public bool ShareMods;
		public bool ShareSaves;
		public bool ShareScenarios;
		public bool ShareBlueprints;

		// Default constructor. Initialises everything to false.
		public ShareSettings() {
			ShareConfig = false;
			ShareMods = false;
			ShareSaves = false;
			ShareScenarios = false;
			ShareBlueprints = false;
		}

		public ShareSettings(bool config, bool mods, bool saves, bool scenarios, bool blueprints)
		{
			ShareConfig = config;
			ShareMods = mods;
			ShareSaves = saves;
			ShareScenarios = scenarios;
			ShareBlueprints = blueprints;
		}

		// Copy constructor.
		public ShareSettings(ShareSettings settings)
		{
			ShareConfig = settings.ShareConfig;
			ShareMods = settings.ShareMods;
			ShareSaves = settings.ShareSaves;
			ShareScenarios = settings.ShareScenarios;
			ShareBlueprints = settings.ShareBlueprints;
		}
	}

	class Profile
	{
		// The unique name of this profile.
		public string Name;
		// The path at which this profile exists on disk.
		public string Path;
		// The sharing settings for this profile.
		public ShareSettings Settings;

		public Profile(String name, String path, ShareSettings settings)
		{
			Name = name;
			Path = path;
			Settings = settings;
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
			if (Settings.ShareConfig)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "config"),
					System.IO.Path.Combine(globalProfilePath, "config"),
					SymlinkType.Directory);
			}
			if (Settings.ShareMods)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "mods"),
					System.IO.Path.Combine(globalProfilePath, "mods"),
					SymlinkType.Directory);
			}
			if (Settings.ShareSaves)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "saves"),
					System.IO.Path.Combine(globalProfilePath, "saves"),
					SymlinkType.Directory);
			}
			if (Settings.ShareScenarios)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "scenarios"),
					System.IO.Path.Combine(globalProfilePath, "scenarios"),
					SymlinkType.Directory);
			}
			if (Settings.ShareBlueprints)
			{
				CreateSymbolicLink(
					System.IO.Path.Combine(Path, "blueprint-storage.dat"),
					System.IO.Path.Combine(globalProfilePath, "blueprint-storage.dat"),
					SymlinkType.File);
			}

			// Add this profile to the database.
			Data.Add(this);
		}

		public void Rename(String newName, Cmdlet cmdlet)
		{
			// Renaming the profile requires the renaming of the directory as well.
			// This is to avoid naming clashes. Only the profile name is checked on creation, so if an old
			// profile had a folder with said name, it would result in a clash. By renaming the folder, we
			// assure that any future profile will be able to create a folder with it's name.

			// Get the leaf of the path, i.e. the path of the parent folder containing the profile.
			var parentPath = new System.IO.DirectoryInfo(Path).Parent.ToString();
			// Then combine the parent directory with the new name of the profile.
			var newPath = System.IO.Path.Combine(parentPath, newName);

			MoveFolder(newPath, cmdlet);

			// Update the database. We pass the 'Profile' object first before changing it's name since
			// the database needs the old name for lookup. Only after the database has been edited, do we
			// change the name on the actual object.
			Data.UpdateProfileName(this, newName);
			Name = newName;
		}

		public void MoveFolder(String newPath, Cmdlet cmdlet)
		{
			// Try to rename the profile directory.
			try
			{
				System.IO.Directory.Move(Path, newPath);
			}
			catch (System.Exception e)
			{
				cmdlet.ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException(
							$"The profile folder located at '{Path}' could not be moved to '{newPath}'! Details:\n{e.Message}"),
							"1",
							ErrorCategory.InvalidOperation,
							null));
			}

			// Update the database.
			Path = newPath;
			Data.UpdateProfilePath(this);
		}

		public void UpdateSharingSettings(ShareSettings newSettings, Cmdlet cmdlet)
		{
			// Must check what sharing settings have changed, to appropriately modify any symlinks pointing
			// to the "global" profile.
			// If a sharing option has been enabled, the symlink must be created.
			// If a sharing option has been disabled, the symlink must be deleted.
			SharingSettingFolder(Settings.ShareConfig, newSettings.ShareConfig, "config", SymlinkType.Directory, cmdlet);
			SharingSettingFolder(Settings.ShareMods, newSettings.ShareMods, "mods", SymlinkType.Directory, cmdlet);
			SharingSettingFolder(Settings.ShareSaves, newSettings.ShareSaves, "saves", SymlinkType.Directory, cmdlet);
			SharingSettingFolder(Settings.ShareScenarios, newSettings.ShareScenarios, "scenarios", SymlinkType.Directory, cmdlet);
			SharingSettingFolder(Settings.ShareBlueprints, newSettings.ShareBlueprints, "blueprint-storage.dat", SymlinkType.File, cmdlet);

			// Update the database.
			Settings = newSettings;
			Data.UpdateProfileSharingSettings(this);
		}

		private void SharingSettingFolder(Boolean originalValue, Boolean newValue, String itemName, SymlinkType type, Cmdlet cmdlet)
		{
			// Get the full expanded path pointing to the "global" profile.
			var globalProfilePath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
				Data.globalProfilePath);

			// Get the path of the item being modified, i.e. a 'config' folder, a 'blueprint-storage.dat' file, etc.
			var itemPath = System.IO.Path.Combine(Path, itemName);

			if (originalValue && !newValue)
			{
				// Just delete symlink. Factorio will re-create any necessary folder or file afterwards.
				try
				{
					// Call the appropriate deletion method depending on the type of item.
					if (System.IO.File.Exists(itemPath))
					{
						System.IO.File.Delete(itemPath);
					}
					if (System.IO.Directory.Exists(itemPath))
					{
						System.IO.Directory.Delete(itemPath);
					}
				}
				catch (System.Exception e)
				{
					cmdlet.ThrowTerminatingError(
						new ErrorRecord(
							new PSInvalidOperationException(
								$"The '{itemName}' symbolic link located at '{itemPath}' could not be deleted! Details:\n{e.Message}"),
								"1",
								ErrorCategory.InvalidOperation,
								null));
				}
			}
			else if (!originalValue && newValue)
			{
				// First, delete the existing item.
				try
				{
					// Call the appropriate deletion method depending on the type of item.
					if (System.IO.File.Exists(itemPath))
					{
						System.IO.File.Delete(itemPath);
					}
					if (System.IO.Directory.Exists(itemPath))
					{
						System.IO.Directory.Delete(itemPath, true);
					}
				}
				catch (System.Exception e)
				{
					cmdlet.ThrowTerminatingError(
						new ErrorRecord(
							new PSInvalidOperationException(
								$"The '{itemName}' item located at '{itemPath}' could not be deleted to make room for a symbolic link! Details:\n{e.Message}"),
								"1",
								ErrorCategory.InvalidOperation,
								null));
				}
				// Then, create the symbolic link.
				CreateSymbolicLink(
					itemPath,
					System.IO.Path.Combine(globalProfilePath, itemName),
					type);
			}
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