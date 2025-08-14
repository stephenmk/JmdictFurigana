using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class Meaning
{
    public string Text;
    public string Language;
    public static readonly string XmlElementName = "meaning";

    public async static Task<Meaning> FromXmlReader(XmlReader reader)
    {
        var meaning = new Meaning()
        {
            Language = reader.GetAttribute("m_lang") ?? "en"
        };

        string currentElementName = XmlElementName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentElementName == XmlElementName)
                {
                    meaning.Text = await reader.GetValueAsync();
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
        return meaning;
    }
}
