// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using SharpX;

namespace CommandLine.Core;

internal class TypeDescriptor
{
    private TypeDescriptor(TargetType targetType, Maybe<int> maxItems, Maybe<TypeDescriptor>? nextValue = null)
    {
        this.TargetType = targetType;
        this.MaxItems = maxItems;
        this.NextValue = nextValue ?? Maybe.Nothing<TypeDescriptor>();
    }

    public TargetType TargetType { get; }

    public Maybe<int> MaxItems { get; }

    public Maybe<TypeDescriptor> NextValue { get; }

    public static TypeDescriptor Create(TargetType tag, Maybe<int> maximumItems, TypeDescriptor? next = null)
    {
        if (maximumItems == null) throw new ArgumentNullException("maximumItems");

        return new TypeDescriptor(tag, maximumItems, next.AsMaybe());
    }
}

internal static class TypeDescriptorExtensions
{
    public static TypeDescriptor WithNextValue(this TypeDescriptor descriptor, Maybe<TypeDescriptor> nextValue) =>
        TypeDescriptor.Create(descriptor.TargetType, descriptor.MaxItems, nextValue.GetValueOrNull());
}
