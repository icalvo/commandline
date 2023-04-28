using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;

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
        return source.OfType<Just<T>>().Select(x => x.Value);
    }

    public static Maybe<T> FirstOrNothing<T>(this IEnumerable<T> source, Func<T, bool> test)
    {
        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext())
            if (test(enumerator.Current))
                return Maybe.Just(enumerator.Current);

        return Maybe.Nothing<T>();
    }
}

internal static class TypeExtensions
{
    public static Maybe<Type> UnderlyingSequenceType(this Type type) =>
        type.IsArray ? type.GetElementType().ToMaybe() : type.GetGenericArguments().SingleOrDefault().ToMaybe();
}
