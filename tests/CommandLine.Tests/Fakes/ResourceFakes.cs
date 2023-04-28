namespace CommandLine.Tests.Fakes;

public static class StaticResource
{
    public static string HelpText => "Localized HelpText";
}

public class NonStaticResource
{
    public static string HelpText => "Localized HelpText";

    public static string WriteOnlyText
    {
        set => value?.ToString();
    }

    private static string PrivateHelpText => "Localized HelpText";
}

public class NonStaticResource_WithNonStaticProperty
{
    public string HelpText => "Localized HelpText";
}

internal class InternalResource
{
    public static string HelpText => "Localized HelpText";
}