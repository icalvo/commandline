﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CommandLine.Infrastructure;
using CSharpx;
using RailwaySharp.ErrorHandling;

namespace CommandLine.Core;

internal static class TypeConverter
{
    public static Maybe<object> ChangeType(IEnumerable<string> values, Type conversionType, bool scalar, bool isFlag,
        CultureInfo conversionCulture, bool ignoreValueCase) =>
        isFlag ? ChangeTypeFlagCounter(values, conversionType, conversionCulture, ignoreValueCase) :
        scalar ? ChangeTypeScalar(values.Last(), conversionType, conversionCulture, ignoreValueCase) :
        ChangeTypeSequence(values, conversionType, conversionCulture, ignoreValueCase);

    private static Maybe<object> ChangeTypeSequence(IEnumerable<string> values, Type conversionType,
        CultureInfo conversionCulture, bool ignoreValueCase)
    {
        Type type = conversionType.UnderlyingSequenceType().FromJustOrFail(
            new InvalidOperationException("Non scalar properties should be sequence of type IEnumerable<T>."));

        var converted = values.Select(value => ChangeTypeScalar(value, type, conversionCulture, ignoreValueCase));

        return converted.Any(a => a.MatchNothing())
            ? Maybe.Nothing<object>()
            : Maybe.Just(converted.Select(c => ((Just<object>)c).Value).ToUntypedArray(type));
    }

    private static Maybe<object> ChangeTypeScalar(string value, Type conversionType, CultureInfo conversionCulture,
        bool ignoreValueCase)
    {
        var result = ChangeTypeScalarImpl(value, conversionType, conversionCulture, ignoreValueCase);
        result.Match(
            (_, _) => { },
            e => e.First().RethrowWhenAbsentIn(
                new[] { typeof(InvalidCastException), typeof(FormatException), typeof(OverflowException) }));
        return result.ToMaybe();
    }

    private static Maybe<object> ChangeTypeFlagCounter(IEnumerable<string> values, Type conversionType,
        CultureInfo conversionCulture, bool ignoreValueCase)
    {
        var converted = values.Select(
            value => ChangeTypeScalar(value, typeof(bool), conversionCulture, ignoreValueCase));
        return converted.Any(maybe => maybe.MatchNothing())
            ? Maybe.Nothing<object>()
            : Maybe.Just((object)converted.Count(value => value.IsJust()));
    }

    private static object ConvertString(string value, Type type, CultureInfo conversionCulture)
    {
        try
        {
            return Convert.ChangeType(value, type, conversionCulture) ??
                   throw new Exception("Unexpected failure in ChangeType");
        }
        catch (InvalidCastException)
        {
            // Required for converting from string to TimeSpan because Convert.ChangeType can't
            return System.ComponentModel.TypeDescriptor.GetConverter(type)
                .ConvertFrom(null, conversionCulture, value) ?? throw new Exception("Error in ConvertFrom");
        }
    }

    private static Result<object, Exception> ChangeTypeScalarImpl(string value, Type conversionType,
        CultureInfo conversionCulture, bool ignoreValueCase)
    {
        var changeType = () =>
        {
            var safeChangeType = () =>
            {
                var isFsOption = ReflectionHelper.IsFSharpOptionType(conversionType);

                var getUnderlyingType = () =>
#if !SKIP_FSHARP
                            isFsOption
                                ? FSharpOptionHelper.GetUnderlyingType(conversionType) :
#endif
                    Nullable.GetUnderlyingType(conversionType);

                Type type = getUnderlyingType() ?? conversionType;

                var withValue = () =>
#if !SKIP_FSHARP
                            isFsOption
                                ? FSharpOptionHelper.Some(type, ConvertString(value, type, conversionCulture)) :
#endif
                    ConvertString(value, type, conversionCulture);
#if !SKIP_FSHARP
                    Func<object> empty = () => isFsOption ? FSharpOptionHelper.None(type) : null;
#else
                Func<object?> empty = () => null;
#endif

                return withValue();
            };

            return value.IsBooleanString() && conversionType == typeof(bool) ? value.ToBoolean() :
                conversionType.GetTypeInfo().IsEnum ? value.ToEnum(conversionType, ignoreValueCase) : safeChangeType();
        };

        var makeType = () =>
        {
            try
            {
                ConstructorInfo ctor = conversionType.GetTypeInfo().GetConstructor(new[] { typeof(string) }) ??
                                       throw new Exception();
                return ctor.Invoke(new object[] { value });
            }
            catch (Exception)
            {
                throw new FormatException("Destination conversion type must have a constructor that accepts a string.");
            }
        };

        if (conversionType.IsCustomStruct()) return Result.Try(makeType);
        return Result.Try(
            conversionType.IsPrimitiveEx() || ReflectionHelper.IsFSharpOptionType(conversionType)
                ? changeType
                : makeType);
    }

    private static object ToEnum(this string value, Type conversionType, bool ignoreValueCase)
    {
        object parsedValue;
        try
        {
            parsedValue = Enum.Parse(conversionType, value, ignoreValueCase);
        }
        catch (ArgumentException)
        {
            throw new FormatException();
        }

        if (IsDefinedEx(parsedValue)) return parsedValue;
        throw new FormatException();
    }

    private static bool IsDefinedEx(object enumValue)
    {
        var s = enumValue.ToString();
        return s is not { Length: >= 1 } || (!char.IsDigit(s[0]) && s[0] != '-');
    }
}
