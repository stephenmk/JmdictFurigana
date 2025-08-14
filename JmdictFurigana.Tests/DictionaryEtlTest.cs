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
    public async Task ExecuteTestWaruguchiFourReadings()
    {
        var expectedResults = new List<string>()
        {
            "悪口|あっこう",
            "悪口|わるくち",
            "悪口|わるぐち",
            "惡口|あっこう",
            "惡口|わるくち",
            "惡口|わるぐち",
        };
        var dictionaryFilePath = Path.Combine("Resources", "Waruguchi.xml");
        var dictionaryEtl = new DictionaryEtl(dictionaryFilePath);
        var results = new List<string>();

        await foreach(var vocabEntry in dictionaryEtl.ExecuteAsync())
        {
            results.Add(vocabEntry.ToString());
        }

        // Note that the order of the strings in the lists doesn't matter.
        CollectionAssert.AreEquivalent(expectedResults, results);
    }
}
