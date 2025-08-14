using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JmdictFurigana.Business;
using JmdictFurigana.Helpers;
using JmdictFurigana.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JmdictFurigana.Tests;

[TestClass]
public class FuriganaTest
{
    /// <summary>
    /// Downloads missing resources.
    /// </summary>
    [TestInitialize]
    public async Task Initialize()
    {
        if (!File.Exists(PathHelper.KanjiDic2Path))
            await ResourceDownloader.Kanjidic();
    }

    [TestMethod]
    public async Task TestFuriganaGanbaruAsync()
    {
        await TestFuriganaAsync("頑張る", "がんばる", "0:がん;1:ば");
    }

    [TestMethod]
    public async Task TestFuriganaIkkagetsuAsync()
    {
        await TestFuriganaAsync("一ヶ月", "いっかげつ", "0:いっ;1:か;2:げつ");
    }

    [TestMethod]
    public async Task TestFuriganaObocchanAsync()
    {
        await TestFuriganaAsync("御坊っちゃん", "おぼっちゃん", "0:お;1:ぼ");
    }

    [TestMethod]
    [Ignore]
    public async Task TestFuriganaAraAsync()
    {
        // Will fail. This is a weird kanji. The string containing only the kanji is Length == 2.
        // Would be cool to find a solution but don't worry too much about it.
        await TestFuriganaAsync("𩺊", "あら", "0:あら");
    }

    [TestMethod]
    public async Task TestFuriganaIjirimawasuAsync()
    {
        await TestFuriganaAsync("弄り回す", "いじりまわす", "0:いじ;2:まわ");
    }

    [TestMethod]
    public async Task TestFuriganaKassarauAsync()
    {
        await TestFuriganaAsync("掻っ攫う", "かっさらう", "0:か;2:さら");
    }

    [TestMethod]
    public async Task TestFuriganaOneesanAsync()
    {
        await TestFuriganaAsync("御姉さん", "おねえさん", "0:お;1:ねえ");
    }

    [TestMethod]
    public async Task TestFuriganaHakabakashiiAsync()
    {
        await TestFuriganaAsync("捗捗しい", "はかばかしい", "0:はか;1:ばか");
    }

    [TestMethod]
    public async Task TestFuriganaIssue5Async()
    {
        var testData = new List<(string, string, string)>
        {
            ("御兄さん", "おにいさん", "0:お;1:にい"),
            ("御姉さん", "おねえさん", "0:お;1:ねえ"),
            ("御母さん", "おかあさん", "0:お;1:かあ"),
            ("抑抑", "そもそも", "0:そも;1:そも"),
            ("犇犇", "ひしひし", "0:ひし;1:ひし"),
            ("険しい路", "けわしいみち", "0:けわ;3:みち"),
            ("芝生", "しばふ", "0-1:しばふ"),
            ("純日本風", "じゅんにほんふう", "0:じゅん;1-2:にほん;3:ふう"),
            ("真珠湾", "しんじゅわん", "0:しん;1:じゅ;2:わん"),
            ("草履", "ぞうり", "0-1:ぞうり"),
            ("大和魂", "やまとだましい", "0-1:やまと;2:だましい"),
            ("竹刀", "しない", "0-1:しない"),
            ("東京湾", "とうきょうわん", "0:とう;1:きょう;2:わん"),
            ("日本学者", "にほんがくしゃ", "0-1:にほん;2:がく;3:しゃ"),
            ("日本製", "にほんせい", "0-1:にほん;2:せい"),
            ("日本側", "にほんがわ", "0-1:にほん;2:がわ"),
            ("日本刀", "にほんとう", "0-1:にほん;2:とう"),
            ("日本風", "にほんふう", "0-1:にほん;2:ふう"),
            ("木ノ葉", "このは", "0:こ;2:は"),
            ("木ノ葉", "きのは", "0:き;2:は"),
            ("余所見", "よそみ", "0:よ;1:そ;2:み"),
            ("嗹", "れん", "0:れん"),
            ("愈愈", "いよいよ", "0:いよ;1:いよ"),
            ("偶偶", "たまたま", "0:たま;1:たま"),
            ("益益", "ますます", "0:ます;1:ます"),
            ("風邪薬", "かぜぐすり", "0-1:かぜ;2:ぐすり"),
            ("日独協会", "にちどくきょうかい", "0:にち;1:どく;2:きょう;3:かい"),
        };
        foreach (var x in testData)
        {
            await TestFuriganaAsync(x.Item1, x.Item2, x.Item3);
        }
    }

    public async static Task TestFuriganaAsync(string kanjiReading, string kanaReading, string expectedFurigana)
    {
        var v = new VocabEntry(kanjiReading, kanaReading);
        var resourceSet = new FuriganaResourceSet();
        await resourceSet.LoadAsync();
        var business = new FuriganaBusiness(DictionaryFile.Jmdict, resourceSet);
        var result = business.Execute(v);

        if (result.GetSingleSolution() == null)
        {
            Assert.Fail();
        }
        else
        {
            Assert.AreEqual(FuriganaSolution.Parse(expectedFurigana, v), result.GetSingleSolution());
        }
    }

    [TestMethod]
    public void TestBreakIntoPartsAkagaeruka()
    {
        var vocab = new VocabEntry("アカガエル科", "アカガエルか");
        var solution = new FuriganaSolution(vocab, new FuriganaPart("か", 5));

        var parts = solution.BreakIntoParts().ToList();

        Assert.AreEqual(2, parts.Count);
        Assert.AreEqual("アカガエル", parts[0].Text);
        Assert.IsNull(parts[0].Furigana);
        Assert.AreEqual("科", parts[1].Text);
        Assert.AreEqual("か", parts[1].Furigana);
    }

    [TestMethod]
    public void TestBreakIntoPartsOtonagai()
    {
        var vocab = new VocabEntry("大人買い", "おとながい");
        var solution = new FuriganaSolution(vocab, new FuriganaPart("おとな", 0, 1), new FuriganaPart("が", 2));

        var parts = solution.BreakIntoParts().ToList();

        Assert.AreEqual(3, parts.Count);
        Assert.AreEqual("大人", parts[0].Text);
        Assert.AreEqual("おとな", parts[0].Furigana);
        Assert.AreEqual("買", parts[1].Text);
        Assert.AreEqual("が", parts[1].Furigana);
        Assert.AreEqual("い", parts[2].Text);
        Assert.IsNull(parts[2].Furigana);
    }
}
