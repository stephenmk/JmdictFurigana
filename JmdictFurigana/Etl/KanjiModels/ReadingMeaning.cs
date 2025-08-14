using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class ReadingMeaning
{
    public List<ReadingMeaningGroup> Groups = [];
    public List<string> Nanori = [];
    public static readonly string XmlElementName = "reading_meaning";

    public async static Task<ReadingMeaning> FromXmlReader(XmlReader reader)
    {
        var readingMeaning = new ReadingMeaning();
        string currentElementName = XmlElementName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
                if (currentElementName == "rmgroup")
                {
                    var group = await ReadingMeaningGroup.FromXmlReader(reader);
                    readingMeaning.Groups.Add(group);
                }
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentElementName == "nanori")
                {
                    var text = await reader.GetValueAsync();
                    readingMeaning.Nanori.Add(text);
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
        return readingMeaning;
    }
}
