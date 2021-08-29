using System;

namespace FactorioProfiles
{
	class Sanitise
	{
		// Sanitise a string by removing characters which cannot be part of a name of a folder or file.
		public static String FolderName(String path)
		{
			// Remove any illegal characters for a name of a folder.
			return path.Replace("\\", "")
				.Replace("/", "")
				.Replace("<", "")
				.Replace(">", "")
				.Replace(":", "")
				.Replace("?", "")
				.Replace("|", "")
				.Replace("*", "")
				.Replace("\"", "");
		}
	}
}