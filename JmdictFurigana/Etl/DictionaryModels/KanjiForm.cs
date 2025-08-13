using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public class KanjiForm
{
    public string Text;
    public List<string> InfoTags = [];

    public async static Task<KanjiForm> FromXmlReader(XmlReader reader)
    {
        var kanjiForm = new KanjiForm();
        string currentElementName = null;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                var text = await reader.GetValueAsync();
                if (currentElementName == "keb")
                    kanjiForm.Text = text;
                else if (currentElementName == "ke_inf")
                    kanjiForm.InfoTags.Add(text);
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == "k_ele")
                {
                    break;
                }
            }
        }
        return kanjiForm;
    }
}
