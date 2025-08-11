using System;
using System.Collections.Generic;
using System.Linq;

namespace JmdictFurigana.Extensions;

public static class ListExtensions
{
    /// <summary>
    /// Clones a list of cloneable objects.
    /// </summary>
    /// <typeparam name="T">List type.</typeparam>
    /// <param name="list">List to clone.</param>
    /// <returns>List containing cloned instances of the input.</returns>
    public static List<T> Clone<T>(this List<T> list) where T: ICloneable
    {
        return list.Select(item => (T)item.Clone()).ToList();
    }
}
