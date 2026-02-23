using JPC.Common;
using Microsoft.Extensions.DependencyInjection;

namespace JPC.Backup
{
    internal static class ServiceContainer
    {    
        public static IServiceProvider Build()
        {
            var services = new ServiceCollection();
            services.AddTransient<SpecificationFileFinder>();
            services.AddRuntimeWrappers();
            AddBackupAPI(services);
            return services.BuildServiceProvider();
        }

        private static void AddBackupAPI(ServiceCollection services)
        {
            services.AddTransient<BackupProcessor>();
            services.AddTransient<ISourceDirectoryWalkerBuilder, SourceDirectoryWalkerBuilder>();
            services.AddTransient<IDirectoryFileCopyFactory, DirectoryFileCopyFactory>();
            services.AddSingleton<IBackupEvents, AggregatingBackupEventSink>();
        }
    }
}
