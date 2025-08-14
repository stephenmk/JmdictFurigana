using JmdictFurigana.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using NLog;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.Threading.Tasks;

namespace JmdictFurigana.Business;

/// <summary>
/// Writes the furigana file.
/// </summary>
class FuriganaFileWriter(string outputPath)
{
    /// <summary>
    /// Gets or sets the value defining whether to write or not words for which a valid
    /// furigana string could not be determined.
    /// </summary>
    public bool WriteUnsuccessfulWords { get; set; }

    public HashSet<string> AlreadyWritten { get; set; } = [];

    /// <summary>
    /// Gets or sets the path where the file is written.
    /// </summary>
    public string OutputPath { get; set; } = outputPath;

    public async Task WriteAsync(IAsyncEnumerable<FuriganaSolutionSet> solutions)
    {
        int success = 0;
        int total = 0;
        var logger = LogManager.GetCurrentClassLogger();
        var start = DateTime.Now;

        var outputDirectory = Path.GetDirectoryName(OutputPath);
        var baseFilename = Path.GetFileNameWithoutExtension(OutputPath);
        var jsonFilePath = Path.Combine(outputDirectory, $"{baseFilename}.json");
        var zipFilePath = Path.Combine(outputDirectory, $"{baseFilename}.zip");
        var tarGzFilePath = Path.Combine(outputDirectory, $"{baseFilename}.tar.gz");

        await using (var stream = new StreamWriter(OutputPath, false, Encoding.UTF8))
        await using (var jsonStream = new StreamWriter(jsonFilePath, false, Encoding.UTF8))
        await using (var jsonWriter = new JsonTextWriter(jsonStream))
        {
            await jsonWriter.WriteStartArrayAsync();
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new FuriganaSolutionJsonSerializer());
            await foreach (var solution in solutions)
            {
                var singleSolution = solution.GetSingleSolution();

                if (!solution.Any())
                    logger.Debug($"X    {solution.Vocab.KanjiReading}|{solution.Vocab.KanaReading}|???");
                else if (singleSolution == null)
                    logger.Debug($"➕   {solution}");
                else
                    logger.Debug($"◯   {solution}");

                if (singleSolution != null && !AlreadyWritten.Contains(singleSolution.ToString()))
                {
                    await stream.WriteLineAsync(singleSolution.ToString());
                    AlreadyWritten.Add(singleSolution.ToString());
                    jsonSerializer.Serialize(jsonWriter, singleSolution);
                }

                if (singleSolution != null)
                    success++;

                total++;
            }
            await jsonWriter.WriteEndArrayAsync();
        }

        // Create a zip of the json file
        using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(jsonFilePath, Path.GetFileName(jsonFilePath));
        }

        // Create a tar of the json file
        await using (var tarStream = new MemoryStream())
        {
            // Create a .tar file in the MemoryStream
            using (var tarWriter = WriterFactory.Open(tarStream, ArchiveType.Tar, CompressionType.None))
                tarWriter.Write(jsonFilePath, jsonFilePath);

            // Reset the position of the MemoryStream to the beginning
            tarStream.Position = 0;

            // Compress the .tar file in the MemoryStream directly into a .tar.gz file
            await using var gzFileStream = File.OpenWrite(tarGzFilePath);
            await using var gzStream = new GZipStream(gzFileStream, CompressionMode.Compress);
            tarStream.CopyTo(gzStream);
        }

        var duration = DateTime.Now - start;
        logger.Info($"Successfuly ended process with {success} out of {total} successfuly found furigana strings.");
        logger.Info($"Process took {double.Round(duration.TotalSeconds, 1)} seconds.");
    }
}

