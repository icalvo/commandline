using System;
using System.Collections.Generic;
using System.Linq;
using SharpX;

namespace CommandLine.Core;

internal static class DictionaryExtensions
{
    public static Maybe<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key) =>
        dic.TryGetValue(key, out TValue? value) ? Maybe.Just(value) : Maybe.Nothing<TValue>();
}

internal static class EnumerableExtensions
{
    public static IEnumerable<T> OnlyJust<T>(this IEnumerable<Maybe<T>> source)
    {
        foreach (var item in source)
            if (item.MatchJust(out T x))
                yield return x;
    }
}

internal static class TypeExtensions
{
    public static Maybe<Type> UnderlyingSequenceType(this Type type) =>
        type.IsArray ? type.GetElementType().AsMaybe() : type.GetGenericArguments().SingleOrDefault().AsMaybe();
}
