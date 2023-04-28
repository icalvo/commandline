using System;
using System.Collections.Generic;
using Xunit;

namespace CommandLine.Tests.Unit;

//MailAndSmsWarningSenderTests
public class Issue424Tests
{
    private readonly MailAndSmsWarningSender _sut;

    public Issue424Tests() => _sut = new MailAndSmsWarningSender();

    [Fact]
    public void SendSmsOnWarning()
    {
        //Arrange
        void Action() => _sut.ParseArgumentsAndRun(new[] { "--task", "MailAndSmsWarningSender", "--test", "hejtest" });
        // Act & Assert
        Assert.Throws<NotImplementedException>(Action);
    }
}

public class MailAndSmsWarningSender
{
    internal class Options
    {
        [Option("task")] public string Task { get; set; }
    }

    public void ParseArgumentsAndRun(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(ExecuteTaskWithOptions).WithNotParsed(HandleParseError);
    }

    private void HandleParseError(IEnumerable<Error> errs)
    {
        throw new NotImplementedException();
    }

    private void ExecuteTaskWithOptions(Options opts)
    {
        Console.WriteLine("Executing");
    }

}