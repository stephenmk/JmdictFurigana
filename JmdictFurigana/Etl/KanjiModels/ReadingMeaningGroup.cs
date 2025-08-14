using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class ReadingMeaningGroup
{
    public List<Reading> Readings = [];
    public List<Meaning> Meanings = [];
    public static readonly string XmlElementName = "rmgroup";

    public async static Task<ReadingMeaningGroup> FromXmlReader(XmlReader reader)
    {
        var group = new ReadingMeaningGroup();
        string currentElementName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
                if (currentElementName == Reading.XmlElementName)
                {
                    var reading = await Reading.FromXmlReader(reader);
                    group.Readings.Add(reading);
                }
                else if (currentElementName == Meaning.XmlElementName)
                {
                    var meaning = await Meaning.FromXmlReader(reader);
                    group.Meanings.Add(meaning);
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
        return group;
    }
}
