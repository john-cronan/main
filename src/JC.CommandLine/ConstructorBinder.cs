using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace JC.CommandLine
{
    internal class ConstructorBinder : IObjectBinder
    {
        private const string UnnamedValuesParameterName = "unnamedValues";
        private const string LeadingUnnamedValuesParameterName = "leadingUnnamedValues";
        private const string TrailingUnnamedValuesParameterName = "trailingUnnamedValues";
        private const string ParseWarningsParameterName = "parseWarnings";

        private readonly ArgumentValueConverter _converter;

        public ConstructorBinder()
            : this(new Filesystem())
        {
        }

        public ConstructorBinder(IFilesystem filesystem)
        {
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _converter = new ArgumentValueConverter(filesystem);
        }

        T IObjectBinder.CreateObject<T>(ActualModelResolution actualModelResolution)
        {
            Guard.IsNotNull(actualModelResolution, nameof(actualModelResolution));

            var selectedConstructor =
                typeof(T).GetConstructors()
                    .Where(ctor => ctor.IsPublic)
                    .Where(ctor => ConstructorIsEligible(actualModelResolution.Model, ctor))
                    .OrderByDescending(ctor => ctor.GetParameters().Length)
                    .FirstOrDefault();
            if (selectedConstructor == null)
            {
                var typeName = ReflectionUtility.GetTypeName(typeof(T));
                var msg = $"A suitable constructor on type '{typeName}' could not be found";
                throw new CommandLineBindingException(msg);
            }
            var returnValue = InvokeConstructor<T>(actualModelResolution, selectedConstructor);
            return (T)returnValue;
        }

        private bool ConstructorIsEligible(ParseModel model, ConstructorInfo ctor)
        {
            //
            //  A ctor is "eligible" if all of its parameters identify arguments in the
            //  model (whether those arguments are actually specified on the parsed 
            //  command line or not). Note: no-argument ctors are never eligible.
            if (!ctor.GetParameters().Any())
            {
                return false;
            }

            var matchingParameters =
                from parameter in ctor.GetParameters()
                where ParameterNameIs(parameter, UnnamedValuesParameterName)
                || ParameterNameIs(parameter, LeadingUnnamedValuesParameterName)
                || ParameterNameIs(parameter, TrailingUnnamedValuesParameterName)
                || ParameterNameIs(parameter, ParseWarningsParameterName)
                || model.Arguments.Any(a => NameMatching.IsMatch(parameter.Name, a.Names, model.NameMatching, StringComparison.InvariantCultureIgnoreCase))
                select parameter;

            return matchingParameters.Count() == ctor.GetParameters().Count();
        }

        private T InvokeConstructor<T>(ActualModelResolution actualModelResolution,
            ConstructorInfo ctor)
        {
            var args = new List<object>();
            foreach (var parameter in ctor.GetParameters())
            {
                object value = null;
                switch (TryGetParameterValue(actualModelResolution, parameter, out value))
                {
                    case true:
                        args.Add(value);
                        break;
                    case false when parameter.HasDefaultValue:
                        args.Add(parameter.DefaultValue);
                        break;
                    case false when parameter.ParameterType.IsValueType:
                        var defaultValue = ReflectionUtility.DefaultValue(parameter.ParameterType);
                        args.Add(defaultValue);
                        break;
                    default:
                        args.Add(null);
                        break;
                }
            }
            var instance = (T)ctor.Invoke(args.ToArray());
            return (T)instance;
        }

        private bool TryGetParameterValue(ActualModelResolution actualModelResolution,
            ParameterInfo parameter, out object value)
        {
            if (TryGetSpecialParameterValue(actualModelResolution, parameter, out value))
            {
                return true;
            }
            if (TryGetArgumentParameterValue(actualModelResolution, parameter, out value))
            {
                return true;
            }
            return false;
        }


        private bool TryGetSpecialParameterValue(ActualModelResolution actualModelResolution,
            ParameterInfo parameter, out object value)
        {
            if (ParameterNameIs(parameter, UnnamedValuesParameterName))
            {
                return TryGetUnnamedValuesParameterValue(actualModelResolution, 
                    parameter, out value);
            }

            if (ParameterNameIs(parameter, TrailingUnnamedValuesParameterName))
            {
                return TryGetTrailingUnnamedValuesParameterValue(actualModelResolution,
                parameter, out value);
            }

            if (ParameterNameIs(parameter, LeadingUnnamedValuesParameterName))
            {
                return TryGetLeadingUnnamedValuesParameterValue(actualModelResolution, 
                    parameter, out value);
            }

            if (ParameterNameIs(parameter, TrailingUnnamedValuesParameterName))
            {
                return TryGetTrailingUnnamedValuesParameterValue(actualModelResolution, 
                    parameter, out value);
            }

            if (ParameterNameIs(parameter, ParseWarningsParameterName))
            {
                return TryGetParseWarningsParameterValue(actualModelResolution, 
                    parameter, out value);
            }

            value = null;
            return false;
        }

        private bool TryGetUnnamedValuesParameterValue(ActualModelResolution actualModelResolution,
            ParameterInfo parameter, out object value)
        {
            value = null;
            var values =
                actualModelResolution.Actuals
                    .Where(a => a.IsUnnamedValuesNodeGroup)
                    .SelectMany(a => a.ValueNodes)
                    .Select(n => n.Text);
            //
            //TODO: Do this the non-lazy way.
            object candidate = values.ToImmutableArray();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            candidate = values.ToArray();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            candidate = values.ToList();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            return false;
        }

        private bool TryGetLeadingUnnamedValuesParameterValue(ActualModelResolution actualModelResolution,
            ParameterInfo parameter, out object value)
        {
            value = null;
            var leadingValues =
                CommandLineNodeGroup.GetLeadingUnnamedValues(actualModelResolution.Actuals)
                    .Select(n => n.Text);

            //
            //TODO: Do this the non-lazy way.
            object candidate = leadingValues.ToImmutableArray();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            candidate = leadingValues.ToArray();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            candidate = leadingValues.ToList();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            return false;
        }

        private bool TryGetTrailingUnnamedValuesParameterValue(ActualModelResolution actualModelResolution,
            ParameterInfo parameter, out object value)
        {
            value = null;
            var leadingValues =
                CommandLineNodeGroup.GetTrailingUnnamedValues(actualModelResolution.Actuals)
                    .Select(n => n.Text);

            //
            //TODO: Do this the non-lazy way.
            object candidate = leadingValues.ToImmutableArray();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            candidate = leadingValues.ToArray();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            candidate = leadingValues.ToList();
            if (IsCompatible(parameter.ParameterType, candidate.GetType()))
            {
                value = candidate;
                return true;
            }
            return false;
        }

        private bool TryGetParseWarningsParameterValue(ActualModelResolution actualModelResolution,
            ParameterInfo parameter, out object value)
        {
            value = null;
            return false;
        }

        private bool TryGetArgumentParameterValue(ActualModelResolution actualModelResolution,
            ParameterInfo parameter, out object value)
        {
            value = null;
            var targetType = new TargetType(parameter.ParameterType);
            var parameterName = parameter.Name;
            var matches =
                from m in actualModelResolution.Matches
                where NameMatching.IsMatch(parameterName, m.Model.Names,
                    actualModelResolution.Model.NameMatching, actualModelResolution.Model.StringComparisons)
                select m;
            if (!matches.Any())
            {
                return false;
            }
            if (matches.Count() > 1)
            {
                return false;
            }
            var match = matches.Single();
            var actualValues = match.Actual.ValueNodes.Select(n => n.Text);
            var convertedValues = actualValues
                    .SelectMany(v => _converter.Convert(v, targetType, match.Model.Flags))
                    .ToArray();
            if (targetType.IsVectorType)
            {
                if (!ToVectorParameterValue(convertedValues, targetType, parameter.ParameterType, out value))
                {
                    return false;
                }
            }
            else
            {
                if (!ToScalarParamaterValue(convertedValues, targetType, out value))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ToVectorParameterValue(IEnumerable<object> convertedValues, TargetType targetType,
            Type parameterType, out object parameterValue)
        {

            if (ToImmutableArrayParameterValue(convertedValues, targetType, parameterType, out parameterValue))
            {
                return true;
            }
            if (ToArrayParameterValue(convertedValues, targetType, parameterType, out parameterValue))
            {
                return true;
            }
            if (ToListParameterValue(convertedValues, targetType, parameterType, out parameterValue))
            {
                return true;
            }
            parameterValue = null;
            return false;
        }

        private bool ToImmutableArrayParameterValue(IEnumerable<object> convertedValues,
            TargetType targetType, Type parameterType, out object parameterValue)
        {
            //
            //TODO: Do this the non-lazy way.
            var value = convertedValues.ToImmutableArray(targetType.ScalarType);
            if (IsCompatible(parameterType, value.GetType()))
            {
                parameterValue = value;
                return true;
            }
            parameterValue = null;
            return false;
        }

        private bool ToArrayParameterValue(IEnumerable<object> convertedValues,
            TargetType targetType, Type parameterType, out object parameterValue)
        {
            //
            //TODO: Do this the non-lazy way.
            var value = convertedValues.ToArray(targetType.ScalarType);
            if (IsCompatible(parameterType, value.GetType()))
            {
                parameterValue = value;
                return true;
            }
            parameterValue = null;
            return false;
        }

        private bool ToListParameterValue(IEnumerable<object> convertedValues,
            TargetType targetType, Type parameterType, out object parameterValue)
        {
            //
            //TODO: Do this the non-lazy way.
            var value = convertedValues.ToList(targetType.ScalarType);
            if (IsCompatible(parameterType, value.GetType()))
            {
                parameterValue = value;
                return true;
            }
            parameterValue = null;
            return false;
        }

        private bool ToScalarParamaterValue(IEnumerable<object> convertedValues, 
            TargetType targetType, out object parameterValue)
        {
            parameterValue = null;
            int valueCount = convertedValues.Count();
            switch (convertedValues.Count())
            {
                case 0 when targetType.IsBoolean:
                    parameterValue = true;
                    return true;
                case 0:
                    return true;
                case 1:
                    parameterValue = convertedValues.Single();
                    return true;
                default:
                    return false;
            }
        }


        private bool IsCompatible(Type parameterType, Type valueType) =>
            parameterType.IsAssignableFrom(valueType);

        private bool ParameterNameIs(ParameterInfo parameter, string name) =>
            parameter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
    }
}
