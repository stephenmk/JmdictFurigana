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
    public async static IAsyncEnumerable<Kanji> ExecuteAsync()
    {
        var supplementaryKanjis = await LoadSupplementaryKanjisAsync(PathHelper.SupplementaryKanjiPath);

        await foreach (var entry in LoadKanjidicEntriesAsync(PathHelper.KanjiDic2Path))
        {
            var kanji = new Kanji
            {
                Character = entry.Literal.First(),
                Readings = entry.ReadingMeaning?
                    .Groups.FirstOrDefault()?
                    .Readings
                    .Where(r => r.IsJapanese)
                    .Select(r => KanaHelper.ToHiragana(r.Text))
                    .ToList() ?? []
            };

            // See if there's a supplementary entry for this kanji.
            var supp = supplementaryKanjis.FirstOrDefault(k => k.Character == kanji.Character);
            if (supp != null)
            {
                // Supplementary entry found. Remove it from the list and add its readings to our current entry.
                kanji.Readings.AddRange(supp.Readings);
                supplementaryKanjis.Remove(supp);
            }

            // Read the nanori readings
            var nanoriReadings = entry.ReadingMeaning?.Nanori ?? [];
            kanji.ReadingsWithNanori = kanji.Readings.Union(nanoriReadings).Distinct().ToList();

            // Return the kanji read and go to the next kanji node.
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
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == Entry.XmlElementName)
                {
                    var entry = await Entry.FromXmlReader(reader);
                    yield return entry;
                }
            }
        }
    }
}
