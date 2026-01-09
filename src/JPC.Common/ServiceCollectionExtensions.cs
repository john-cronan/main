using JPC.Common.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace JPC.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRuntimeWrappers(this IServiceCollection self)
        {
            self = self.AddSingleton<IEnvironment, EnvironmentWrapper>();
            self = self.AddSingleton<IFilesystem, Filesystem>();
            self = self.AddSingleton<ICompressionService, CompressionService>();
            self = self.AddSingleton<IConsole, ConsoleWrapper>();
            self = self.AddSingleton<IProcessService, ProcessService>();
            self = self.AddSingleton<IClock, Clock>();
            self = self.AddSingleton<ITempFileService, TempFileService>();
            self = self.AddSingleton<IRuntime, Runtime>();
            self = self.AddTransient<ICryptographyService, CryptographyService>();
            return self;
        }
    }
}
