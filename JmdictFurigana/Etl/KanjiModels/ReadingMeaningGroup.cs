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
        var exit = false;
        string currentTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    await ProcessElementAsync(reader, currentTagName, group);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async static Task ProcessElementAsync(XmlReader reader, string tagName, ReadingMeaningGroup group)
    {
        switch (tagName)
        {
            case Reading.XmlTagName:
                var reading = await Reading.FromXmlAsync(reader);
                group.Readings.Add(reading);
                break;
            case Meaning.XmlTagName:
                var meaning = await Meaning.FromXmlAsync(reader);
                group.Meanings.Add(meaning);
                break;
        }
    }
}
