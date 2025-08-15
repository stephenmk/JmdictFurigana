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
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == XmlTagName)
                    {
                        meaning.Text = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return meaning;
    }
}
