// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace CommandLine.Infrastructure;

internal static class StringExtensions
{
    public static string ToOneCharString(this char c) => new(c, 1);

    public static string? ToStringInvariant<T>(this T value) => Convert.ToString(value, CultureInfo.InvariantCulture);

    [StringFormatMethod("value")]
    public static string? ToStringLocal<T>(this T value) => Convert.ToString(value, CultureInfo.CurrentCulture);

    [StringFormatMethod("value")]
    public static string FormatInvariant(this string value, params object[] arguments) =>
        string.Format(CultureInfo.InvariantCulture, value, arguments);

    [StringFormatMethod("value")]
    public static string FormatLocal(this string value, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, value, arguments);

    public static string Spaces(this int value) => new(' ', value);

    public static bool EqualsOrdinal(this string strA, string strB) => string.CompareOrdinal(strA, strB) == 0;

    public static bool EqualsOrdinalIgnoreCase(this string strA, string strB) =>
        string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0;

    public static int SafeLength(this string value) => value == null ? 0 : value.Length;

    public static string JoinTo(this string value, params string[] others)
    {
        var builder = new StringBuilder(value);
        foreach (var v in others) builder.Append(v);
        return builder.ToString();
    }

    public static bool IsBooleanString(this string value) =>
        value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("false", StringComparison.OrdinalIgnoreCase);

    public static bool ToBoolean(this string value) => value.Equals("true", StringComparison.OrdinalIgnoreCase);

    public static bool ToBooleanLoose(this string? value)
    {
        if (string.IsNullOrEmpty(value) || value == "0" || value.Equals("f", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("n", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("off", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("false", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }
}
