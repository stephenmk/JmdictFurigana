using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JmdictFurigana.Etl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JmdictFurigana.Tests;

[TestClass]
public class DictionaryEtlTest
{
    [TestMethod]
    public async Task ExecuteTestWaruguchiReadings()
    {
        /*
            `Waruguchi.xml` contains an entry with the following forms and info tags:

            Kanji Forms
            悪口；惡口[oK]；わる口；悪ぐち[sK]
            Readings
            あっこう[悪口,惡口]；わるくち；わるぐち；あかくち[nokanji]；あかぐち[sk]

            The reading あっこう is restricted to forms 悪口 and 惡口.
            The kanji form 悪ぐち is hidden and therefore has no associated readings.
            The kana form あかくち is tagged "nokanji" and therefore has no associated kanji forms.
            The kana form あかぐち is hidden and therefore has no associated kanji forms.
        */
        var expectedResults = new List<string>()
        {
            // あっこう has two kanji forms.
            "悪口|あっこう",
            "惡口|あっこう",

            // わるくち has three kanji forms.
            "悪口|わるくち",
            "惡口|わるくち",
            "わる口|わるくち",

            // わるぐち has three kanji forms.
            "悪口|わるぐち",
            "惡口|わるぐち",
            "わる口|わるぐち",
        };
        var dictionaryFilePath = Path.Combine("Resources", "Waruguchi.xml");
        var dictionaryEtl = new DictionaryEtl(dictionaryFilePath);
        var results = new List<string>();

        await foreach (var vocabEntry in dictionaryEtl.ExecuteAsync())
        {
            results.Add(vocabEntry.ToString());
        }

        // Note that the order of the strings in the lists doesn't matter.
        CollectionAssert.AreEquivalent(expectedResults, results);
    }
}
