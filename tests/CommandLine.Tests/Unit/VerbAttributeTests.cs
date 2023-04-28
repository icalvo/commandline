using System;
using CommandLine.Tests.Fakes;
using Xunit;

namespace CommandLine.Tests;

//Test localization of VerbAttribute
public class VerbAttributeTests
{
    [Theory]
    [InlineData("", null, "")]
    [InlineData("", typeof(StaticResource), "")]
    [InlineData("Help text", null, "Help text")]
    [InlineData("HelpText", typeof(StaticResource), "Localized HelpText")]
    [InlineData("HelpText", typeof(NonStaticResource), "Localized HelpText")]
    public static void VerbHelpText(string helpText, Type resourceType, string expected)
    {
        var verbAttribute = new TestVerbAttribute { HelpText = helpText, ResourceType = resourceType };

        Assert.Equal(expected, verbAttribute.HelpText);
    }

    [Theory]
    [InlineData("HelpText", typeof(NonStaticResource_WithNonStaticProperty))]
    [InlineData("WriteOnlyText", typeof(NonStaticResource))]
    [InlineData("PrivateOnlyText", typeof(NonStaticResource))]
    [InlineData("HelpText", typeof(InternalResource))]
    public void ThrowsHelpText(string helpText, Type resourceType)
    {
        var verbAttribute = new TestVerbAttribute { HelpText = helpText, ResourceType = resourceType };

        // Verify exception
        Assert.Throws<ArgumentException>(() => verbAttribute.HelpText);
    }

    private class TestVerbAttribute : VerbAttribute
    {
        public TestVerbAttribute() : base("verb")
        {
            // Do nothing
        }
    }
}