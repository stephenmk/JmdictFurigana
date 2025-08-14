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
    public static readonly string XmlElementName = "r_ele";

    public async static Task<Reading> FromXmlReader(XmlReader reader, DocumentMetadata docMeta)
    {
        var reading = new Reading();
        string currentElementName = XmlElementName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
                if (currentElementName == "re_nokanji")
                {
                    reading.NoKanji = true;
                }
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                var text = await reader.GetValueAsync();
                if (currentElementName == "reb")
                {
                    reading.Text = text;
                }
                else if (currentElementName == "re_inf")
                {
                    var tag = docMeta.EntityValueToName[text];
                    reading.InfoTags.Add(tag);
                }
                else if (currentElementName == "re_restr")
                {
                    reading.ConstraintKanjiFormTexts.Add(text);
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == XmlElementName)
                {
                    break;
                }
            }
        }
        return reading;
    }
}
