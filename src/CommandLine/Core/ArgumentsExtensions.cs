// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine.Core;

internal static class ArgumentsExtensions
{
    public static IEnumerable<Error> Preprocess(
        this IEnumerable<string> arguments,
        IEnumerable<Func<IEnumerable<string>, IEnumerable<Error>>> preprocessorLookup)
    {
        var argsArray = arguments.Memoize();
        var lookupArray = preprocessorLookup.Memoize();
        return lookupArray.TryHead().MapValueOrDefault(
            func =>
            {
                var errors = func(argsArray).Memoize();
                return errors.Any() ? errors : argsArray.Preprocess(lookupArray.TailNoFail());
            },
            Enumerable.Empty<Error>());
    }
}
