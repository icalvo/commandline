﻿namespace CommandLine.Tests.Fakes;

internal class Options_With_Custom_Version_Option : Simple_Options
{
    [Option('v', "version")] public bool MyVersion { get; set; }
}
