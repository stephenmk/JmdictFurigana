using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public class Entry
{
    public List<KanjiForm> KanjiForms = [];
    public List<Reading> Readings = [];
    public static readonly string XmlElementName = "entry";

    public async static Task<Entry> FromXmlReader(XmlReader reader, DocumentMetadata docMeta)
    {
        var entry = new Entry();
        string currentElementName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
                if (currentElementName == KanjiForm.XmlElementName)
                {
                    var kanjiForm = await KanjiForm.FromXmlReader(reader, docMeta);
                    entry.KanjiForms.Add(kanjiForm);
                }
                else if (currentElementName == Reading.XmlElementName)
                {
                    var reading = await Reading.FromXmlReader(reader, docMeta);
                    entry.Readings.Add(reading);
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