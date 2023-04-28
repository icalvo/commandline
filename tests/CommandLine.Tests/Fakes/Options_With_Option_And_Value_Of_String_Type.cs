// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

namespace CommandLine.Tests.Fakes;

internal class Options_With_Option_And_Value_Of_String_Type
{
    [Option('o', "opt")] public string OptValue { get; set; }

    [Value(0)] public string PosValue { get; set; }
}
