using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public class KanjiForm
{
    public string Text;
    public List<string> InfoTags = [];
    public bool IsHidden => InfoTags.Any(tag => tag == "sK");
    public const string XmlTagName = "k_ele";

    public async static Task<KanjiForm> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        var kanjiForm = new KanjiForm();
        string currentTagName = XmlTagName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTagName = reader.Name;
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                var text = await reader.GetValueAsync();
                if (currentTagName == "keb")
                {
                    kanjiForm.Text = text;
                }
                else if (currentTagName == "ke_inf")
                {
                    var tag = docMeta.EntityValueToName[text];
                    kanjiForm.InfoTags.Add(tag);
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
        return kanjiForm;
    }
}
