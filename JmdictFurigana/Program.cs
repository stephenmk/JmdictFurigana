using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JmdictFurigana.Business;
using JmdictFurigana.Etl;
using JmdictFurigana.Helpers;
using JmdictFurigana.Models;
using NLog;

namespace JmdictFurigana;

public class Program
{
    public static async Task Main()
    {
        var sw = new Stopwatch();
        sw.Start();
        var logger = LogManager.GetCurrentClassLogger();

        logger.Info("Fetching resources.");
        var t1 = ResourceDownloader.Kanjidic();
        var t2 = ResourceDownloader.Jmdict();
        await Task.WhenAll(t1, t2);
        var t3 = ResourceDownloader.Jmnedict();

        logger.Info("Starting the JMdict furigana process.");
        var jmdictEtl = new DictionaryEtl(PathHelper.JmDictPath);
        var furiganaJmdict = new FuriganaBusiness(DictionaryFile.Jmdict);
        var jmdictWriter = new FuriganaFileWriter(PathHelper.JmdictOutFilePath);
        jmdictWriter.Write(furiganaJmdict.Execute(jmdictEtl.Execute()));

        await t3;

        logger.Info("Starting the JMnedict furigana process.");
        var jmnedictEtl = new DictionaryEtl(PathHelper.JmneDictPath);
        var furiganaJmnedict = new FuriganaBusiness(DictionaryFile.Jmnedict);
        var jmnedictWriter = new FuriganaFileWriter(PathHelper.JmnedictOutFilePath);
        jmnedictWriter.Write(furiganaJmnedict.Execute(jmnedictEtl.Execute()));

        sw.Stop();
        logger.Info($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }
}
