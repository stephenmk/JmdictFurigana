using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class ReadingMeaning
{
    public List<ReadingMeaningGroup> Groups = [];
    public List<string> Nanori = [];
    public const string XmlTagName = "reading_meaning";

    public async static Task<ReadingMeaning> FromXmlAsync(XmlReader reader)
    {
        var readingMeaning = new ReadingMeaning();
        string currentTagName = XmlTagName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTagName = reader.Name;
                if (currentTagName == "rmgroup")
                {
                    var group = await ReadingMeaningGroup.FromXmlAsync(reader);
                    readingMeaning.Groups.Add(group);
                }
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentTagName == "nanori")
                {
                    var text = await reader.GetValueAsync();
                    readingMeaning.Nanori.Add(text);
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
        return readingMeaning;
    }
}
