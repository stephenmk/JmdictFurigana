using System.Collections.Generic;

namespace JmdictFurigana.Models;

public class SpecialReading(string kanaReading, FuriganaSolution furigana) : IEqualityComparer<SpecialReading>
{
    #region Properties

    /// <summary>
    /// Gets or sets the kana reading string of the special reading.
    /// </summary>
    public string KanaReading { get; set; } = kanaReading;

    /// <summary>
    /// Gets or sets the furigana solution of the special reading.
    /// </summary>
    public FuriganaSolution Furigana { get; set; } = furigana;

    #endregion

    #region Constructors

    public SpecialReading()
        : this(string.Empty, null) { }

    #endregion

    #region Methods

    public bool Equals(SpecialReading x, SpecialReading y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        return x.KanaReading == y.KanaReading
            && x.Furigana.Equals(y.Furigana);
    }

    public int GetHashCode(SpecialReading obj)
    {
        return GetHashCode();
    }

    #endregion
}
