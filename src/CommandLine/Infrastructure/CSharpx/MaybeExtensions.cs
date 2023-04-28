//#define CSX_MAYBE_INTERNAL // Uncomment or define at build time set accessibility to internal.
//#define CSX_REM_EITHER_FUNC // Uncomment or define at build time to remove dependency to Either.cs.

using System;
using System.Collections.Generic;
using System.Linq;
using SharpX;

namespace CommandLine;

/// <summary>
///     Provides convenience extension methods for <see cref="Maybe" />.
/// </summary>
#if !CSX_MAYBE_INTERNAL
    public
#endif
internal static class MaybeExtensions
{
    #region Alternative Match Methods

    /// <summary>
    ///     Provides pattern matching using <see cref="System.Action" /> delegates.
    /// </summary>
    public static void Match<T>(this Maybe<T> maybe, Action<T> ifJust, Action ifNothing)
    {
        if (maybe.MatchJust(out T? value))
        {
            ifJust(value);
            return;
        }

        ifNothing();
    }

    /// <summary>
    ///     Provides pattern matching using <see cref="System.Action" /> delegates.
    /// </summary>
    public static T2 Match<T1, T2>(this Maybe<T1> maybe, Func<T1, T2> ifJust, T2 ifNothing) =>
        maybe.MatchJust(out T1? value) ? ifJust(value) : ifNothing;

    /// <summary>
    ///     Provides pattern matching using <see cref="System.Action" /> delegates over maybe with tupled wrapped value.
    /// </summary>
    public static void Match<T1, T2>(this Maybe<Tuple<T1, T2>> maybe, Action<T1?, T2?> ifJust, Action ifNothing)
    {
        if (maybe.MatchJust(out T1? value1, out T2? value2))
        {
            ifJust(value1, value2);
            return;
        }

        ifNothing();
    }

    /// <summary>
    ///     Matches a value returning <c>true</c> and tupled value itself via two output parameters.
    /// </summary>
    public static bool MatchJust<T1, T2>(this Maybe<Tuple<T1, T2>> maybe, out T1? value1, out T2? value2)
    {
        if (maybe.MatchJust(out var value))
        {
            value1 = value.Item1;
            value2 = value.Item2;
            return true;
        }

        value1 = default;
        value2 = default;
        return false;
    }

    #endregion

    /// <summary>
    ///     Equivalent to monadic <see cref="Maybe.Return{T}" /> operation.
    ///     Builds a <see cref="CSharpx.Just{T}" /> value in case <paramref name="value" /> is different from its default.
    /// </summary>
    public static Maybe<T> AsMaybe<T>(this T? value) => value == null ? Maybe.Nothing<T>() : Maybe.Return(value);

    /// <summary>
    ///     Invokes a function on this maybe value that itself yields a maybe.
    /// </summary>
    public static Maybe<T2> Bind<T1, T2>(this Maybe<T1> maybe, Func<T1, Maybe<T2>> func) => Maybe.Bind(maybe, func);

    /// <summary>
    ///     Transforms this maybe value by using a specified mapping function.
    /// </summary>
    public static Maybe<T2> Map<T1, T2>(this Maybe<T1> maybe, Func<T1, T2> func) => Maybe.Map(maybe, func);

    #region Linq Operators

    /// <summary>
    ///     Map operation compatible with Linq.
    /// </summary>
    public static Maybe<TResult> Select<TSource, TResult>(this Maybe<TSource> maybe, Func<TSource, TResult> selector) =>
        Maybe.Map(maybe, selector);

    /// <summary>
    ///     Bind operation compatible with Linq.
    /// </summary>
    public static Maybe<TResult> SelectMany<TSource, TValue, TResult>(this Maybe<TSource> maybe,
        Func<TSource, Maybe<TValue>> valueSelector, Func<TSource, TValue, TResult> resultSelector)
    {
        return maybe.Bind(
            sourceValue => valueSelector(sourceValue).Map(resultValue => resultSelector(sourceValue, resultValue)));
    }

    #endregion

    #region Do Semantic

    /// <summary>
    ///     If contains a value executes an <see cref="System.Action{T}" /> delegate over it.
    /// </summary>
    public static void Do<T>(this Maybe<T> maybe, Action<T> action)
    {
        if (maybe.MatchJust(out T? value)) action(value);
    }

    /// <summary>
    ///     If contans a value executes an <see cref="System.Action{T1, T2}" /> delegate over it.
    /// </summary>
    public static void Do<T1, T2>(this Maybe<(T1, T2)> maybe, Action<T1?, T2?> action)
    {
        T1? value1;
        T2? value2;
        if (maybe.MatchJust(out value1, out value2)) action(value1, value2);
    }

    #endregion

    /// <summary>
    ///     Returns <c>true</c> iffits argument is of the form <see cref="CSharpx.Just{T}" />.
    /// </summary>
    public static bool IsJust<T>(this Maybe<T> maybe) => maybe.Tag == MaybeType.Just;

    /// <summary>
    ///     Returns <c>true</c> iffits argument is of the form <see cref="CSharpx.Nothing{T}" />.
    /// </summary>
    public static bool IsNothing<T>(this Maybe<T> maybe) => maybe.Tag == MaybeType.Nothing;

    /// <summary>
    ///     Extracts the element out of a <see cref="CSharpx.Just{T}" /> and returns a default value if its argument is
    ///     <see cref="CSharpx.Nothing{T}" />.
    /// </summary>
    public static T? FromJust<T>(this Maybe<T> maybe) => maybe.MatchJust(out T? value) ? value : default;

    /// <summary>
    ///     Extracts the element out of a <see cref="CSharpx.Just{T}" /> and throws an error if its argument is
    ///     <see cref="CSharpx.Nothing{T}" />.
    /// </summary>
    public static T FromJustOrFail<T>(this Maybe<T> maybe) => FromJustOrFail(maybe, (Func<Exception>?)null);

    /// <summary>
    ///     Extracts the element out of a <see cref="CSharpx.Just{T}" /> and throws an error if its argument is
    ///     <see cref="CSharpx.Nothing{T}" />.
    /// </summary>
    public static T FromJustOrFail<T>(this Maybe<T> maybe, Exception exceptionToThrow) =>
        FromJustOrFail(maybe, () => exceptionToThrow);

    /// <summary>
    ///     Extracts the element out of a <see cref="CSharpx.Just{T}" /> and throws an error if its argument is
    ///     <see cref="CSharpx.Nothing{T}" />.
    /// </summary>
    public static T FromJustOrFail<T>(this Maybe<T> maybe, Func<Exception>? exceptionToThrow = null)
    {
        if (maybe.MatchJust(out T? value)) return value;

        throw exceptionToThrow?.Invoke() ?? new ArgumentException("Value empty.");
    }

    /// <summary>
    ///     If contains a values returns  it, otherwise returns <paramref name="noneValue" />.
    /// </summary>
    public static T? GetValueOrNull<T>(this Maybe<T> maybe) where T : class =>
        maybe.MatchJust(out T? value) ? value : null;

    /// <summary>
    ///     If contains a values returns  it, otherwise returns <paramref name="noneValue" />.
    /// </summary>
    public static T GetValueOrDefault<T>(this Maybe<T> maybe, T noneValue) =>
        maybe.MatchJust(out T? value) ? value : noneValue;

    /// <summary>
    ///     If contains a values executes a mapping function over it, otherwise returns <paramref name="noneValue" />.
    /// </summary>
    public static T2 MapValueOrDefault<T1, T2>(this Maybe<T1> maybe, Func<T1, T2> func, T2 noneValue) =>
        maybe.MatchJust(out T1? value1) ? func(value1) : noneValue;

    /// <summary>
    ///     If contains a values executes a mapping function over it, otherwise returns the value from
    ///     <paramref name="noneValueFactory" />.
    /// </summary>
    public static T2 MapValueOrDefault<T1, T2>(this Maybe<T1> maybe, Func<T1, T2> func, Func<T2> noneValueFactory) =>
        maybe.MatchJust(out T1? value1) ? func(value1) : noneValueFactory();

    /// <summary>
    ///     Returns an empty list when given <see cref="CSharpx.Nothing{T}" /> or a singleton list when given a
    ///     <see cref="CSharpx.Just{T}" />.
    /// </summary>
    public static IEnumerable<T> ToEnumerable<T>(this Maybe<T> maybe)
    {
        if (maybe.MatchJust(out T? value)) return Enumerable.Empty<T>().Concat(new[] { value });
        return Enumerable.Empty<T>();
    }
}
