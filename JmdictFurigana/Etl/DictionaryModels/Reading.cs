using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public class Reading
{
    public string Text;
    public List<string> InfoTags = [];
    public List<string> ConstraintKanjiFormTexts = [];
    public bool NoKanji = false;

    public async static Task<Reading> FromXmlReader(XmlReader reader)
    {
        var reading = new Reading();
        string currentElementName = null;
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
                    reading.InfoTags.Add(text);
                }
                else if (currentElementName == "re_restr")
                {
                    reading.ConstraintKanjiFormTexts.Add(text);
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == "r_ele")
                {
                    break;
                }
            }
        }
        return reading;
    }
}
