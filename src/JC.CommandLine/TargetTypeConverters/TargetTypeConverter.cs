namespace JC.CommandLine.TargetTypeConverters
{
    internal abstract class TargetTypeConverter
    {
        private readonly ITargetTypeConverterInstances _otherConverters;
        private readonly IFilesystem _filesystem;

        protected TargetTypeConverter(ITargetTypeConverterInstances otherConverters,
            IFilesystem filesystem)
        {
            Guard.IsNotNull(otherConverters, nameof(otherConverters));
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _otherConverters = otherConverters;
            _filesystem = filesystem;
        }


        public abstract TargetTypeConverterResult TryConvert(string value,
            TargetType targetType, ArgumentFlags argumentFlags);

        internal T GetConverter<T>() where T : TargetTypeConverter
        {
            return _otherConverters.Get<T>();
        }

        protected IFilesystem Filesystem => _filesystem;
    }
}
