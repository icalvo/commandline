// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;

namespace CommandLine.Tests.Fakes;

[Verb("add", HelpText = "Add file contents to the index.")]
internal class Immutable_Add_Verb
{
    public Immutable_Add_Verb(bool patch, bool force, string fileName)
    {
        this.Patch = patch;
        this.Force = force;
        this.FileName = fileName;
    }

    [Option(
        'p',
        "patch",
        SetName = "mode",
        HelpText =
            "Interactively choose hunks of patch between the index and the work tree and add them to the index.")]
    public bool Patch { get; }

    [Option('f', "force", SetName = "mode", HelpText = "Allow adding otherwise ignored files.")]
    public bool Force { get; }

    [Value(0)] public string FileName { get; }
}

[Verb("commit", HelpText = "Record changes to the repository.")]
internal class Immutable_Commit_Verb
{
    public Immutable_Commit_Verb(bool patch, bool amend)
    {
        this.Patch = patch;
        this.Amend = amend;
    }

    [Option('p', "patch", HelpText = "Use the interactive patch selection interface to chose which changes to commit.")]
    public bool Patch { get; }

    [Option("amend", HelpText = "Used to amend the tip of the current branch.")]
    public bool Amend { get; }
}

[Verb("clone", HelpText = "Clone a repository into a new directory.")]
internal class Immutable_Clone_Verb
{
    public Immutable_Clone_Verb(bool noHardLinks, bool quiet, IEnumerable<string> urls)
    {
        this.NoHardLinks = noHardLinks;
        this.Quiet = quiet;
        this.Urls = urls;
    }

    [Option(
        "no-hardlinks",
        HelpText = "Optimize the cloning process from a repository on a local filesystem by copying files.")]
    public bool NoHardLinks { get; }

    [Option('q', "quiet", HelpText = "Suppress summary message.")]
    public bool Quiet { get; }

    [Value(0)] public IEnumerable<string> Urls { get; }
}
