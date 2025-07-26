using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace JC.CommandLine
{
    internal class PropertyBinder : IObjectBinder
    {
        private readonly IFilesystem _filesystem;
        private readonly ArgumentValueConverter _converter;

        public PropertyBinder()
            : this(new Filesystem())
        {
        }

        public PropertyBinder(IFilesystem filesystem)
        {
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _filesystem = filesystem;
            _converter = new ArgumentValueConverter(_filesystem);
        }

        T IObjectBinder.CreateObject<T>(ActualModelResolution actualModelResolution)
        {
            Guard.IsNotNull(actualModelResolution, nameof(actualModelResolution));

            var instance = CreateInstance<T>();
            try
            {
                var converter = new ArgumentValueConverter(_filesystem);
                AssignProperties(instance, actualModelResolution,
                    actualModelResolution.Model.StringComparisons);
                AssignParseWarnings(instance, actualModelResolution);
                AssignUnnamedValues(instance, actualModelResolution);
                CompleteInstance(instance);
                return (T)instance;
            }
            catch
            {
                DisposeInstance(instance);
                throw;
            }
        }

        private T CreateInstance<T>()
        {
            var instance = Activator.CreateInstance(typeof(T));
            if (instance is ISupportInitialize)
            {
                (instance as ISupportInitialize).BeginInit();
            }
            return (T)instance;
        }

        private void AssignProperties(object instance,
            ActualModelResolution actualModelRes, StringComparison stringComparisons)
        {
            var eligibleProperties =
                (from property in instance.GetType().GetProperties()
                 where property.GetGetMethod() != null
                 && property.GetGetMethod().IsPublic
                 && property.GetSetMethod() != null
                 && property.GetSetMethod().IsPublic
                 select property).ToArray();
            var propertiesAndValues =
                (from property in eligibleProperties
                 from match in actualModelRes.Matches
                 let names = PreprocessNames(match.Model.Names)
                 where names.Any(n => property.Name.Equals(n, stringComparisons))
                 select new
                 {
                     PropertyInfo = property,
                     Values = match.Actual.ValueNodes.Select(vn => vn.Text),
                     Flags = match.Model.Flags
                 }).ToArray();
            foreach (var property in propertiesAndValues)
            {
                var targetType = new TargetType(property.PropertyInfo.PropertyType);
                var convertedValues = property.Values
                    .SelectMany(v => _converter.Convert(v, targetType, property.Flags))
                    .ToArray();
                if (targetType.IsVectorType)
                {
                    AssignVectorProperty(instance, property.PropertyInfo,
                        targetType, convertedValues);
                }
                else
                {
                    AssignScalarProperty(instance, property.PropertyInfo,
                        targetType, convertedValues);
                }
            }
        }

        private void AssignVectorProperty(object instance, PropertyInfo propertyInfo,
            TargetType targetType, IEnumerable<object> convertedValues)
        {
            try
            {
                var collection = convertedValues.ToImmutableArray(targetType.ScalarType);
                propertyInfo.SetValue(instance, collection);
                return;
            }
            catch
            {
            }
            try
            {
                var collection = convertedValues.ToArray(targetType.ScalarType);
                propertyInfo.SetValue(instance, collection);
                return;
            }
            catch
            {
            }
            try
            {
                var collection = convertedValues.ToList(targetType.ScalarType);
                propertyInfo.SetValue(instance, collection);
                return;
            }
            catch
            {
            }
            var msg = $"Cannot assign a vector value to a property of type {targetType}";
            throw new CommandLineParseException(msg);
        }

        private void AssignScalarProperty(object instance, PropertyInfo propertyInfo,
            TargetType targetType, IEnumerable<object> convertedValues)
        {
            int valueCount = convertedValues.Count();
            switch (convertedValues.Count())
            {
                case 0 when targetType.IsBoolean:
                    propertyInfo.SetValue(instance, true);
                    break;
                case 0:
                    break;
                case 1:
                    propertyInfo.SetValue(instance, convertedValues.Single());
                    break;
                default:
                    throw new CommandLineParseException($"Cannot assign multiple values to property '{propertyInfo.Name}'");
            }
        }

        private void AssignParseWarnings(object instance, ActualModelResolution actualModelResolution)
        {
            (var errors, var warnings) = actualModelResolution.Validate();
            if (errors != null)
            {
                throw errors;
            }
            if (warnings == null)
            {
                return;
            }
            var parseWarningsProperty =
                (from property in instance.GetType().GetProperties()
                 let setter = property.GetSetMethod()
                 where setter != null
                 && setter.IsPublic
                 && property.Name.Equals("ParseWarnings", StringComparison.InvariantCultureIgnoreCase)
                 && property.PropertyType.IsAssignableFrom(typeof(CommandLineParseException))
                 select property).FirstOrDefault();
            if (parseWarningsProperty != null)
            {
                parseWarningsProperty.SetValue(instance, warnings);
            }
        }

        private void AssignUnnamedValues(object instance, ActualModelResolution resolutions)
        {
            AssignAllUnnamedValues(instance, resolutions);
            AssignLeadingUnnamedValues(instance, resolutions);
            AssignTrailingUnnamedValues(instance, resolutions);
        }

        private void AssignAllUnnamedValues(object instance, 
            ActualModelResolution resolution)
        {
            var all = resolution.Actuals
                        .Where(a => a.IsUnnamedValuesNodeGroup)
                        .SelectMany(n => n.ValueNodes)
                        .Select(n => n.Text);
            AssignUnnamedValuesToProperty(instance, all, "UnnamedValues");
        }

        private void AssignLeadingUnnamedValues(object instance, 
            ActualModelResolution resolutions)
        {
            var leadingValues = CommandLineNodeGroup.GetLeadingUnnamedValues(resolutions.Actuals)
                .Select(n => n.Text);
            AssignUnnamedValuesToProperty(instance, leadingValues, "LeadingUnnamedValues");
        }

        private void AssignTrailingUnnamedValues(object instance,
            ActualModelResolution resolutions)
        {
            var trailing = CommandLineNodeGroup.GetTrailingUnnamedValues(resolutions.Actuals)
                    .Select(n => n.Text);
            AssignUnnamedValuesToProperty(instance, trailing, "TrailingUnnamedValues");
        }

        private void AssignUnnamedValuesToProperty(object instance,
            IEnumerable<string> values, string propertyName)
        {
            var property = instance.GetType().GetProperty(propertyName);
            var propertyIsOK = property != null && property.GetSetMethod() != null 
                    && property.GetSetMethod().IsPublic;
            if (!propertyIsOK)
            {
                return;
            }
            try
            {
                property.SetValue(instance, values.ToImmutableArray());
                return;
            }
            catch
            {
            }
            try
            {
                var propertyValue = values.ToArray();
                property.SetValue(instance, propertyValue);
                return;
            }
            catch
            {
            }
            try
            {
                var propertyValue = values.ToList();
                property.SetValue(instance, propertyValue);
                return;
            }
            catch
            {
            }
            var msg = $"Unable to assign unnamed values to property {propertyName} of type {property.GetType().Name}";
            throw new CommandLineParseException(msg);
        }

        private void CompleteInstance(object instance)
        {
            if (instance is ISupportInitialize)
            {
                (instance as ISupportInitialize).EndInit();
            }
        }

        private void DisposeInstance(object instance)
        {
            if (instance is IDisposable)
            {
                (instance as IDisposable).Dispose();
            }
        }

        private IEnumerable<string> PreprocessNames(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                var newName =
                    name.Replace("-", string.Empty)
                        .Replace("_", string.Empty)
                        .Replace(":", string.Empty);
                yield return newName;
            }
        }
    }
}
