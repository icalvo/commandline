// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine.Core;
using CSharpx;

namespace CommandLine.Infrastructure;

internal static class ReflectionHelper
{
    /// <summary>
    ///     Per thread assembly attribute overrides for testing.
    /// </summary>
    [ThreadStatic] private static IDictionary<Type, Attribute>? _overrides;

    /// <summary>
    ///     Assembly attribute overrides for testing.
    /// </summary>
    /// <remarks>
    ///     The implementation will fail if two or more attributes of the same type
    ///     are included in <paramref name="overrides" />.
    /// </remarks>
    /// <param name="overrides">
    ///     Attributes that replace the existing assembly attributes or null,
    ///     to clear any testing attributes.
    /// </param>
    public static void SetAttributeOverride(IEnumerable<Attribute> overrides)
    {
        if (overrides != null)
            _overrides = overrides.ToDictionary(attr => attr.GetType(), attr => attr);
        else
            _overrides = null;
    }

    public static Maybe<TAttribute> GetAttribute<TAttribute>() where TAttribute : Attribute
    {
        // Test support
        if (_overrides != null)
            return _overrides.ContainsKey(typeof(TAttribute))
                ? Maybe.Just((TAttribute)_overrides[typeof(TAttribute)])
                : Maybe.Nothing<TAttribute>();

        Assembly assembly = GetExecutingOrEntryAssembly();

#if NET40
            var attributes = assembly.GetCustomAttributes(typeof(TAttribute), false);
#else
        var attributes = assembly.GetCustomAttributes<TAttribute>().ToArray();
#endif

        return attributes.Length > 0 ? Maybe.Just(attributes[0]) : Maybe.Nothing<TAttribute>();
    }

    public static string? GetAssemblyName()
    {
        Assembly assembly = GetExecutingOrEntryAssembly();
        return assembly.GetName().Name;
    }

    public static string? GetAssemblyVersion()
    {
        Assembly assembly = GetExecutingOrEntryAssembly();
        return assembly.GetName().Version?.ToStringInvariant();
    }

    public static bool IsFSharpOptionType(Type type) =>
        type.FullName?.StartsWith("Microsoft.FSharp.Core.FSharpOption`1", StringComparison.Ordinal) ??
        throw new InvalidOperationException("The type must have full name");

    public static T CreateDefaultImmutableInstance<T>(PropertyInfo[] constructorTypes)
    {
        Type t = typeof(T);
        return (T)CreateDefaultImmutableInstance(t, constructorTypes);
    }

    public static IEnumerable<(PropertyInfo, Maybe<ParameterInfo>)> Matches(MethodBase c,
        PropertyInfo[] constructorTypes)
    {
        var parameters = c.GetParameters();
        if (parameters.Length != constructorTypes.Length)
            return Enumerable.Empty<(PropertyInfo, Maybe<ParameterInfo>)>();

        var parametersDic = parameters.ToDictionary(
            x => x.Name ?? throw new InvalidOperationException("Cannot have nameless parameter"),
            StringComparer.OrdinalIgnoreCase);

        return constructorTypes.Select(ct => (ct, parametersDic.TryGetValue(ct.Name)));
    }

    public static bool IsMatch(MethodBase c, PropertyInfo[] constructorTypes)
    {
        var matches = Matches(c, constructorTypes);
        return matches.Any() && matches.All(x => x.Item2.IsJust());
    }

    public static object CreateDefaultImmutableInstance(Type type, PropertyInfo[] constructorTypes)
    {
        ConstructorInfo ctor = GetMatchingConstructor(type, constructorTypes);

        var values = (from prms in ctor.GetParameters() select prms.ParameterType.CreateDefaultForImmutable())
            .ToArray();
        return ctor.Invoke(values);
    }

    private static string GetCSharpRepresentation(Type t)
    {
        if (!t.IsGenericType) return t.Name;
        var genericArgs = t.GetGenericArguments().ToList();
        return GetCSharpRepresentation(t, genericArgs);
    }

    private static string GetCSharpRepresentation(Type t, IList<Type> availableArguments)
    {
        if (!t.IsGenericType) return t.Name;
        var value = t.Name;
        if (value.IndexOf("`", StringComparison.Ordinal) > -1)
            value = value.Substring(0, value.IndexOf("`", StringComparison.Ordinal));

        if (t.DeclaringType != null)
            // This is a nested type, build the nesting type first
            value = $"{GetCSharpRepresentation(t.DeclaringType, availableArguments)}+{value}";

        // Build the type arguments (if any)
        var argString = "";
        var thisTypeArgs = t.GetGenericArguments();
        for (var i = 0; i < thisTypeArgs.Length && availableArguments.Count > 0; i++)
        {
            if (i != 0) argString += ", ";

            argString += GetCSharpRepresentation(availableArguments[0]);
            availableArguments.RemoveAt(0);
        }

        // If there are type arguments, add them with < >
        if (argString.Length > 0) value += $"<{argString}>";

        return value;
    }

    public static ConstructorInfo GetMatchingConstructor(Type type, PropertyInfo[] constructorTypes)
    {
        return type.GetTypeInfo().GetConstructors().FirstOrNothing(ci => IsMatch(ci, constructorTypes)).FromJustOrFail(
            () =>
            {
                var ctorArgs = constructorTypes.Select(
                    x => $"{GetCSharpRepresentation(x.PropertyType)} {char.ToLowerInvariant(x.Name[0])}{x.Name[1..]}");
                var ctorSyntax = string.Join(", ", ctorArgs);
                var msg =
                    $"Type {type.FullName} appears to be Immutable with invalid constructor. Check that constructor parameters have the following names and types (in any order): {ctorSyntax}";
                return new InvalidOperationException(msg);
            });
    }

    private static Assembly GetExecutingOrEntryAssembly() =>
        //resolve issues of null EntryAssembly in Xunit Test #392,424,389
        //return Assembly.GetEntryAssembly();
        Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

    public static IEnumerable<string> GetNamesOfEnum(Type t)
    {
        if (t.IsEnum)
            return Enum.GetNames(t);
        Type? u = Nullable.GetUnderlyingType(t);
        return u is { IsEnum: true } ? Enum.GetNames(u) : Enumerable.Empty<string>();
    }
}
