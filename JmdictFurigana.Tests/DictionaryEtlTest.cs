using System.Collections.Generic;
using System.IO;
using System.Linq;
using JmdictFurigana.Etl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JmdictFurigana.Tests;

[TestClass]
public class DictionaryEtlTest
{
    [TestMethod]
    public void ExecuteTest_Waruguchi_FourReadings()
    {
        // Arrange
        var dictionaryEtl = new DictionaryEtl(Path.Combine("Resources", "Waruguchi.xml"));
        var wanted = new List<string>()
        {
            "悪口|あっこう",
            "悪口|わるくち",
            "悪口|わるぐち",
            "惡口|あっこう",
            "惡口|わるくち",
            "惡口|わるぐち",
        };

        // Act
        var results = dictionaryEtl.Execute().ToList();
        var resultsAsStrings = results.Select(r => r.ToString()).ToList();

        // Assert
        CollectionAssert.AreEquivalent(wanted, resultsAsStrings);
    }
}
