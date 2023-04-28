using System.IO;
using FluentAssertions;
using Xunit;

namespace CommandLine.Tests.Unit;

public class ParserSettingsTests
{
    public class DisposeTrackingStringWriter : StringWriter
    {
        public DisposeTrackingStringWriter() => Disposed = false;

        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    [Fact]
    public void Disposal_does_not_dispose_HelpWriter()
    {
        using (var textWriter = new DisposeTrackingStringWriter())
        {
            using (var parserSettings = new ParserSettings())
            {
                parserSettings.HelpWriter = textWriter;
            }

            textWriter.Disposed.Should().BeFalse("not disposed");
        }
    }
}