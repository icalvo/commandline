// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;

namespace CommandLine.Tests.Fakes;

public class Immutable_Simple_Options
{
    public Immutable_Simple_Options(string stringValue, IEnumerable<int> intSequence, bool boolValue, long longValue)
    {
        this.StringValue = stringValue;
        this.IntSequence = intSequence;
        this.BoolValue = boolValue;
        this.LongValue = longValue;
    }

    [Option(HelpText = "Define a string value here.")]
    public string StringValue { get; }

    [Option('i', Min = 3, Max = 4, HelpText = "Define a int sequence here.")]
    public IEnumerable<int> IntSequence { get; }

    [Option('x', HelpText = "Define a boolean or switch value here.")]
    public bool BoolValue { get; }

    [Value(0)] public long LongValue { get; }
}

public class Immutable_Simple_Options_Unsorted
{
    public Immutable_Simple_Options_Unsorted(IEnumerable<int> intSequence, bool boolValue, string stringValue,
        long longValue)
    {
        StringValue = stringValue;
        IntSequence = intSequence;
        BoolValue = boolValue;
        LongValue = longValue;
    }

    [Option(HelpText = "Define a string value here.")]
    public string StringValue { get; }

    [Option('i', Min = 3, Max = 4, HelpText = "Define a int sequence here.")]
    public IEnumerable<int> IntSequence { get; }

    [Option('x', HelpText = "Define a boolean or switch value here.")]
    public bool BoolValue { get; }

    [Value(0)] public long LongValue { get; }
}

public class Immutable_Simple_Options_Invalid_Ctor_Args
{
    public Immutable_Simple_Options_Invalid_Ctor_Args(string stringValue1, IEnumerable<int> intSequence2,
        bool boolValue, long longValue)
    {
        StringValue = stringValue1;
        IntSequence = intSequence2;
        this.BoolValue = boolValue;
        this.LongValue = longValue;
    }

    [Option(HelpText = "Define a string value here.")]
    public string StringValue { get; }

    [Option('i', Min = 3, Max = 4, HelpText = "Define a int sequence here.")]
    public IEnumerable<int> IntSequence { get; }

    [Option('x', HelpText = "Define a boolean or switch value here.")]
    public bool BoolValue { get; }

    [Value(0)] public long LongValue { get; }
}
