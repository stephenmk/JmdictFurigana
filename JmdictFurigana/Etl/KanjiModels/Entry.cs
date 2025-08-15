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
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    if (currentTagName == ReadingMeaning.XmlTagName)
                    {
                        entry.ReadingMeaning = await ReadingMeaning.FromXmlAsync(reader);
                    }
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == "literal")
                    {
                        entry.Literal = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return entry;
    }
}