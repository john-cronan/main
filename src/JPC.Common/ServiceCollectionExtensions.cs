using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using JPC.Common.Internal;

namespace JPC.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpasCommon(this IServiceCollection self)
        {
            self = self.AddSingleton<IEnvironment, EnvironmentWrapper>();
            self = self.AddSingleton<ICompressionService, CompressionService>();
            self = self.AddSingleton<ITempFileService, TempFileService>();
            self = self.AddSingleton<IFilesystem, Filesystem>();
            self = self.AddTransient<IConsole, ConsoleWrapper>();
            self = self.AddTransient<IProcessService, ProcessService>();
            self = self.AddSingleton<IClock, Clock>();
            //self = self.AddSingleton<IExclusiveLockService, ExclusiveLockService>();
            self = self.AddTransient<ICryptographyService, CryptographyService>();
            return self;
        }

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
            return self;
        }
    }
}
