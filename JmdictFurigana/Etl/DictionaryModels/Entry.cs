using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public class Entry
{
    public List<KanjiForm> KanjiForms = [];
    public List<Reading> Readings = [];

    public async static Task<Entry> FromXmlReader(XmlReader reader)
    {
        var entry = new Entry();
        string currentElementName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
                if (currentElementName == "k_ele")
                {
                    var kanjiForm = await KanjiForm.FromXmlReader(reader);
                    entry.KanjiForms.Add(kanjiForm);
                }
                else if (currentElementName == "r_ele")
                {
                    var reading = await Reading.FromXmlReader(reader);
                    entry.Readings.Add(reading);
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == "entry")
                {
                    break;
                }
            }
        }
        return entry;
    }
}