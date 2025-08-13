using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using JmdictFurigana.Helpers;
using NLog;

namespace JmdictFurigana.Business;

/// <summary>
/// Downloads the dictionary files used by the program.
/// </summary>
public class ResourceDownloader
{
    // HttpClient is intended to be instantiated once per application, rather than per-use
    private static readonly HttpClient Client = new();
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private const string Kanjidic2Uri = "http://www.edrdg.org/kanjidic/kanjidic2.xml.gz";

    // Note that we use the English-only version of the Jmdict file, because it's lighter and we don't need translations
    private const string JmdictUri = "http://ftp.edrdg.org/pub/Nihongo/JMdict_e.gz";

    private const string JmnedictUri = "http://ftp.edrdg.org/pub/Nihongo/JMnedict.xml.gz";

    /// <summary>
    /// Downloads the Kanjidic2 XML file to its resource path.
    /// </summary>
    public static async Task Kanjidic()
    {
        Logger.Info("Starting download of Kanjidic2 file.");
        await DownloadGzFile(Kanjidic2Uri, PathHelper.KanjiDic2Path);
        Logger.Info("Finished download of Kanjidic2 file.");
    }

    /// <summary>
    /// Downloads the JMdict file to its resource path.
    /// </summary>
    public static async Task Jmdict()
    {
        Logger.Info("Starting download of Jmdict file.");
        await DownloadGzFile(JmdictUri, PathHelper.JmDictPath);
        Logger.Info("Finished download of Jmdict file.");
    }

    /// <summary>
    /// Downloads the JMnedict file to its resource path.
    /// </summary>
    public static async Task Jmnedict()
    {
        Logger.Info("Starting download of Jmnedict file.");
        await DownloadGzFile(JmnedictUri, PathHelper.JmneDictPath);
        Logger.Info("Finished download of Jmnedict file.");
    }

    /// <summary>
    /// Downloads and unzips the gzipped file at the given URI, and stores it to the given path.
    /// </summary>
    /// <param name="uri">URI of the file to obtain.</param>
    /// <param name="targetPath">Path of the resulting file.</param>
    private static async Task DownloadGzFile(string uri, string targetPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
        await using var httpStream = await Client.GetStreamAsync(uri);
        await using var gzipStream = new GZipStream(httpStream, CompressionMode.Decompress);
        await using var fileStream = new FileStream(targetPath, FileMode.Create);
        await gzipStream.CopyToAsync(fileStream);
    }
}
