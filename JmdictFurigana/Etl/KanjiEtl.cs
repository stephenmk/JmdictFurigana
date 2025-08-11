using JmdictFurigana.Helpers;
using JmdictFurigana.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace JmdictFurigana.Etl;

/// <summary>
/// Parses the kanji data file and produces instances of the Kanji model.
/// </summary>
public class KanjiEtl
{
    #region Constants

    private static readonly string XmlNode_Character = "character";
    private static readonly string XmlNode_Literal = "literal";
    private static readonly string XmlNode_ReadingMeaning = "reading_meaning";
    private static readonly string XmlNode_ReadingMeaningGroup = "rmgroup";
    private static readonly string XmlNode_Reading = "reading";
    private static readonly string XmlNode_Nanori = "nanori";

    private static readonly string XmlAttribute_ReadingType = "r_type";

    private static readonly string XmlAttributeValue_KunYomiReading = "ja_kun";
    private static readonly string XmlAttributeValue_OnYomiReading = "ja_on";

    #endregion

    #region Fields

    private ILogger _logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Methods

    /// <summary>
    /// Reads and returns Kanji models.
    /// </summary>
    public IEnumerable<Kanji> Execute()
    {
        var supplementaryKanjis = SupplementaryKanjis();

        var validReadingAttrs = new HashSet<string> {
            XmlAttributeValue_OnYomiReading,
            XmlAttributeValue_KunYomiReading,
        };

        var xdoc = XDocument.Load(PathHelper.KanjiDic2Path);

        foreach (var xkanji in xdoc.Root.Elements(XmlNode_Character))
        {
            var xreadingMeaning = xkanji.Element(XmlNode_ReadingMeaning);
            var kanji = new Kanji
            {
                Character = xkanji.Element(XmlNode_Literal).Value.First(),
                Readings = xreadingMeaning?.Element(XmlNode_ReadingMeaningGroup)?
                    .Elements(XmlNode_Reading)
                    .Where(r => validReadingAttrs.Contains(r.Attribute(XmlAttribute_ReadingType).Value))
                    .Select(r => KanaHelper.ToHiragana(r.Value))
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
            var nanoriReadings = xreadingMeaning?.Elements(XmlNode_Nanori)
                .Select(n => n.Value)
                .ToList() ?? [];
            kanji.ReadingsWithNanori = kanji.Readings.Union(nanoriReadings).Distinct().ToList();

            // Return the kanji read and go to the next kanji node.
            yield return kanji;

            xkanji.RemoveAll();
        }

        // Return the remaining supplementary kanji as new kanji.
        foreach (var kanji in supplementaryKanjis)
        {
            yield return kanji;
        }
    }

    private static List<Kanji> SupplementaryKanjis()
    {
        var supplementaryKanjis = new List<Kanji>();
        foreach (string line in File.ReadAllLines(PathHelper.SupplementaryKanjiPath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.First() == ';')
                continue;

            char c = line.First();
            var split = line.Split(SeparatorHelper.FileFieldSeparator);
            var readings = split[1].Split(SeparatorHelper.FileReadingSeparator);

            var kanji = new Kanji()
            {
                Character = c,
                Readings = readings.ToList(),
                ReadingsWithNanori = readings.ToList(),
                IsRealKanji = false
            };
            supplementaryKanjis.Add(kanji);
        }
        return supplementaryKanjis;
    }
    #endregion
}
