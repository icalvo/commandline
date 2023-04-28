// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;

namespace CommandLine.Core;

internal static class PartitionExtensions
{
    public static Tuple<IEnumerable<T>, IEnumerable<T>> PartitionByPredicate<T>(this IEnumerable<T> items,
        Func<T, bool> pred)
    {
        var yes = new List<T>();
        var no = new List<T>();
        foreach (T item in items)
        {
            var list = pred(item) ? yes : no;
            list.Add(item);
        }

        return Tuple.Create<IEnumerable<T>, IEnumerable<T>>(yes, no);
    }
}
