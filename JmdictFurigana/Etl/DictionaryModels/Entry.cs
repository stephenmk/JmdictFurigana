using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.DictionaryModels;

public class Entry
{
    public List<KanjiForm> KanjiForms = [];
    public List<Reading> Readings = [];
    public const string XmlTagName = "entry";

    public async static Task<Entry> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        var entry = new Entry();
        var exit = false;
        string currentTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    await ProcessElementAsync(reader, docMeta, currentTagName, entry);
                    break;
                case XmlNodeType.EndElement:
                    if (reader.Name == XmlTagName)
                    {
                        exit = true;
                    }
                    break;
            }
        }
        return entry;
    }

    private async static Task ProcessElementAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, Entry entry)
    {
        switch (tagName)
        {
            case KanjiForm.XmlTagName:
                var kanjiForm = await KanjiForm.FromXmlAsync(reader, docMeta);
                entry.KanjiForms.Add(kanjiForm);
                break;
            case Reading.XmlTagName:
                var reading = await Reading.FromXmlAsync(reader, docMeta);
                entry.Readings.Add(reading);
                break;
        }
    }
}
