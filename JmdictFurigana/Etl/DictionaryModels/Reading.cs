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
        string currentTagName = XmlTagName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTagName = reader.Name;
                if (currentTagName == "re_nokanji")
                {
                    reading.NoKanji = true;
                }
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                var text = await reader.GetValueAsync();
                if (currentTagName == "reb")
                {
                    reading.Text = text;
                }
                else if (currentTagName == "re_inf")
                {
                    var tag = docMeta.EntityValueToName[text];
                    reading.InfoTags.Add(tag);
                }
                else if (currentTagName == "re_restr")
                {
                    reading.ConstraintKanjiFormTexts.Add(text);
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == XmlTagName)
                {
                    break;
                }
            }
        }
        return reading;
    }
}
