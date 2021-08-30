using System;
using System.Xml;
using System.Collections.Generic;

namespace FactorioProfiles
{
	class Data
	{
		// The path to the root folder where all data is stored.
		// ℹ This path starts at the %APPDATA% directory.
		public string appdataFolderPath = @"Powershell\FactorioProfiles";

		// The path to the database.
		// ℹ This path starts at the %APPDATA% directory.
		public static string databasePath = @"Powershell\FactorioProfiles\database.xml";

		// The path to the folder containing the "global" profile.
		// ℹ This path starts at the %APPDATA% directory.
		public static string globalProfilePath = @"Powershell\FactorioProfiles\Profiles\Global";

		// The default path to the folder containing all profiles.
		// ⚠ This should only be used by the 'OpenFile()' function to write an initial value to the database
		// file. If a database file exists, this value should be read from there. 
		private static string defaultProfileSavePath = @"%APPDATA%\Powershell\FactorioProfiles\Profiles";

		private static XmlDocument OpenFile()
		{
			var document = new XmlDocument();
			// Get the full expanded path. This method is arguably a better way than storing '%APPDATA%'
			// and then expanding that at runtime.
			var fullDatabasePath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
				databasePath);

			if (System.IO.File.Exists(fullDatabasePath))
			{
				// Load an existing configuration file if one exists.
				// TODO: Validate against a schema.
				document.Load(fullDatabasePath);
			}
			else
			{
				// Otherwise, create the configuration file DOM for the first time.
				var root = document.DocumentElement;

				// Create the XML Declaration Node (not strictly necessary).
				document.InsertBefore(document.CreateXmlDeclaration("1.0", "UTF-8", null), root);

				// Create a 'Data' body element to contain all of the data nodes.
				// Give it a version attribute, to allow for data migration if the data structure ever changes.
				var body = document.CreateElement("Data");
				body.SetAttribute("Version", "0.1.0");
				document.AppendChild(body);

				// Create a 'Config' element to contain configuration data which is related to the module
				// as a whole rather than any specific profile.
				var config = document.CreateElement("Config");

				// Create an element to store the path at which new profiles are created by default.
				var defaultPath = document.CreateElement("NewProfileSavePath");
				defaultPath.InnerText = defaultProfileSavePath;

				// Create an element to store the default global settings used for when a profile is created.
				var defaultSharing = document.CreateElement("NewProfileSharingSettings");
				var sharingConfig = document.CreateAttribute("Config");
				sharingConfig.Value = false.ToString();
				var sharingMods = document.CreateAttribute("Mods");
				sharingMods.Value = false.ToString();
				var sharingSaves = document.CreateAttribute("Saves");
				sharingSaves.Value = false.ToString();
				var sharingScenarios = document.CreateAttribute("Scenarios");
				sharingScenarios.Value = false.ToString();
				var sharingBlueprints = document.CreateAttribute("Blueprints");
				sharingBlueprints.Value = false.ToString();

				defaultSharing.Attributes.Append(sharingConfig);
				defaultSharing.Attributes.Append(sharingMods);
				defaultSharing.Attributes.Append(sharingSaves);
				defaultSharing.Attributes.Append(sharingScenarios);
				defaultSharing.Attributes.Append(sharingBlueprints);

				config.AppendChild(defaultPath);
				config.AppendChild(defaultSharing);
				body.AppendChild(config);

				// Create an element to contain all of the 'Profile' class data from serialization.
				var profiles = document.CreateElement("Profiles");
				body.AppendChild(profiles);
			}

			return document;
		}

		private static void CloseFile(XmlDocument document)
		{
			var fullDatabasePath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
				databasePath);

			document.Save(fullDatabasePath);
		}

		public static String GetGlobalProfilePath()
		{
			return System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
				globalProfilePath);
		}

		public static String GetNewProfileSavePath()
		{
			// If the database exists, this value will be read from that.
			// If the database doesn't yet exist, this will be the default initial value,
			// as defined by the 'defaultProfileSavePath' variable.
			var document = OpenFile();
			return document.DocumentElement.SelectSingleNode("/Data/Config/NewProfileSavePath").InnerText;
		}

		public static ShareSettings GetNewProfileSharingSettings()
		{
			return GetSharingSettings("/Data/Config/NewProfileSharingSettings");
		}

		private static ShareSettings GetSharingSettings(String rootNode)
		{
			// Load in the document.
			var document = OpenFile();
			var node = document.DocumentElement.SelectSingleNode(rootNode);

			return new ShareSettings(
				Convert.ToBoolean(node.Attributes.GetNamedItem("Config").Value),
				Convert.ToBoolean(node.Attributes.GetNamedItem("Mods").Value),
				Convert.ToBoolean(node.Attributes.GetNamedItem("Saves").Value),
				Convert.ToBoolean(node.Attributes.GetNamedItem("Scenarios").Value),
				Convert.ToBoolean(node.Attributes.GetNamedItem("Blueprints").Value)
			);
		}

		public static List<String> GetNames()
		{
			// Load in the document.
			var document = OpenFile();

			// Select all of the 'Name' nodes and add the values to the return list.
			var nodes = document.DocumentElement.SelectNodes("/Data/Profiles/Profile/Name");
			var list = new List<string>();
			foreach (XmlNode node in nodes)
			{
				list.Add(node.InnerText);
			}

			return list;
		}

		public static List<Profile> GetProfiles()
		{
			// Load in the document.
			var document = OpenFile();

			// Select all of the 'Profile' elements, iterate through them, and create a 'Profile' object.
			var nodes = document.DocumentElement.SelectNodes("/Data/Profiles/Profile");
			var list = new List<Profile>();
			foreach (XmlNode node in nodes)
			{
				// Create a new 'Profile', and populate its members from the node values.
				var name = node.SelectSingleNode("Name").InnerText;
				var profile = new Profile(
					name,
					node.SelectSingleNode("Path").InnerText,
					GetSharingSettings($"/Data/Profiles/Profile[Name = '{name}']/SharingSettings"));
				list.Add(profile);
			}

			return list;
		}

		public static Profile GetProfile(String name)
		{
			// Load in the document.
			var document = OpenFile();

			// Select the 'Profile' element with the matching name, and create a 'Profile' object.
			var nodes = document.DocumentElement.SelectNodes("/Data/Profiles/Profile");
			foreach (XmlNode node in nodes)
			{
				var nodeName = node.SelectSingleNode("Name").InnerText;
				// Check if the name in the current node matches the requested name.
				if (name == nodeName)
				{
					// Create a new 'Profile', and populate its members from the node values.
					var profile = new Profile(
						nodeName,
						node.SelectSingleNode("Path").InnerText,
						GetSharingSettings($"/Data/Profiles/Profile[Name = '{nodeName}']/SharingSettings"));

					return profile;
				}
			}

			return null;
		}

		public static void Add(Profile profile)
		{
			// Load in the document.
			var document = OpenFile();
			var root = document.DocumentElement;

			// Create all of the new XML Nodes required to store the data for the 'Profile' object.
			var newElementName = document.CreateElement("Name");
			newElementName.InnerText = profile.Name;
			var newElementPath = document.CreateElement("Path");
			newElementPath.InnerText = profile.Path;

			var newElementGlobal = document.CreateElement("SharingSettings");

			var newAttrConfig = document.CreateAttribute("Config");
			newAttrConfig.Value = profile.Settings.ShareConfig.ToString();
			var newAttrMods = document.CreateAttribute("Mods");
			newAttrMods.Value = profile.Settings.ShareMods.ToString();
			var newAttrSaves = document.CreateAttribute("Saves");
			newAttrSaves.Value = profile.Settings.ShareSaves.ToString();
			var newAttrScenarios = document.CreateAttribute("Scenarios");
			newAttrScenarios.Value = profile.Settings.ShareScenarios.ToString();
			var newAttrBlueprints = document.CreateAttribute("Blueprints");
			newAttrBlueprints.Value = profile.Settings.ShareBlueprints.ToString();

			newElementGlobal.Attributes.Append(newAttrConfig);
			newElementGlobal.Attributes.Append(newAttrMods);
			newElementGlobal.Attributes.Append(newAttrSaves);
			newElementGlobal.Attributes.Append(newAttrScenarios);
			newElementGlobal.Attributes.Append(newAttrBlueprints);

			var newElementProfile = document.CreateElement("Profile");
			newElementProfile.AppendChild(newElementName);
			newElementProfile.AppendChild(newElementPath);
			newElementProfile.AppendChild(newElementGlobal);

			// Add the new 'Profile' node to the container element.
			var node = root.SelectSingleNode("/Data/Profiles");
			node.AppendChild(newElementProfile);

			// Save the changes to disk.
			CloseFile(document);
		}

		public static void UpdateProfileName(Profile profile, String newName)
		{
			// Load in the document.
			var document = OpenFile();

			// Modify the 'Name' node for the correct 'Profile'.
			document.DocumentElement.SelectSingleNode(
				$"/Data/Profiles/Profile[Name = '{profile.Name}']/Name").InnerText = newName;

			// Save the changes to disk.
			CloseFile(document);
		}

		public static void UpdateProfilePath(Profile profile)
		{
			// Load in the document.
			var document = OpenFile();

			// Modify the 'Path' node for the correct 'Profile'.
			document.DocumentElement.SelectSingleNode(
				$"/Data/Profiles/Profile[Name = '{profile.Name}']/Path").InnerText = profile.Path;

			// Save the changes to disk.
			CloseFile(document);
		}

		public static void UpdateProfileSharingSettings(Profile profile)
		{
			// Load in the document.
			var document = OpenFile();

			// Modify the 'Global' node for the correct 'Profile'.
			var node = document.DocumentElement.SelectSingleNode(
				$"/Data/Profiles/Profile[Name = '{profile.Name}']/SharingSettings");

			node.Attributes.GetNamedItem("Config").Value = profile.Settings.ShareConfig.ToString();
			node.Attributes.GetNamedItem("Mods").Value = profile.Settings.ShareMods.ToString();
			node.Attributes.GetNamedItem("Saves").Value = profile.Settings.ShareSaves.ToString();
			node.Attributes.GetNamedItem("Scenarios").Value = profile.Settings.ShareScenarios.ToString();
			node.Attributes.GetNamedItem("Blueprints").Value = profile.Settings.ShareBlueprints.ToString();

			// Save the changes to disk.
			CloseFile(document);
		}

		public static void UpdateDefaultPathForNewProfiles(String newPath)
		{
			// Load in the document.
			var document = OpenFile();

			// Modify the 'DefaultPath' node.
			document.DocumentElement.SelectSingleNode(
				$"/Data/Config/NewProfileSavePath").InnerText = newPath;

			// Save the changes to disk.
			CloseFile(document);
		}

		public static void UpdateDefaultSharingSettings(ShareSettings newSettings)
		{
			// Load in the document.
			var document = OpenFile();

			// Modify the 'DefaultGlobal' node.
			var node = document.DocumentElement.SelectSingleNode(
				$"/Data/Config/NewProfileSharingSettings");

			node.Attributes.GetNamedItem("Config").Value = newSettings.ShareConfig.ToString();
			node.Attributes.GetNamedItem("Mods").Value = newSettings.ShareMods.ToString();
			node.Attributes.GetNamedItem("Saves").Value = newSettings.ShareSaves.ToString();
			node.Attributes.GetNamedItem("Scenarios").Value = newSettings.ShareScenarios.ToString();
			node.Attributes.GetNamedItem("Blueprints").Value = newSettings.ShareBlueprints.ToString();

			// Save the changes to disk.
			CloseFile(document);
		}

		public static void Remove(Profile profile)
		{
			// Load in the document.
			var document = OpenFile();

			// Find the 'Profile' node with the matching name, and delete it.
			var node = document.DocumentElement.SelectSingleNode(
				$"/Data/Profiles/Profile[Name = '{profile.Name}']");
			node.ParentNode.RemoveChild(node);

			// Save the changes to disk.
			CloseFile(document);
		}
	}
}