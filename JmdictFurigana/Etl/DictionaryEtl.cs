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

    private class ReadingElement
    {
        public string Text;
        public List<string> InfoTags = [];
        public List<string> ConstraintKanjiForms = [];
        public bool NoKanji = false;
    }
    private class KanjiFormElement
    {
        public string Text;
        public List<string> InfoTags = [];
    }
    private class EntryElement
    {
        public List<KanjiFormElement> KanjiFormElements = [];
        public List<ReadingElement> ReadingElements = [];
    }

    public async IAsyncEnumerable<VocabEntry> ExecuteAsync()
    {
        await using var stream = File.OpenRead(DictionaryFilePath);

        var readerSettings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = long.MaxValue,
            MaxCharactersInDocument = long.MaxValue,
        };
        using var reader = XmlReader.Create(stream, readerSettings);

        EntryElement currentEntry = null;
        KanjiFormElement currentKanjiFormElement = null;
        ReadingElement currentReadingElement = null;
        string currentElementName = null;

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentElementName = reader.Name;
                if (currentElementName == "entry")
                    currentEntry = new EntryElement();
                else if (currentElementName == "k_ele")
                    currentKanjiFormElement = new KanjiFormElement();
                else if (currentElementName == "r_ele")
                    currentReadingElement = new ReadingElement();
                else if (currentElementName == "re_nokanji")
                    currentReadingElement.NoKanji = true;
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentElementName == "keb")
                    currentKanjiFormElement.Text = await reader.GetValueAsync();
                else if (currentElementName == "ke_inf")
                    currentKanjiFormElement.InfoTags.Add(await reader.GetValueAsync());
                else if (currentElementName == "reb")
                    currentReadingElement.Text = await reader.GetValueAsync();
                else if (currentElementName == "re_inf")
                    currentReadingElement.InfoTags.Add(await reader.GetValueAsync());
                else if (currentElementName == "re_restr")
                    currentReadingElement.ConstraintKanjiForms.Add(await reader.GetValueAsync());
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == "k_ele")
                {
                    currentEntry.KanjiFormElements.Add(currentKanjiFormElement);
                    currentKanjiFormElement = null;
                }
                else if (reader.Name == "r_ele")
                {
                    currentEntry.ReadingElements.Add(currentReadingElement);
                    currentReadingElement = null;
                }
                else if (reader.Name == "entry")
                {
                    foreach (var vocabEntry in VocabEntries(currentEntry))
                        yield return vocabEntry;
                    currentEntry = null;
                }
            }
        }
    }

    private static IEnumerable<VocabEntry> VocabEntries(EntryElement entryElement)
    {
        var kanjiForms = entryElement.KanjiFormElements
            .Where(k => !k.InfoTags.Any(i => i == "sK"))
            .Select(k => k.Text)
            .ToList();
        var readingElements = entryElement.ReadingElements
            .Where(r => !r.NoKanji)
            .Where(r => !r.InfoTags.Any(i => i == "sk"))
            .ToList();
        foreach (var readingElement in readingElements)
        {
            var relevantKanjiForms = readingElement.ConstraintKanjiForms.Count > 0
                ? readingElement.ConstraintKanjiForms
                : kanjiForms;
            foreach (var kanjiForm in relevantKanjiForms)
            {
                yield return new VocabEntry
                {
                    KanjiReading = kanjiForm,
                    KanaReading = readingElement.Text,
                };
            }
        }
    }

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
