using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class Entry
{
    public string Literal;
    public ReadingMeaning ReadingMeaning;
    public static readonly string XmlElementName = "character";

    public async static Task<Entry> FromXmlReader(XmlReader reader)
    {
        var entry = new Entry();
        string currentElementName = XmlElementName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
                if (currentElementName == ReadingMeaning.XmlElementName)
                {
                    entry.ReadingMeaning = await ReadingMeaning.FromXmlReader(reader);
                }
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentElementName == "literal")
                {
                    entry.Literal = await reader.GetValueAsync();
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
        return entry;
    }
}