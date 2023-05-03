// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CommandLine.Infrastructure;
using SharpX;

namespace CommandLine.Core;

internal static class InstanceBuilder
{
    public static ParserResult<T> Build<T>(Maybe<Func<T>> factory,
        Func<IEnumerable<string>, IEnumerable<OptionSpecification>, Result<IEnumerable<Token>, Error>> tokenizer,
        IEnumerable<string> arguments,
        StringComparer nameComparer,
        bool ignoreValueCase,
        CultureInfo parsingCulture,
        bool autoHelp,
        bool autoVersion,
        IEnumerable<ErrorType> nonFatalErrors) where T : notnull =>
        Build(
            factory,
            tokenizer,
            arguments,
            nameComparer,
            ignoreValueCase,
            parsingCulture,
            autoHelp,
            autoVersion,
            false,
            nonFatalErrors);

    public static ParserResult<T> Build<T>(Maybe<Func<T>> factory,
        Func<IEnumerable<string>, IEnumerable<OptionSpecification>, Result<IEnumerable<Token>, Error>> tokenizer,
        IEnumerable<string> arguments,
        StringComparer nameComparer,
        bool ignoreValueCase,
        CultureInfo parsingCulture,
        bool autoHelp,
        bool autoVersion,
        bool allowMultiInstance,
        IEnumerable<ErrorType> nonFatalErrors)
        where T : notnull
    {
        Type typeInfo = factory.MapValueOrDefault(f => f().GetType(), typeof(T));

        var specProps = typeInfo.GetSpecifications(
            pi => SpecificationProperty.Create(Specification.FromProperty(pi), pi, Maybe.Nothing<object>())).Memoize();

        var specs = from pt in specProps select pt.Specification;

        var optionSpecs = specs.ThrowingValidate(SpecificationGuards.Lookup).OfType<OptionSpecification>().Memoize();

        T MakeDefault() =>
            typeof(T).IsMutable()
                ? factory.MapValueOrDefault(f => f(), Activator.CreateInstance<T>)
                : ReflectionHelper.CreateDefaultImmutableInstance<T>((from p in specProps select p.Property).ToArray());

        ParserResult<T> NotParsed(IEnumerable<Error> errs) =>
            new NotParsed<T>(MakeDefault().GetType().ToTypeInfo(), errs);

        var argumentsList = arguments.Memoize();

        ParserResult<T> BuildUp()
        {
            var tokenizerResult = tokenizer(argumentsList, optionSpecs);

            var tokens = tokenizerResult.SucceededWith().Memoize();

            var partitions = TokenPartitioner.Partition(
                tokens,
                name => TypeLookup.FindTypeDescriptorAndSibling(name, optionSpecs, nameComparer));
            var optionsPartition = partitions.Item1.Memoize();
            var valuesPartition = partitions.Item2.Memoize();
            var errorsPartition = partitions.Item3.Memoize();

            Maybe<object> OptionsConverter(
                IEnumerable<string> values,
                Type type,
                bool isScalar,
                bool isFlag) =>
                TypeConverter.ChangeType(values, type, isScalar, isFlag, parsingCulture, ignoreValueCase);

            var optionSpecPropsResult = OptionMapper.MapValues(
                from pt in specProps
                where pt.Specification.IsOption()
                select pt,
                optionsPartition,
                OptionsConverter,
                nameComparer);

            Maybe<object> ValuesConverter(
                IEnumerable<string> values,
                Type type,
                bool isScalar) =>
                TypeConverter.ChangeType(values, type, isScalar, false, parsingCulture, ignoreValueCase);

            var valueSpecPropsResult = ValueMapper.MapValues(
                from pt in specProps
                where pt.Specification.IsValue()
                orderby ((ValueSpecification)pt.Specification).Index
                select pt,
                valuesPartition,
                ValuesConverter);

            var missingValueErrors = from token in errorsPartition
                select new MissingValueOptionError(
                    optionSpecs.Single(o => token.Text.MatchName(o.ShortName, o.LongNames, nameComparer))
                        .FromOptionSpecification());

            var specPropsWithValue = optionSpecPropsResult.SucceededWith().Concat(valueSpecPropsResult.SucceededWith())
                .Memoize();

            var setPropertyErrors = new List<Error>();

            //build the instance, determining if the type is mutable or not.
            T instance = typeInfo.IsMutable()
                ? BuildMutable(factory, specPropsWithValue, setPropertyErrors)
                : BuildImmutable<T>(typeInfo, specProps, specPropsWithValue);

            var validationErrors =
                specPropsWithValue.Validate(SpecificationPropertyRules.Lookup(tokens, allowMultiInstance));

            var allErrors = tokenizerResult.SuccessMessages().Concat(missingValueErrors)
                .Concat(optionSpecPropsResult.SuccessMessages()).Concat(valueSpecPropsResult.SuccessMessages())
                .Concat(validationErrors).Concat(setPropertyErrors).Memoize();

            var warnings = from e in allErrors where nonFatalErrors.Contains(e.Tag) select e;

            return allErrors.Except(warnings).ToParserResult(instance);
        }

        var preprocessorErrors =
            (argumentsList.Any()
                ? argumentsList.Preprocess(PreprocessorGuards.Lookup(nameComparer, autoHelp, autoVersion))
                : Enumerable.Empty<Error>()).Memoize();

        var result = argumentsList.Any()
            ? preprocessorErrors.Any() ? NotParsed(preprocessorErrors) : BuildUp()
            : BuildUp();

        return result;
    }

    private static T BuildMutable<T>(
        Maybe<Func<T>> factory,
        IEnumerable<SpecificationProperty> specPropsWithValue,
        List<Error> setPropertyErrors)
    {
        T mutable = factory.MapValueOrDefault(f => f(), Activator.CreateInstance<T>);

        var specPropsArray = specPropsWithValue as SpecificationProperty[] ?? specPropsWithValue.ToArray();
        setPropertyErrors.AddRange(
            mutable.SetProperties(specPropsArray, sp => sp.Value.IsJust(), sp => sp.Value.FromJustOrFail()));

        setPropertyErrors.AddRange(
            mutable.SetProperties(
                specPropsArray,
                sp => sp.Value.IsNothing() && sp.Specification.DefaultValue.IsJust(),
                sp => sp.Specification.DefaultValue.FromJustOrFail()));

        setPropertyErrors.AddRange(
            mutable.SetProperties(
                specPropsArray,
                sp => sp.Value.IsNothing() && sp.Specification.TargetType == TargetType.Sequence &&
                      sp.Specification.DefaultValue.MatchNothing(),
                sp => sp.Property.PropertyType.UnderlyingSequenceType().FromJustOrFail().CreateEmptyArray()));

        return mutable;
    }

    private static T BuildImmutable<T>(
        Type typeInfo,
        IEnumerable<SpecificationProperty> specProps,
        IEnumerable<SpecificationProperty> specPropsWithValue)
    {
        var specPropsArray = specProps as SpecificationProperty[] ?? specProps.ToArray();
        ConstructorInfo ctor = ReflectionHelper.GetMatchingConstructor(
            typeInfo,
            specPropsArray.Select(sp => sp.Property).ToArray());

        var values = from parameter in ctor.GetParameters()
            join sp in specPropsWithValue on parameter.Name?.ToLower() equals sp.Property.Name.ToLower() into spv
            from sp in spv.DefaultIfEmpty()
            select GetValueForParameter(sp, parameter, specPropsArray);

        var immutable = (T)ctor.Invoke(values.ToArray());

        return immutable;

        static object GetValueForParameter(
            SpecificationProperty? specPropWithValue,
            ParameterInfo parameter,
            SpecificationProperty[] specPropsArray) =>
            specPropWithValue == null
                ? GetDefaultValue(specPropsArray, parameter)
                : GetValueFromGiven(specPropWithValue);

        static object GetDefaultValue(
            SpecificationProperty[] specificationProperties,
            ParameterInfo parameter)
        {
            return specificationProperties.First(
                    s => string.Equals(s.Property.Name, parameter.Name, StringComparison.CurrentCultureIgnoreCase))
                .Property
                .PropertyType.GetDefaultValue();
        }

        static object GetValueFromGiven(SpecificationProperty specPropWithValue) =>
            specPropWithValue.Value.GetValueOrDefault(
                specPropWithValue.Specification.DefaultValue.GetValueOrDefault(
                    specPropWithValue.Specification.ConversionType.CreateDefaultForImmutable()));
    }

}
