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
        logger.Info("Starting.");

        var downloader = new ResourceDownloader();
        logger.Info("Downloading the Kanjidic2 file...");
        var t1 = downloader.DownloadKanjidic();
        logger.Info("Downloading the Jmdict file...");
        var t2 = downloader.DownloadJmdict();
        logger.Info("Downloading the Jmnedict file...");
        var t3 = downloader.DownloadJmnedict();

        await Task.WhenAll(t1, t2, t3);
        logger.Info("Resources are now downloaded. Starting the furigana process.");

        // Jmdict
        var jmdictEtl = new DictionaryEtl(PathHelper.JmDictPath);
        var furiganaJmdict = new FuriganaBusiness(DictionaryFile.Jmdict);
        var jmdictWriter = new FuriganaFileWriter(PathHelper.JmdictOutFilePath);
        jmdictWriter.Write(furiganaJmdict.Execute(jmdictEtl.Execute()));

        // Jmnedict
        var jmnedictEtl = new DictionaryEtl(PathHelper.JmneDictPath);
        var furiganaJmnedict = new FuriganaBusiness(DictionaryFile.Jmnedict);
        var jmnedictWriter = new FuriganaFileWriter(PathHelper.JmnedictOutFilePath);
        jmnedictWriter.Write(furiganaJmnedict.Execute(jmnedictEtl.Execute()));

        sw.Stop();
        logger.Info($"Finished in {sw.Elapsed}.");
    }
}
