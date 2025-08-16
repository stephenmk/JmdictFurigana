using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public class Reading
{
    public string Text;
    public List<string> InfoTags = [];
    public List<string> ConstraintKanjiFormTexts = [];
    public bool NoKanji = false;
    public bool IsHidden => InfoTags.Any(tag => tag == "sk");
    public const string XmlTagName = "r_ele";

    public async static Task<Reading> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        var reading = new Reading();
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    ProcessElement(currentTagName, reading);
                    break;
                case XmlNodeType.Text:
                    await ProcessTextAsync(reader, docMeta, currentTagName, reading);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return reading;
    }

    private static void ProcessElement(string tagName, Reading reading)
    {
        switch (tagName)
        {
            case "re_nokanji":
                reading.NoKanji = true;
                break;
                // Potentially more cases here later.
        }
    }

    private async static Task ProcessTextAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, Reading reading)
    {
        switch (tagName)
        {
            case "reb":
                reading.Text = await reader.GetValueAsync();
                break;
            case "re_inf":
                var infoValue = await reader.GetValueAsync();
                var infoName = docMeta.EntityValueToName[infoValue];
                reading.InfoTags.Add(infoName);
                break;
            case "re_restr":
                var kanjiFormText = await reader.GetValueAsync();
                reading.ConstraintKanjiFormTexts.Add(kanjiFormText);
                break;
        }
    }
}
