using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class ReadingMeaningGroup
{
    public List<Reading> Readings = [];
    public List<Meaning> Meanings = [];
    public const string XmlTagName = "rmgroup";

    public async static Task<ReadingMeaningGroup> FromXmlAsync(XmlReader reader)
    {
        var group = new ReadingMeaningGroup();
        string currentTagName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTagName = reader.Name;
                if (currentTagName == Reading.XmlTagName)
                {
                    var reading = await Reading.FromXmlAsync(reader);
                    group.Readings.Add(reading);
                }
                else if (currentTagName == Meaning.XmlTagName)
                {
                    var meaning = await Meaning.FromXmlAsync(reader);
                    group.Meanings.Add(meaning);
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
        return group;
    }
}
