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

        string currentTagName = XmlTagName;
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTagName = reader.Name;
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                if (currentTagName == XmlTagName)
                {
                    reading.Text = await reader.GetValueAsync();
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (reader.Name == XmlTagName)
                {
                    break;
                }
            }
        }
        return reading;
    }
}
