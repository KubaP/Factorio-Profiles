using System;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Runtime.InteropServices;

namespace FactorioProfiles
{
	/// <summary>
	/// <para type="description">Contains information about a Factorio profiles sharing settings.</para>
	/// </summary>
	public class ShareSettings
	{
		// Each flag determines if this profile shares anything with the "global" profile.
		// This can be used, for example, to share blueprints between many profiles.
		public bool ShareConfig;
		public bool ShareMods;
		public bool ShareSaves;
		public bool ShareScenarios;
		public bool ShareBlueprints;

		// Default constructor. Initialises everything to false.
		public ShareSettings()
		{
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

	/// <summary>
	/// <para type="description">Contains information about a Factorio profile.</para>
	/// </summary>
	public class Profile
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

			// Create any symlinks to the "global" profile depending on what settings are enabled.
			ShareSettingsFolder(false, Settings.ShareConfig, "config", cmdlet);
			ShareSettingsFolder(false, Settings.ShareMods, "mods", cmdlet);
			ShareSettingsFolder(false, Settings.ShareSaves, "saves", cmdlet);
			ShareSettingsFolder(false, Settings.ShareScenarios, "scenarios", cmdlet);

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
			// Just like in the 'New-FactorioProfile' cmdlet, sanitise the name of the profile.
			// Since the name will make up the name of a folder, it cannot contain certain characters.
			var newPath = System.IO.Path.Combine(parentPath, Sanitise.FolderName(newName));

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
			ShareSettingsFolder(Settings.ShareConfig, newSettings.ShareConfig, "config", cmdlet);
			ShareSettingsFolder(Settings.ShareMods, newSettings.ShareMods, "mods", cmdlet);
			ShareSettingsFolder(Settings.ShareSaves, newSettings.ShareSaves, "saves", cmdlet);
			ShareSettingsFolder(Settings.ShareScenarios, newSettings.ShareScenarios, "scenarios", cmdlet);

			// Update the database.
			Settings = newSettings;
			Data.UpdateProfileSharingSettings(this);
		}

		private void ShareSettingsFolder(Boolean originalValue, Boolean newValue, String itemName, Cmdlet cmdlet)
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
				try
				{
					CreateSymbolicLink(
						itemPath,
						System.IO.Path.Combine(globalProfilePath, itemName),
						SymlinkType.Directory);
				}
				catch (System.Exception e)
				{
					cmdlet.ThrowTerminatingError(
						new ErrorRecord(
							new PSInvalidOperationException(
								$"The symbolic link at '{itemPath}' pointing to '{System.IO.Path.Combine(globalProfilePath, itemName)}' could not be created! Details:\n{e.Message}"),
								"1",
								ErrorCategory.InvalidOperation,
								null));
				}
			}
		}

		public void Switch(PSCmdlet cmdlet)
		{
			// Switch to this profile, by symlinking the appdata factorio folder to the location of this profile.
			var factorioAppdataPath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
				"Factorio");

			if (System.IO.Directory.Exists(factorioAppdataPath))
			{
				// Check if the folder is real, i.e. not a symlink.
				var dirInfo = new System.IO.FileInfo(factorioAppdataPath);
				if (!dirInfo.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint))
				{
					// There already is something there, so ask the user for confirmation.
					var result = cmdlet.Host.UI.PromptForChoice(
						"\n",
						$"Detected an existing factorio profile at '%APPDATA%\\Factorio'. Do you want to:",
						new System.Collections.ObjectModel.Collection<ChoiceDescription> {
							new ChoiceDescription("&Go ahead anyway with switching",
								"This will delete any content in the existing profile folder."),
							new ChoiceDescription("&Cancel",
								"This will stop the execution of this cmdlet.\nYou can then create a new profile and move the contents over.")},
						1);

					switch (result)
					{
						case 0:
							// Force continue option, so delete the existing folder.
							try
							{
								System.IO.Directory.Delete(factorioAppdataPath, true);
							}
							catch (System.Exception e)
							{
								cmdlet.ThrowTerminatingError(
									new ErrorRecord(
										new PSInvalidOperationException(
											$"The folder located at '%APPDATA%\\Factorio' could not be deleted! Details:\n{e.Message}"),
											"1",
											ErrorCategory.InvalidOperation,
											null));
							}
							break;
						case 1:
							// Cancel option, so return out of the cmdlet, i.e. stop it.
							return;
						default:
							break;
					}
				}
				else
				{
					// There is already a symlink, so just delete it to make room.
					System.IO.Directory.Delete(factorioAppdataPath);
				}
			}

			// Create the symlink.
			try
			{
				CreateSymbolicLink(factorioAppdataPath, Path, SymlinkType.Directory);
			}
			catch (System.Exception e)
			{
				cmdlet.ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException(
							$"The symbolic link at '{factorioAppdataPath}' pointing to '{Path}' could not be created! Details:\n{e.Message}"),
							"1",
							ErrorCategory.InvalidOperation,
							null));
			}

			// Update the active profile in the database.
			Data.SetActiveProfile(this);

			// If this profile is sharing blueprints, copy over the "global" blueprint file.
			if (this.Settings.ShareBlueprints)
			{
				// Get the full expanded path pointing to the "global" profile.
				var globalBlueprintPath = System.IO.Path.Combine(
					System.IO.Path.Combine(
						System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
						Data.globalProfilePath),
					"blueprint-storage.dat");

				// Get the path of the item being modified, i.e. a 'config' folder, a 'blueprint-storage.dat' file, etc.
				var blueprintPath = System.IO.Path.Combine(Path, "blueprint-storage.dat");

				// Validate that there is a global profile blueprint file to copy over.
				if (!System.IO.File.Exists(globalBlueprintPath))
				{
					cmdlet.ThrowTerminatingError(
						new ErrorRecord(
							new PSInvalidOperationException(
								$"This profile is sharing blueprints globally, but the global profile does not have a blueprint file!"),
								"1",
								ErrorCategory.InvalidOperation,
								null));
				}

				// If the blueprint file already exists, delete it first otherwise the copy operation would be blocked.
				if (System.IO.File.Exists(blueprintPath))
				{
					try
					{
						System.IO.File.Delete(blueprintPath);
					}
					catch (System.Exception e)
					{
						cmdlet.ThrowTerminatingError(
							new ErrorRecord(
								new PSInvalidOperationException(
									$"Could not delete the blueprint file at '{blueprintPath}'! Details:\n{e.Message}"),
									"1",
									ErrorCategory.InvalidOperation,
									null));
					}
				}

				// Copy over the global profile blueprint file.
				System.IO.File.Copy(globalBlueprintPath, blueprintPath, true);
			}
		}

		public void Sync(Cmdlet cmdlet)
		{
			// The blueprint file may need syncing through a manual copy operation, unlike everything else which
			// is just symlinked. See 'ARCHITECTURE.md' for details.

			// Check if this profile even needs to sync.
			if (!this.Settings.ShareBlueprints)
			{
				return;
			}

			// Get the full expanded path pointing to the "global" profile.
			var globalBlueprintPath = System.IO.Path.Combine(
				System.IO.Path.Combine(
					System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
					Data.globalProfilePath),
				"blueprint-storage.dat");

			// Get the path of the item being modified, i.e. a 'config' folder, a 'blueprint-storage.dat' file, etc.
			var blueprintPath = System.IO.Path.Combine(Path, "blueprint-storage.dat");

			// Validate that there is a profile blueprint file to copy over.
			if (!System.IO.File.Exists(blueprintPath))
			{
				cmdlet.ThrowTerminatingError(
					new ErrorRecord(
						new PSInvalidOperationException(
							$"This profile is sharing blueprints but it does not have a blueprint file!"),
							"1",
							ErrorCategory.InvalidOperation,
							null));
			}

			// If the blueprint file already exists, delete it first otherwise the copy operation would be blocked.
			if (System.IO.File.Exists(globalBlueprintPath))
			{
				try
				{
					System.IO.File.Delete(globalBlueprintPath);
				}
				catch (System.Exception e)
				{
					cmdlet.ThrowTerminatingError(
						new ErrorRecord(
							new PSInvalidOperationException(
								$"Could not delete the blueprint file at '{globalBlueprintPath}'! Details:\n{e.Message}"),
								"1",
								ErrorCategory.InvalidOperation,
								null));
				}
			}

			// Copy over the global profile blueprint file.
			System.IO.File.Copy(blueprintPath, globalBlueprintPath, true);
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