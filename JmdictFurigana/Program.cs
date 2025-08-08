using System.Diagnostics;
using System.Threading.Tasks;
using JmdictFurigana.Business;
using JmdictFurigana.Etl;
using JmdictFurigana.Helpers;
using JmdictFurigana.Models;
using NLog;

namespace JmdictFurigana
{
    public class Program
    {
        public static async Task Main(string[] args)
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
            DictionaryEtl jmdictEtl = new DictionaryEtl(PathHelper.JmDictPath);
            FuriganaBusiness furiganaJmdict = new FuriganaBusiness(DictionaryFile.Jmdict);
            FuriganaFileWriter jmdictWriter = new FuriganaFileWriter(PathHelper.JmdictOutFilePath);
            jmdictWriter.Write(furiganaJmdict.Execute(jmdictEtl.Execute()));

            // Jmnedict
            DictionaryEtl jmnedictEtl = new DictionaryEtl(PathHelper.JmneDictPath);
            FuriganaBusiness furiganaJmnedict = new FuriganaBusiness(DictionaryFile.Jmnedict);
            FuriganaFileWriter jmnedictWriter = new FuriganaFileWriter(PathHelper.JmnedictOutFilePath);
            jmnedictWriter.Write(furiganaJmnedict.Execute(jmnedictEtl.Execute()));

            sw.Stop();
            logger.Info($"Finished in {sw.Elapsed}.");
        }
    }
}
