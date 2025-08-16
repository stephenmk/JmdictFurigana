using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using JmdictFurigana.Etl.KanjiModels;
using JmdictFurigana.Helpers;
using JmdictFurigana.Models;

namespace JmdictFurigana.Etl;

/// <summary>
/// Parses the kanji data file and produces instances of the Kanji model.
/// </summary>
public class KanjiEtl
{
    /// <summary>
    /// Reads and returns Kanji models.
    /// </summary>
    public async static IAsyncEnumerable<Kanji> ExecuteAsync(string kanjidicPath)
    {
        var supplementaryKanjis = await LoadSupplementaryKanjisAsync(PathHelper.SupplementaryKanjiPath);

        await foreach (var entry in LoadKanjidicEntriesAsync(kanjidicPath))
        {
            var kanji = new Kanji
            {
                Character = entry.Literal.First(), // TODO: Many truncations occurring here. Problem?
                Readings = entry.ReadingMeaning?
                    .Groups.FirstOrDefault()?  // Currently no entry has more than one.
                    .Readings
                    .Where(r => r.IsJapanese)
                    .Select(r => KanaHelper.ToHiragana(r.Text))
                    .ToList() ?? []
            };

            var supp = supplementaryKanjis.FirstOrDefault(k => k.Character == kanji.Character);
            if (supp != null)
            {
                // Merge the supplementary kanji info and remove it from the supplement list.
                kanji.Readings.AddRange(supp.Readings);
                supplementaryKanjis.Remove(supp);
            }

            var nanoriReadings = entry.ReadingMeaning?.Nanori ?? [];
            kanji.ReadingsWithNanori = kanji.Readings.Union(nanoriReadings).Distinct().ToList();

            yield return kanji;
        }

        // Return the remaining supplementary kanji as new kanji.
        foreach (var kanji in supplementaryKanjis)
        {
            yield return kanji;
        }
    }

    private async static Task<List<Kanji>> LoadSupplementaryKanjisAsync(string path)
    {
        var supplementaryKanjis = new List<Kanji>();
        await foreach (var line in File.ReadLinesAsync(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.First() == ';')
                continue;

            char c = line.First();
            var split = line.Split(SeparatorHelper.FileFieldSeparator);
            var readings = split[1].Split(SeparatorHelper.FileReadingSeparator);

            var kanji = new Kanji()
            {
                Character = c,
                Readings = [.. readings],
                ReadingsWithNanori = [.. readings],
                IsRealKanji = false,
            };
            supplementaryKanjis.Add(kanji);
        }
        return supplementaryKanjis;
    }

    private async static IAsyncEnumerable<Entry> LoadKanjidicEntriesAsync(string path)
    {
        await using var stream = File.OpenRead(path);

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
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name == Entry.XmlTagName)
                    {
                        var entry = await Entry.FromXmlAsync(reader);
                        yield return entry;
                    }
                    break;
                    // Potentially more cases here later.
            }
        }
    }
}
