using System;

namespace CommandLine.Tests.Fakes;

internal class Options_With_Property_Throwing_Exception
{
    private string optValue;

    [Option('e')]
    public string OptValue
    {
        get => optValue;
        set
        {
            if (value != "good")
                throw new ArgumentException("Invalid value, only accept 'good' value");

            optValue = value;
        }
    }
}
