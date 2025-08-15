using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class Meaning
{
    public string Text;
    public string Language;
    public const string XmlTagName = "meaning";

    public async static Task<Meaning> FromXmlAsync(XmlReader reader)
    {
        var meaning = new Meaning()
        {
            Language = reader.GetAttribute("m_lang") ?? "en"
        };

        string currentTagName = XmlTagName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTagName = reader.Name;
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentTagName == XmlTagName)
                {
                    meaning.Text = await reader.GetValueAsync();
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
        return meaning;
    }
}
