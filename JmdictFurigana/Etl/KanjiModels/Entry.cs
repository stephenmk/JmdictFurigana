using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class Entry
{
    public string Literal;
    public ReadingMeaning ReadingMeaning;
    public const string XmlTagName = "character";

    public async static Task<Entry> FromXmlAsync(XmlReader reader)
    {
        var entry = new Entry();
        string currentTagName = XmlTagName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTagName = reader.Name;
                if (currentTagName == ReadingMeaning.XmlTagName)
                {
                    entry.ReadingMeaning = await ReadingMeaning.FromXmlAsync(reader);
                }
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentTagName == "literal")
                {
                    entry.Literal = await reader.GetValueAsync();
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
        return entry;
    }
}