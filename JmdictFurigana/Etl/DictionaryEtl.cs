using JmdictFurigana.Models;
using JmdictFurigana.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;

namespace JmdictFurigana.Etl
{
    /// <summary>
    /// Parses the dictionary file and produces VocabEntry model instances.
    /// </summary>
    public class DictionaryEtl
    {
        private static readonly XNamespace XmlNs = "http://www.w3.org/XML/1998/namespace";
        private const string XmlNode_Entry = "entry";
        private const string XmlNode_KanjiElement = "k_ele";
        private const string XmlNode_KanjiForm = "keb";
        private const string XmlNode_ReadingElement = "r_ele";
        private const string XmlNode_ReadingForm = "reb";
        private const string XmlNode_ReadingConstraint = "re_restr";
        private const string XmlNode_NoKanji = "re_nokanji";

        private ILogger _logger = LogManager.GetCurrentClassLogger();

        public string DictionaryFilePath { get; set; }

        public DictionaryEtl(string dictionaryFilePath)
        {
            DictionaryFilePath = dictionaryFilePath;
        }

        /// <summary>
        /// Parses the dictionary file and returns entries.
        /// </summary>
        public IEnumerable<VocabEntry> Execute()
        {
            var xdoc = LoadXmlDocument();
            foreach (var xentry in xdoc.Root.Elements(XmlNode_Entry))
            {
                var kanjiForms = ExtractKanjiForms(xentry);
                if (kanjiForms.Count == 0)
                    continue;
                foreach (var xreadingElement in xentry.Elements(XmlNode_ReadingElement))
                {
                    if (xreadingElement.HasElement(XmlNode_NoKanji))
                        continue;
                    if (xreadingElement.Elements("re_inf").Select(x => x.Value).Contains("sk"))
                        continue;

                    var constraintForms = xreadingElement
                        .Elements(XmlNode_ReadingConstraint)
                        .Select(x => x.Value).ToList();

                    var relevantKanjiForms = constraintForms.Count > 0 ? constraintForms : kanjiForms;
                    var reading = xreadingElement.Element(XmlNode_ReadingForm).Value;

                    foreach (var kanjiForm in relevantKanjiForms)
                    {
                        yield return new VocabEntry
                        {
                            KanjiReading = kanjiForm,
                            KanaReading = reading,
                        };
                    }
                }
            }
        }

        private XDocument LoadXmlDocument()
        {
            XDocument doc;
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                MaxCharactersFromEntities = long.MaxValue,
                MaxCharactersInDocument = long.MaxValue,
            };
            var xmlFileTextContent = File.ReadAllText(DictionaryFilePath);
            var bytes = Encoding.UTF8.GetBytes(xmlFileTextContent);
            using (var stream = new MemoryStream(bytes))
            using (var reader = XmlReader.Create(stream, settings))
            doc = XDocument.Load(reader);
            return doc;
        }

        private List<string> ExtractKanjiForms(XElement entry)
        {
            var kanjiForms = new List<string>();
            foreach (var xkanjiElement in entry.Elements(XmlNode_KanjiElement))
            {
                if (xkanjiElement.Elements("ke_inf").Select(i => i.Value).Contains("sK"))
                    continue;
                kanjiForms.Add(xkanjiElement.Element(XmlNode_KanjiForm).Value);
            }
            return kanjiForms;
        }
    }
}
