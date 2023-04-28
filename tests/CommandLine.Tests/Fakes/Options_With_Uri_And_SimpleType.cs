using System;

namespace CommandLine.Tests.Fakes
{
    class MySimpleType
    {
        private readonly string value;

        public MySimpleType(string value)
        {
            this.value = value;
        }

        public string Value
        {
            get { return value; }
        }
    }

    internal class Options_With_Uri_And_SimpleType
    {
        [Option] public Uri EndPoint { get; set; }

        [Value(0)] public MySimpleType MyValue { get; set; }
}
