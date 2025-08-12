using JmdictFurigana.Models;
using JmdictFurigana.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace JmdictFurigana.Etl;

/// <summary>
/// Parses the dictionary file and produces VocabEntry model instances.
/// </summary>
public class DictionaryEtl(string dictionaryFilePath)
{
    public string DictionaryFilePath { get; set; } = dictionaryFilePath;

    /// <summary>
    /// Parses the dictionary file and returns entries.
    /// </summary>
    public IEnumerable<VocabEntry> Execute()
    {
        var xDoc = LoadXmlDocument();
        foreach (var xEntry in xDoc.Root.Elements("entry"))
        {
            var kanjiForms = KanjiForms(xEntry);
            if (kanjiForms.Count == 0)
                continue;
            foreach (var xReadingElement in xEntry.Elements("r_ele"))
            {
                if (xReadingElement.HasElement("re_nokanji"))
                    continue;
                if (xReadingElement.Elements("re_inf").Any(i => i.Value == "sk"))
                    continue;

                var constraintForms = xReadingElement.Elements("re_restr")
                    .Select(k => k.Value)
                    .ToList();

                var relevantKanjiForms = constraintForms.Count > 0 ? constraintForms : kanjiForms;
                var reading = xReadingElement.Element("reb").Value;

                foreach (var kanjiForm in relevantKanjiForms)
                {
                    yield return new VocabEntry
                    {
                        KanjiReading = kanjiForm,
                        KanaReading = reading,
                    };
                }
            }
        }
    }

    private XDocument LoadXmlDocument()
    {
        var xmlFileTextContent = File.ReadAllText(DictionaryFilePath);
        var bytes = Encoding.UTF8.GetBytes(xmlFileTextContent);
        using var stream = new MemoryStream(bytes);
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = long.MaxValue,
            MaxCharactersInDocument = long.MaxValue,
        };
        using var reader = XmlReader.Create(stream, settings);
        var doc = XDocument.Load(reader);
        return doc;
    }

    private static List<string> KanjiForms(XElement entry)
    {
        return entry.Elements("k_ele")
            .Where(k => !k.Elements("ke_inf").Any(i => i.Value == "sK"))
            .Select(k => k.Element("keb").Value)
            .ToList();
    }
}
