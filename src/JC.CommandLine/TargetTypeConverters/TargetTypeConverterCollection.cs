using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine.TargetTypeConverters
{
    internal class TargetTypeConverterCollection : ITargetTypeConverterInstances
    {
        private readonly List<TargetTypeConverter> _list;
        private readonly IDictionary<Type, TargetTypeConverter> _map;
        private readonly IFilesystem _filesystem;

        public TargetTypeConverterCollection()
            : this(new Filesystem())
        {
        }

        public TargetTypeConverterCollection(IFilesystem filesystem)
        {
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _list = new List<TargetTypeConverter>();
            _map = new Dictionary<Type, TargetTypeConverter>();
            _filesystem = filesystem;
        }

        public T Add<T>() where T : TargetTypeConverter
        {
            var converter = (T)Activator.CreateInstance(typeof(T), this, _filesystem);
            _list.Add(converter);
            _map.Add(converter.GetType(), converter);
            return converter;
        }

        T ITargetTypeConverterInstances.Get<T>()
        {
            return (T)_map[typeof(T)];
        }

        public TargetTypeConverterResult TryConvert(string value,
            TargetType targetType, ArgumentFlags argumentFlags)
        {
            var result = _list
                .Select(c => c.TryConvert(value, targetType, argumentFlags))
                .FirstOrDefault(r => r.Success);
            return result ?? TargetTypeConverterResult.Unsucessful;
        }
    }
}
