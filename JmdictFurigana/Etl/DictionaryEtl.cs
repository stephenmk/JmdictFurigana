using JmdictFurigana.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using JmdictFurigana.Etl.DictionaryModels;


namespace JmdictFurigana.Etl;

/// <summary>
/// Parses the dictionary file and produces VocabEntry model instances.
/// </summary>
public class DictionaryEtl(string dictionaryFilePath)
{
    public string DictionaryFilePath { get; set; } = dictionaryFilePath;

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

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == Entry.XmlElementName)
                {
                    var entry = await Entry.FromXmlReader(reader);
                    foreach (var vocabEntry in VocabEntries(entry))
                    {
                        yield return vocabEntry;
                    }
                }
            }
        }
    }

    private static IEnumerable<VocabEntry> VocabEntries(Entry entry)
    {
        var kanjiFormTexts = entry.KanjiForms
            .Where(k => !k.IsHidden)
            .Select(k => k.Text);
        var readings = entry.Readings
            .Where(r => !r.NoKanji)
            .Where(r => !r.IsHidden);
        foreach (var reading in readings)
        {
            var readingText = reading.Text;
            var relevantKanjiFormTexts = reading.ConstraintKanjiFormTexts.Count > 0
                ? reading.ConstraintKanjiFormTexts
                : kanjiFormTexts;
            foreach (var kanjiFormText in relevantKanjiFormTexts)
            {
                yield return new VocabEntry
                {
                    KanjiReading = kanjiFormText,
                    KanaReading = readingText,
                };
            }
        }
    }
}
