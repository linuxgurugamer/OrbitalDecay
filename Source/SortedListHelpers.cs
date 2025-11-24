using System;
using System.Collections.Generic;

public static class SortedListHelpers
{
    private class AscendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y) => x.CompareTo(y);
    }

    private class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y) => y.CompareTo(x);
    }

    /// <summary>
    /// Creates a SortedList with optional descending sort order.
    /// </summary>
    public static SortedList<TKey, TValue> Create<TKey, TValue>(bool descending)
        where TKey : IComparable<TKey>
    {
        IComparer<TKey> cmp = descending
            ? (IComparer<TKey>)new DescendingComparer<TKey>()
            : new AscendingComparer<TKey>();

        return new SortedList<TKey, TValue>(cmp);
    }
}
