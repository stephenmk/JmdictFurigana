using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JmdictFurigana.Etl.KanjiModels;

public class Reading
{
    public string Text;
    public ReadingType Type;
    public const string XmlTagName = "reading";
    public bool IsJapanese => Type == ReadingType.JapaneseOn || Type == ReadingType.JapaneseKun;
    private static readonly Dictionary<string, ReadingType> AttributeToType = new()
    {
        ["pinyin"] = ReadingType.Pinyin,
        ["korean_r"] = ReadingType.KoreanRomanized,
        ["korean_h"] = ReadingType.KoreanHangul,
        ["vietnam"] = ReadingType.Vietnamese,
        ["ja_on"] = ReadingType.JapaneseOn,
        ["ja_kun"] = ReadingType.JapaneseKun,
    };

    public async static Task<Reading> FromXmlAsync(XmlReader reader)
    {
        var reading = new Reading()
        {
            Type = AttributeToType[reader.GetAttribute("r_type")]
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == XmlTagName)
                    {
                        reading.Text = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return reading;
    }
}
