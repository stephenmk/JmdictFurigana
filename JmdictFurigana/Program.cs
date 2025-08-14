using System.Diagnostics;
using System.IO;
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
        var t3 = ResourceDownloader.Jmnedict();

        if (Directory.Exists(PathHelper.OutputBasePath))
            logger.Info("Deleting the previously created output directory.");
            Directory.Delete(PathHelper.OutputBasePath, true);
        Directory.CreateDirectory(PathHelper.OutputBasePath);

        await t1;
        logger.Info("Loading kanji data from Kanjidic2.");
        var resourceSet = new FuriganaResourceSet();
        var t4 = resourceSet.LoadAsync();

        await Task.WhenAll(t2, t4);
        logger.Info("Starting the JMdict furigana process.");
        var jmdictEtl = new DictionaryEtl(PathHelper.JmDictPath);
        var furiganaJmdict = new FuriganaBusiness(DictionaryFile.Jmdict, resourceSet);
        var jmdictWriter = new FuriganaFileWriter(PathHelper.JmdictOutFilePath);
        var t5 = jmdictWriter.WriteAsync(furiganaJmdict.ExecuteAsync(jmdictEtl.ExecuteAsync()));

        await t3;
        logger.Info("Starting the JMnedict furigana process.");
        var jmnedictEtl = new DictionaryEtl(PathHelper.JmneDictPath);
        var furiganaJmnedict = new FuriganaBusiness(DictionaryFile.Jmnedict, resourceSet);
        var jmnedictWriter = new FuriganaFileWriter(PathHelper.JmnedictOutFilePath);
        var t6 = jmnedictWriter.WriteAsync(furiganaJmnedict.ExecuteAsync(jmnedictEtl.ExecuteAsync()));

        await Task.WhenAll(t5, t6);
        sw.Stop();
        logger.Info($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }
}
