using System;

namespace CommandLine.Tests.Fakes;

internal class MySimpleType
{
    public MySimpleType(string value) => this.Value = value;

    public string Value { get; }
}

internal class Options_With_Uri_And_SimpleType
{
    [Option] public Uri EndPoint { get; set; }

    [Value(0)] public MySimpleType MyValue { get; set; }
}
