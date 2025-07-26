namespace JC.CommandLine
{
    internal class ActualModelMatch
    {
        private readonly CommandLineNodeGroup _actual;
        private readonly Argument _model;

        public ActualModelMatch(CommandLineNodeGroup actual,
            Argument model)
        {
            Guard.IsNotNull(actual, nameof(actual));
            Guard.IsNotNull(model, nameof(model));

            _actual = actual;
            _model = model;
        }

        public Argument Model => _model;

        internal CommandLineNodeGroup Actual => _actual;
    }

}
