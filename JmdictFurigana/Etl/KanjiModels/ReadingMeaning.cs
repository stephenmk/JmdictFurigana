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
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    if (currentTagName == "rmgroup")
                    {
                        var group = await ReadingMeaningGroup.FromXmlAsync(reader);
                        readingMeaning.Groups.Add(group);
                    }
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == "nanori")
                    {
                        var text = await reader.GetValueAsync();
                        readingMeaning.Nanori.Add(text);
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return readingMeaning;
    }
}
