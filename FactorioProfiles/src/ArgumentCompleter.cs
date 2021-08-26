using System.Collections.Generic;
using System.Management.Automation;
using System.Linq;

namespace FactorioProfiles
{
	public class NameCompleter : IArgumentCompleter
	{
		public IEnumerable<CompletionResult> CompleteArgument(
			string commandName,
			string parameterName,
			string wordToComplete,
			System.Management.Automation.Language.CommandAst commandAst,
			System.Collections.IDictionary fakeBoundParameters)
		{
			var names = Data.GetNames();

			// If there are no profiles, then return no completion result.
			if (names.Count == 0)
			{
				return new List<CompletionResult>();
			}
			else
			{
				// The search logic first strips any (") and (') characters from the 'wordToComplete'.
				// This is done since otherwise a wildcard search would not match anything, since the
				// actual names aren't surrounded by quotes.
				// After, it adds the (") marks back in again. This is so that 
				// the text is properly quoted to avoid any potential issues with powershell not parsing
				// it correctly as text. 
				var list = names
					.Where(
						new WildcardPattern(wordToComplete.Replace("'", "").Replace("\"", "") + "*", WildcardOptions.IgnoreCase).IsMatch)
					.Select(s => new CompletionResult($"\"{s}\"", s, CompletionResultType.ParameterValue, $"Profile named '{s}'"));
				return list;
			}
		}
	}
}