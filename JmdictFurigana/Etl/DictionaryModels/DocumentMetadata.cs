using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public partial class DocumentMetadata
{
    public string Name;
    public Dictionary<string, string> EntityValueToName;

    public async static Task<DocumentMetadata> FromXmlAsync(XmlReader reader)
    {
        var dtd = await reader.GetValueAsync();
        var documentMetadata = new DocumentMetadata
        {
            Name = reader.Name,
            EntityValueToName = ParseEntityDeclarations(dtd),
        };
        return documentMetadata;
    }

    [GeneratedRegex(@"<!ENTITY\s+(\w+)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdRegex();

    private static Dictionary<string, string> ParseEntityDeclarations(string dtd)
    {
        var valueToName = new Dictionary<string, string>();
        foreach (Match match in DtdRegex().Matches(dtd))
        {
            var entityName = match.Groups[1].Value;
            var entityValue = match.Groups[2].Value;
            try
            {
                valueToName.Add(entityValue, entityName);
            }
            catch (System.ArgumentException)
            {
                // The "ik" name is reused with the same value.
                // As long as the Name/Value pair is the same,
                // don't throw an exception.
                if (valueToName[entityValue] != entityName)
                {
                    throw;
                }
            }
        }
        return valueToName;
    }
}
