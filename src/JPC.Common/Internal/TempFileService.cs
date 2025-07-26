using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JPC.Common.Internal
{
    internal class TempFileService : ITempFileService
    {
        private readonly IEnvironment _environment;
        private readonly Lazy<string> _processInstanceTempDirectory;
        private readonly Lazy<string> _processTempDirectory;
        private readonly IFilesystem _filesystem;
        private readonly IProcessService _processService;
        private readonly IClock _clock;

        public TempFileService(IEnvironment environment, IFilesystem filesystem, IProcessService processService,
            IClock clock)
        {
            _clock = clock;
            _environment = environment;
            _filesystem = filesystem;
            _processService = processService;
            _processInstanceTempDirectory = new Lazy<string>(CreateProcessInstanceTempDirectory, LazyThreadSafetyMode.ExecutionAndPublication);
            _processTempDirectory = new Lazy<string>(GetProcessTempDirectory, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        Task ITempFileService.CleanAsync()
        {
            return (this as ITempFileService).CleanAsync((Action<CleanObjectResult>)null);
        }

        async Task ITempFileService.CleanAsync(Action<CleanObjectResult> cleanObjectCallback)
        {
            if (!_filesystem.DirectoryExists(_processTempDirectory.Value))
            {
                return;
            }

            var processInstanceDirectoryNames =
                from directoryName in _filesystem.GetSubdirectoryNames(_processTempDirectory.Value)
                let segments = directoryName.Split('.')
                let lastSegment = segments[segments.Length - 1]
                let leafDirectoryName = _filesystem.GetFileName(directoryName)
                let processId = TryParseInt(leafDirectoryName)
                where processId != null
                select processId.Value;
            var processes =
                from process in _processService.GetAll()
                where process.ProcessName == _processService.GetCurrentProcess().ProcessName
                select process.Id;
            var directoriesWithoutProcesses =
                processInstanceDirectoryNames.GroupJoin(processes,
                    o => o, i => i,
                    (o, ii) => new { Id = o, Count = ii.Count() })
                .Where(p => p.Count == 0)
                .Select(p => _filesystem.CombinePath(_processTempDirectory.Value, p.Id.ToString()));
            var tasks = new List<Task>();
            foreach (var directoryWithoutProcesses in directoriesWithoutProcesses)
            {
                tasks.Add(CleanDirectoryRecursivelyAsync(directoryWithoutProcesses, cleanObjectCallback));
            }
            tasks.Add(CleanDirectoryRecursivelyAsync(_processTempDirectory.Value, cleanObjectCallback));
            // tasks.Add(CleanDirectoryRecursivelyAsync(_processTempDirectory.Value, cleanObjectCallback));

            await Task.WhenAll(tasks);
        }

        string ITempFileService.ReserveFileName(string extension)
        {
            var fileName = Guid.NewGuid().ToString("n").Substring(0, 8) + "." + extension;
            var pathAndFileName = _filesystem.CombinePath(_processInstanceTempDirectory.Value, fileName);
            _filesystem.WriteAllBytes(pathAndFileName, Array.Empty<byte>());
            return pathAndFileName;
        }

        private string CreateProcessInstanceTempDirectory()
        {
            var currentProcess = _processService.GetCurrentProcess();
            var tempDirectoryPath =
                _filesystem.CombinePath(
                    _environment.ExpandEnvironmentVariables("%TEMP%"),
                    _processService.GetCurrentProcess().ProcessName,
                    _processService.GetCurrentProcess().Id.ToString());
            _filesystem.CreateDirectory(tempDirectoryPath);
            return tempDirectoryPath;
        }

        private string GetProcessTempDirectory()
        {
            var currentProcess = _processService.GetCurrentProcess();
            var tempDirectoryPath =
                _filesystem.CombinePath(
                    _environment.ExpandEnvironmentVariables("%TEMP%"),
                    _processService.GetCurrentProcess().ProcessName);
            return tempDirectoryPath;
        }

        private Task CleanDirectoryRecursivelyAsync(string directoryName, Action<CleanObjectResult> cleanObjectCallback = null)
        {
            var q = new Queue<string>();
            q.Enqueue(directoryName);
            _clock.StartTimer("CleanDir");
            while (q.Any())
            {
                var current = q.Dequeue();
                foreach (var item in _filesystem.GetSubdirectoryNames(current))
                {
                    q.Enqueue(item);
                }
                foreach (var fileNameAndPath in _filesystem.GetFileNames(current))
                {
                    try
                    {
                        _clock.ResetTimer("CleanDir");
                        _filesystem.DeleteFile(fileNameAndPath);
                        var elapsed = _clock.StopTimer("CleanDir");
                        if (cleanObjectCallback != null)
                        {
                            cleanObjectCallback(new CleanObjectResult(fileNameAndPath, false, true, null, elapsed));
                        }
                    }
                    catch (Exception ex)
                    {
                        var elapsed = _clock.StopTimer("CleanDir");
                        if (cleanObjectCallback != null)
                        {
                            cleanObjectCallback(new CleanObjectResult(fileNameAndPath, false, false, ex, elapsed));
                        }
                    }
                }
                try
                {
                    _clock.ResetTimer("CleanDir");
                    _filesystem.DeleteDirectory(current, true);
                    var elapsed = _clock.StopTimer("CleanDir");
                    if (cleanObjectCallback != null)
                    {
                        cleanObjectCallback(new CleanObjectResult(current, false, true, null, elapsed));
                    }
                }
                catch (Exception ex)
                {
                    var elapsed = _clock.StopTimer("CleanDir");
                    if (cleanObjectCallback != null)
                    {
                        cleanObjectCallback(new CleanObjectResult(current, false, false, ex, elapsed));
                    }
                }
            }
            return Task.CompletedTask;
        }

        private static int? TryParseInt(string value) => int.TryParse(value, out var i) ? i : (int?)null;
    }
}
