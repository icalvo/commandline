using System;
using CommandLine.Tests.Fakes;
using Xunit;

namespace CommandLine.Tests.Unit;

public class BaseAttributeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    public static void Default(object defaultValue)
    {
        var baseAttribute = new TestBaseAttribute();
        baseAttribute.Default = defaultValue;
        Assert.Equal(defaultValue, baseAttribute.Default);
    }

    [Theory]
    [InlineData("", null, "")]
    [InlineData("", typeof(StaticResource), "")]
    [InlineData("Help text", null, "Help text")]
    [InlineData("HelpText", typeof(StaticResource), "Localized HelpText")]
    [InlineData("HelpText", typeof(NonStaticResource), "Localized HelpText")]
    public static void HelpText(string helpText, Type resourceType, string expected)
    {
        var baseAttribute = new TestBaseAttribute();
        baseAttribute.HelpText = helpText;
        baseAttribute.ResourceType = resourceType;

        Assert.Equal(expected, baseAttribute.HelpText);
    }

    [Theory]
    [InlineData("HelpText", typeof(NonStaticResource_WithNonStaticProperty))]
    [InlineData("WriteOnlyText", typeof(NonStaticResource))]
    [InlineData("PrivateOnlyText", typeof(NonStaticResource))]
    [InlineData("HelpText", typeof(InternalResource))]
    public void ThrowsHelpText(string helpText, Type resourceType)
    {
        var baseAttribute = new TestBaseAttribute();
        baseAttribute.HelpText = helpText;
        baseAttribute.ResourceType = resourceType;

        // Verify exception
        Assert.Throws<ArgumentException>(() => baseAttribute.HelpText);
    }


    private class TestBaseAttribute : BaseAttribute
    {
    }
}