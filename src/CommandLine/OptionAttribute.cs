﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using CommandLine.Infrastructure;

namespace CommandLine;

/// <summary>
///     Models an option specification.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionAttribute : BaseAttribute
{
    private string setName;

    private OptionAttribute(string shortName, string[] longNames)
    {
        if (shortName == null) throw new ArgumentNullException("shortName");
        if (longNames == null) throw new ArgumentNullException("longNames");

        ShortName = shortName;
        LongNames = longNames;
        setName = string.Empty;
        Separator = '\0';
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLine.OptionAttribute" /> class.
    ///     The default long name will be inferred from target property.
    /// </summary>
    public OptionAttribute() : this(string.Empty, new string[0])
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLine.OptionAttribute" /> class.
    /// </summary>
    /// <param name="longName">The long name of the option.</param>
    public OptionAttribute(string longName) : this(string.Empty, new[] { longName })
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLine.OptionAttribute" /> class.
    /// </summary>
    /// <param name="longNames">The long name of the option.</param>
    public OptionAttribute(string[] longNames) : this(string.Empty, longNames)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLine.OptionAttribute" /> class.
    /// </summary>
    /// <param name="shortName">The short name of the option.</param>
    /// <param name="longName">The long name of the option or null if not used.</param>
    public OptionAttribute(char shortName, string longName) : this(shortName.ToOneCharString(), new[] { longName })
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLine.OptionAttribute" /> class.
    /// </summary>
    /// <param name="shortName">The short name of the option.</param>
    /// <param name="longNames">The long name of the option or null if not used.</param>
    public OptionAttribute(char shortName, string[] longNames) : this(shortName.ToOneCharString(), longNames)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLine.OptionAttribute" /> class.
    /// </summary>
    /// <param name="shortName">The short name of the option..</param>
    public OptionAttribute(char shortName) : this(shortName.ToOneCharString(), new string[0])
    {
    }

    /// <summary>
    ///     Gets long name of this command line option. This name is usually a single english word.
    /// </summary>
    public string[] LongNames { get; }

    /// <summary>
    ///     Gets a short name of this command line option, made of one character.
    /// </summary>
    public string ShortName { get; }

    /// <summary>
    ///     Gets or sets the option's mutually exclusive set name.
    /// </summary>
    public string SetName
    {
        get => setName;
        set
        {
            if (value == null) throw new ArgumentNullException("value");

            setName = value;
        }
    }

    /// <summary>
    ///     If true, this is an int option that counts how many times a flag was set (e.g. "-v -v -v" or "-vvv" would return
    ///     3).
    ///     The property must be of type int (signed 32-bit integer).
    /// </summary>
    public bool FlagCounter { get; set; }

    /// <summary>
    ///     When applying attribute to <see cref="System.Collections.Generic.IEnumerable{T}" /> target properties,
    ///     it allows you to split an argument and consume its content as a sequence.
    /// </summary>
    public char Separator { get; set; }

    /// <summary>
    ///     Gets or sets the option group name. When one or more options are grouped, at least one of them should have value.
    ///     Required rules are ignored.
    /// </summary>
    public string Group { get; set; } = string.Empty;
}
