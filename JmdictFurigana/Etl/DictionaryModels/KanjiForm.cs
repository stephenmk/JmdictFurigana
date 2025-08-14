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
    public static readonly string XmlElementName = "k_ele";

    public async static Task<KanjiForm> FromXmlReader(XmlReader reader, DocumentMetadata docMeta)
    {
        var kanjiForm = new KanjiForm();
        string currentElementName = XmlElementName;
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
                {
                    kanjiForm.Text = text;
                }
                else if (currentElementName == "ke_inf")
                {
                    var tag = docMeta.EntityValueToName[text];
                    kanjiForm.InfoTags.Add(tag);
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
        return kanjiForm;
    }
}
