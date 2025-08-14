using JmdictFurigana.Etl;
using JmdictFurigana.Helpers;
using JmdictFurigana.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JmdictFurigana.Business;

public class FuriganaResourceSet
{
    private Dictionary<char, Kanji> _kanjiDictionary = [];
    private Dictionary<string, FuriganaSolution> _overrideList = [];
    private Dictionary<string, SpecialExpression> _specialExpressions = [];

    #region Methods

    #region Loading

    /// <summary>
    /// Loads the resources. Should be done before using any accessor method.
    /// </summary>
    public async Task LoadAsync(string kanjiDictionaryPath)
    {
        var t1 = LoadKanjiDictionaryAsync(kanjiDictionaryPath);
        var t2 = LoadOverrideListAsync();
        var t3 = LoadSpecialExpressionsAsync();
        await Task.WhenAll(t1, t2, t3);
    }

    /// <summary>
    /// Loads the kanji dictionary using resource files.
    /// </summary>
    private async Task LoadKanjiDictionaryAsync(string path)
    {
        _kanjiDictionary = [];
        await foreach (var kanji in KanjiEtl.ExecuteAsync(path))
        {
            AddKanjiEntry(kanji);
        }
    }

    /// <summary>
    /// Loads the furigana override list.
    /// </summary>
    private async Task LoadOverrideListAsync()
    {
        _overrideList = [];
        await foreach (string line in File.ReadLinesAsync(PathHelper.OverrideFuriganaPath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.First() == ';')
                continue;
            var split = line.Split(SeparatorHelper.FileFieldSeparator);
            var vocab = new VocabEntry(split[0], split[1]);
            var solution = FuriganaSolution.Parse(split[2], null);
            _overrideList.Add(vocab.ToString(), solution);
        }
    }

    /// <summary>
    /// Loads the special expressions dictionary.
    /// </summary>
    private async Task LoadSpecialExpressionsAsync()
    {
        _specialExpressions = [];
        await foreach (string line in File.ReadLinesAsync(PathHelper.SpecialReadingsPath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.First() == ';')
                continue;

            var split = line.Split(SeparatorHelper.FileFieldSeparator);
            var kanjiReading = split[0];
            var kanaReading = split[1];

            var v = new VocabEntry(kanjiReading, kanaReading);

            // Read the solution if it is explicitly written. Compute it otherwise.
            var solution = split.Length == 3 ?
                FuriganaSolution.Parse(split[2], v)
                : new FuriganaSolution(v, new FuriganaPart(kanaReading, 0, kanjiReading.Length - 1));

            // Add the special reading or special expression.
            var specialReading = new SpecialReading(kanaReading, solution);
            if (_specialExpressions.TryGetValue(kanjiReading, out SpecialExpression specExp))
            {
                specExp.Readings.Add(specialReading);
            }
            else
            {
                _specialExpressions.Add(kanjiReading, new SpecialExpression(kanjiReading, specialReading));
            }
        }
    }

    /// <summary>
    /// Adds or merge a kanji to the kanji dictionary.
    /// </summary>
    /// <param name="newKanji">Kanji to add or merge.</param>
    private void AddKanjiEntry(Kanji newKanji)
    {
        if (_kanjiDictionary.TryGetValue(newKanji.Character, out Kanji kanji))
        {
            kanji.Readings.AddRange(newKanji.Readings);
            kanji.Readings = kanji.Readings.Distinct().ToList();
            kanji.ReadingsWithNanori.AddRange(newKanji.ReadingsWithNanori);
            kanji.ReadingsWithNanori = kanji.ReadingsWithNanori.Distinct().ToList();
        }
        else
        {
            _kanjiDictionary.Add(newKanji.Character, newKanji);
        }
    }

    #endregion

    #region Getters

    /// <summary>
    /// Gets the kanji matching the given character from the dictionary.
    /// </summary>
    /// <param name="c">Character to look for in the kanji dictionary.</param>
    /// <returns>The kanji matching the given character, or null if it does not exist.</returns>
    public Kanji GetKanji(char c)
    {
        return _kanjiDictionary.TryGetValue(c, out Kanji kanji) ? kanji : null;
    }

    /// <summary>
    /// Gets the special expression matching the given string from the dictionary.
    /// </summary>
    /// <param name="s">String to look for in the special expression dictionary.</param>
    /// <returns>The expression matching the given string, or null if it does not exist.</returns>
    public SpecialExpression GetExpression(string s)
    {
        return _specialExpressions.TryGetValue(s, out SpecialExpression exp) ? exp : null;
    }

    /// <summary>
    /// Gets the override solution matching the given vocab entry.
    /// </summary>
    /// <param name="v">Entry to look for in the override list.</param>
    /// <returns>The matching solution if found. Null otherwise.</returns>
    public FuriganaSolution GetOverride(VocabEntry v)
    {
        string s = v.ToString();
        return _overrideList.TryGetValue(s, out FuriganaSolution sol) ? sol : null;
    }

    #endregion

    #endregion
}
