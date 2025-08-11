﻿using System.Collections.Generic;

namespace JmdictFurigana.Models;

/// <summary>
/// Represents a kanji.
/// </summary>
public class Kanji
{
    /// <summary>
    /// Gets or sets the character representing the kanji.
    /// </summary>
    public char Character { get; set; }

    /// <summary>
    /// Gets or sets the list of possible readings of the kanji.
    /// </summary>
    public List<string> Readings { get; set; }

    /// <summary>
    /// Gets or sets the list of possible readings of the kanji, including the nanori readings.
    /// </summary>
    public List<string> ReadingsWithNanori { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if this character should make the
    /// vocabulary entry containing it count as kanji strings.
    /// Default is true.
    /// </summary>
    public bool IsRealKanji { get; set; }

    public Kanji()
    {
        Readings = [];
        IsRealKanji = true;
    }

    public override string ToString()
    {
        return Character.ToString();
    }
}
