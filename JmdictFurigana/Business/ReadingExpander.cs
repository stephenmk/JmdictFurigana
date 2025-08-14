using JmdictFurigana.Helpers;
using JmdictFurigana.Models;
using JmdictFurigana.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JmdictFurigana.Business;

/// <summary>
/// Provides a process that expands a given list of readings by adding rendaku versions and stuff like this.
/// </summary>
public static class ReadingExpander
{
    private static readonly Dictionary<char, char[]> RendakuDictionary = new()
    {
        ['か'] = ['が'],
        ['き'] = ['ぎ'],
        ['く'] = ['ぐ'],
        ['け'] = ['げ'],
        ['こ'] = ['ご'],
        ['さ'] = ['ざ'],
        ['し'] = ['じ'],
        ['す'] = ['ず'],
        ['せ'] = ['ぜ'],
        ['そ'] = ['ぞ'],
        ['た'] = ['だ'],
        ['ち'] = ['ぢ','じ'],
        ['つ'] = ['づ','ず'],
        ['て'] = ['で'],
        ['と'] = ['ど'],
        ['は'] = ['ば','ぱ'],
        ['ひ'] = ['び','ぴ'],
        ['ふ'] = ['ぶ','ぷ'],
        ['へ'] = ['べ','ぺ'],
        ['ほ'] = ['ぼ','ぽ'],
    };

    private static readonly Dictionary<string, string> AfterDotKunYomiTransformDictionary = new()
    {
        ["く"] = "き",
        ["ぐ"] = "ぎ",
        ["す"] = "し",
        ["ず"] = "じ",
        ["む"] = "み",
        ["る"] = "り",
        ["ぶ"] = "び",
        ["う"] = "い",
    };

    private static readonly char[] SmallTsuRendakuList =
    [
        'つ',
        'く',
        'き',
        'ち'
    ];

    /// <summary>
    /// Given a kanji, finds and returns all potential readings that it could take in a string.
    /// </summary>
    /// <param name="k">Kanji to evaluate.</param>
    /// <param name="isFirstChar">Set to true if this kanji is the first character of the string
    /// that the kanji is found in.</param>
    /// <param name="isLastChar">Set to true if this kanji is the last character of the string
    /// that the kanji is found in.</param>
    /// <param name="useNanori">Set to true to use nanori readings as well.</param>
    /// <returns>A list containing all potential readings that the kanji could take.</returns>
    public static List<string> GetPotentialKanjiReadings(Kanji k, bool isFirstChar, bool isLastChar, bool useNanori)
    {
        var output = new List<string>();
        foreach (string reading in useNanori ? k.ReadingsWithNanori : k.Readings)
        {
            string r = reading.Replace("-", string.Empty);
            if (!KanaHelper.IsAllKatakana(r))
            {
                r = r.Replace("ー", string.Empty);
            }

            string[] dotSplit = r.Split('.');
            if (dotSplit.Length == 1)
            {
                output.Add(r);
            }
            else if (dotSplit.Length == 2)
            {
                output.Add(dotSplit[0]);
                output.Add(r.Replace(".", string.Empty));

                if (AfterDotKunYomiTransformDictionary.TryGetValue(dotSplit[1], out string newTerm))
                {
                    string newReading = r.Replace(".", string.Empty);
                    newReading = newReading[..^dotSplit[1].Length];
                    newReading += newTerm;
                    output.Add(newReading);
                }

                if (dotSplit[1].Length >= 2 && dotSplit[1][1] == 'る')
                {
                    // Add variant without the ending る.
                    string newReading = r.Replace(".", string.Empty);
                    newReading = newReading[..^1];
                    output.Add(newReading);
                }
            }
            else
            {
                throw new Exception(string.Format("Weird reading: {0} for kanji {1}.", reading, k.Character));
            }
        }

        // Add final small tsu rendaku
        if (!isLastChar)
        {
            output.AddRange(GetSmallTsuRendaku(output));
        }

        // Rendaku
        if (!isFirstChar)
        {
            output.AddRange(GetAllRendaku(output));
        }

        return output.Distinct().ToList();
    }

    /// <summary>
    /// Given a special reading expression, returns all potential kana readings the expression could use.
    /// </summary>
    /// <param name="sp">Target special reading expression.</param>
    /// <param name="isFirstChar">Set to true if the first character of the expression is the first
    /// character of the string that the expression is found in.</param>
    /// <param name="isLastChar">Set to true if the last character of the expression is the last
    /// character of the string that the expression is found in.</param>
    /// <returns>A list containing all potential readings the expression could assume.</returns>
    public static List<SpecialReading> GetPotentialSpecialReadings(SpecialExpression sp, bool isFirstChar, bool isLastChar)
    {
        var output = new List<SpecialReading>(sp.Readings);

        // Add final small tsu rendaku
        if (!isLastChar)
        {
            var add = new List<SpecialReading>();
            foreach (var r in output)
            {
                if (SmallTsuRendakuList.Contains(r.KanaReading.Last()))
                {
                    string newKanaReading = r.KanaReading[..^1] + "っ";
                    var newReading = new SpecialReading(newKanaReading, new FuriganaSolution(r.Furigana.Vocab,
                        r.Furigana.Furigana.Clone()));

                    var affectedParts = newReading.Furigana.GetPartsForIndex(
                        newReading.Furigana.Vocab.KanjiReading.Length - 1);
                    foreach (var part in affectedParts)
                    {
                        part.Value = part.Value.Remove(part.Value.Length - 1) + "っ";
                    }
                    add.Add(newReading);
                }
            }
            output.AddRange(add);
        }

        // Rendaku
        if (!isFirstChar)
        {
            var add = new List<SpecialReading>();
            foreach (var r in output)
            {
                if (RendakuDictionary.TryGetValue(r.KanaReading.First(), out char[] rendakuChars))
                {
                    foreach (var renChar in rendakuChars)
                    {
                        var newKanaReading = renChar + r.KanaReading[1..];
                        var newReading = new SpecialReading(newKanaReading, new FuriganaSolution(r.Furigana.Vocab,
                            r.Furigana.Furigana.Clone()));

                        var affectedParts = newReading.Furigana.GetPartsForIndex(0);
                        foreach (var part in affectedParts)
                        {
                            part.Value = renChar + part.Value[1..];
                        }
                        add.Add(newReading);
                    }
                }
            }
            output.AddRange(add);
        }
        return output.Distinct().ToList();
    }

    private static List<string> GetSmallTsuRendaku(List<string> readings)
    {
        var addedOutput = new List<string>();
        foreach (var reading in readings)
        {
            if (SmallTsuRendakuList.Contains(reading.Last()))
            {
                addedOutput.Add(reading[..^1] + "っ");
            }
        }

        return addedOutput;
    }

    private static List<string> GetAllRendaku(List<string> readings)
    {
        var rendakuOutput = new List<string>();
        foreach (var reading in readings)
        {
            if (RendakuDictionary.TryGetValue(reading.First(), out char[] rendakuChars))
            {
                foreach (var renChar in rendakuChars)
                {
                    rendakuOutput.Add(renChar + reading[1..]);
                }
            }
        }
        return rendakuOutput;
    }
}
