using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using JPC.Common.Internal;

namespace JPC.Common.Testing
{
    public class MockFilesystem : Mock<IFilesystem>
    {
        private string _currentDirectory;
        private Encoding _textEncoding;
        private readonly IFilesystem _realImplementation;


        public MockFilesystem()
        {
            _textEncoding = Encoding.UTF8;
            _realImplementation = new Filesystem(new EnvironmentWrapper());
            Setup(m => m.GetCurrentDirectory()).Returns(_currentDirectory);
            Setup(m => m.SetCurrentDirectory(It.IsAny<string>())).Callback((Action<string>)(arg => _currentDirectory = arg));
            AllContentMethodsThrow(new FileNotFoundException());
        }


        public Encoding TextEncoding { get => _textEncoding; set => _textEncoding = value; }


        public void AllPathMembersDelegate()
        {
            Setup(p => p.AltDirectorySeparator).Returns(_realImplementation.AltDirectorySeparator);
            Setup(p => p.DirectorySeparator).Returns(_realImplementation.DirectorySeparator);
            Setup(m => m.CombinePath(It.IsAny<string[]>())).Returns<string>(
                (paths) => _realImplementation.CombinePath(paths));
            Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns<string>(
                directoryPath => _realImplementation.GetDirectoryName(directoryPath));
            Setup(m => m.GetFileName(It.IsAny<string>())).Returns<string>(
                anyPath => _realImplementation.GetFileName(anyPath));
            Setup(m => m.IsPathRooted(It.IsAny<string>())).Returns<bool>(
                anyPath => _realImplementation.IsPathRooted(anyPath.ToString()));
            Setup(m => m.SplitPath(It.IsAny<string>())).Returns<IEnumerable<string>>(
                anyPath => _realImplementation.SplitPath(anyPath.ToString()));
        }

        public void CombinePathDelegates()
            => CombinePathDelegates(_realImplementation);

        public void CombinePathDelegates(IFilesystem delegatesTo)
            => Setup(m => m.CombinePath(It.IsAny<string[]>())).Returns((Func<string[], string>)(arg => delegatesTo.CombinePath(arg)));

        public void CombinePathDelegates(Func<string[], string> delegatesTo)
            => Setup(m => m.CombinePath(It.IsAny<string[]>())).Returns((Func<string[], string>)(arg => delegatesTo(arg)));

        public void CombinePathReturns(string[] arg, string returnValue)
            => Setup(m => m.CombinePath(arg)).Returns(returnValue);
            
        public void DirectoryExists(string directoryPath)
            => Setup(m => m.DirectoryExists(directoryPath)).Returns(true);

        public void DirectoryDoesNotExist(string directoryPath)
            => Setup(m => m.DirectoryExists(directoryPath)).Returns(false);

        public void DirectoryExistsReturnsTrue()
            => Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns(true);

        public void FileDoesNotExist(string filePath)
        {
            Setup(m => m.FileExists(filePath)).Returns(false);
            AllContentMethodsThrow(filePath, new FileNotFoundException($"File '{filePath}' not found"));
        }

        public void FileExists(string filePath)
        {
            Setup(m => m.FileExists(filePath)).Returns(true);
            var directoryPath = Path.GetDirectoryName(filePath);
        }

        public void FileHasContent(string filePath, string content)
        {
            FileExists(filePath);
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, contentToAppend) => FileHasContent(filePath, content + contentToAppend));
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>(), It.IsAny<Encoding>())).Callback<string, string, Encoding>(
                (path, contentToAppend, encoding) => FileHasContent(filePath, content + contentToAppend));
            Setup(m => m.AppendLine(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, line) => FileHasContent(filePath, content + line + Environment.NewLine));
            Setup(m => m.AppendLineAsync(filePath, It.IsAny<string>())).Returns((Delegate)(Func<string,string,Task>)(
                (path, line) =>
                {
                    FileHasContent(filePath, content + line + Environment.NewLine);
                    return Task.CompletedTask;
                }));
            Setup(m => m.ReadAllBytes(filePath)).Returns(_textEncoding.GetBytes(content));
            Setup(m => m.ReadAllBytesAsync(filePath)).Returns(Task.FromResult(_textEncoding.GetBytes(content)));
            Setup(m => m.ReadAllLines(filePath)).Returns(content.Split(Environment.NewLine));
            Setup(m => m.ReadAllText(filePath)).Returns(content);
            Setup(m => m.ReadAllTextAsync(filePath)).Returns(Task.FromResult(content));
            Setup(m => m.WriteAllBytes(filePath, It.IsAny<byte[]>())).Callback<string, byte[]>(
                (path, content) => FileHasContent(path, content));
            Setup(m => m.WriteAllBytesAsync(filePath, It.IsAny<byte[]>())).Returns(
                (Func<string, byte[], Task>)((filePath, content) =>
                {
                    FileHasContent(filePath, content);
                    return Task.CompletedTask;
                }));
            Setup(m => m.WriteAllLines(filePath, It.IsAny<string[]>())).Callback<string, string[]>(
                (path, lines) => FileHasContent(path, lines));
            Setup(m => m.WriteAllText(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, content) => FileHasContent(path, content));
            Setup(m => m.WriteAllTextAsync(filePath, It.IsAny<string>())).Returns((Func<string, string, Task>)
                ((path, content) =>
                {
                    FileHasContent(path, content);
                    return Task.CompletedTask;
                }));
        }

        public void FileHasContent(string filePath, string[] lines)
        {
            FileExists(filePath);
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, contentToAppend) => FileHasContent(filePath, string.Join(Environment.NewLine, lines) + contentToAppend));
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>(), It.IsAny<Encoding>())).Callback<string, string, Encoding>(
                (path, contentToAppend, encoding) => FileHasContent(filePath, string.Join(Environment.NewLine, lines) + contentToAppend));
            Setup(m => m.AppendLine(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, line) => FileHasContent(filePath, new string[] { line }));
            Setup(m => m.AppendLineAsync(filePath, It.IsAny<string>())).Returns((Delegate)(Func<string,string,Task>)(
                (path, line) =>
                {
                    FileHasContent(filePath, new string[] { line });
                    return Task.CompletedTask;
                }));
            Setup(m => m.ReadAllBytes(filePath)).Returns(lines.SelectMany(l => _textEncoding.GetBytes(l)).ToArray());
            Setup(m => m.ReadAllBytesAsync(filePath)).Returns(Task.FromResult(lines.SelectMany(l => _textEncoding.GetBytes(l)).ToArray()));
            Setup(m => m.ReadAllLines(filePath)).Returns(lines);
            Setup(m => m.ReadAllText(filePath)).Returns(string.Join(Environment.NewLine, lines));
            Setup(m => m.ReadAllTextAsync(filePath)).Returns(Task.FromResult(string.Join(Environment.NewLine, lines)));
            Setup(m => m.WriteAllBytes(filePath, It.IsAny<byte[]>())).Callback<string, byte[]>((path, content) =>
            {
                FileHasContent(filePath, content);
            });
            Setup(m => m.WriteAllBytesAsync(filePath, It.IsAny<byte[]>())).Returns(
                (Func<string, byte[], Task>)((filePath, content) =>
                {
                    FileHasContent(filePath, content);
                    return Task.CompletedTask;
                }));
            Setup(m => m.WriteAllLines(filePath, It.IsAny<string[]>())).Callback<string, string[]>(
                (path, lines) => FileHasContent(path, lines));
            Setup(m => m.WriteAllText(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, content) => FileHasContent(path, content));
            Setup(m => m.WriteAllTextAsync(filePath, It.IsAny<string>())).Returns((Func<string, string, Task>)
                ((path, content) =>
                {
                    FileHasContent(path, content);
                    return Task.CompletedTask;
                }));
        }

        public void FileHasContent(string filePath, byte[] bytes)
        {
            FileExists(filePath);
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, contentToAppend) => FileHasContent(filePath, _textEncoding.GetString(bytes) + contentToAppend));
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>(), It.IsAny<Encoding>())).Callback<string, string, Encoding>(
                (path, contentToAppend, encoding) => FileHasContent(filePath, encoding.GetString(bytes) + contentToAppend));
            Setup(m => m.AppendLine(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, line) => FileHasContent(filePath, _textEncoding.GetString(bytes) + line + Environment.NewLine));
            Setup(m => m.AppendLineAsync(filePath, It.IsAny<string>())).Returns((Delegate)(Func<string,string,Task>)(
                (path, line) =>
                {
                    FileHasContent(filePath, _textEncoding.GetString(bytes) + line + Environment.NewLine);
                    return Task.CompletedTask;
                }));
            Setup(m => m.ReadAllBytes(filePath)).Returns(bytes);
            Setup(m => m.ReadAllBytesAsync(filePath)).Returns(Task.FromResult(bytes));
            Setup(m => m.ReadAllLines(filePath)).Returns(_textEncoding.GetString(bytes).Split(Environment.NewLine));
            Setup(m => m.ReadAllText(filePath)).Returns(_textEncoding.GetString(bytes));
            Setup(m => m.ReadAllTextAsync(filePath)).Returns(Task.FromResult(_textEncoding.GetString(bytes)));
            Setup(m => m.WriteAllBytes(filePath, It.IsAny<byte[]>())).Callback<string, byte[]>((path, content) =>
            {
                FileHasContent(filePath, content);
            });
            Setup(m => m.WriteAllBytesAsync(filePath, It.IsAny<byte[]>())).Returns(
                (Func<string, byte[], Task>)((filePath, content) =>
                {
                    FileHasContent(filePath, content);
                    return Task.CompletedTask;
                }));
            Setup(m => m.WriteAllLines(filePath, It.IsAny<string[]>())).Callback<string, string[]>(
                (path, lines) => FileHasContent(path, lines));
            Setup(m => m.WriteAllText(filePath, It.IsAny<string>())).Callback<string, string>(
                (path, content) => FileHasContent(path, content));
            Setup(m => m.WriteAllTextAsync(filePath, It.IsAny<string>())).Returns((Func<string, string, Task>)
                ((path, content) =>
                {
                    FileHasContent(path, content);
                    return Task.CompletedTask;
                }));
        }

        public void FileHasJsonContent(string filePath, object content)
            => FileHasContent(filePath, JsonSerializer.Serialize(content));

        public void GetDirectoryNameThrows(string directoryName, Exception exception)
            => Setup(m => m.GetDirectoryName(directoryName)).Throws(exception);

        public void GetDirectoryNameThrows(Exception exception)
            => Setup(m => m.GetDirectoryName(It.IsAny<string>())).Throws(exception);

        public void GetDirectoryNameDelegates()
            => GetDirectoryNameDelegates(_realImplementation);

        public void GetDirectoryNameDelegates(IFilesystem delegatesTo)
            => Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns((Func<string, string>)(arg => delegatesTo.GetDirectoryName(arg)));

        public void GetDirectoryRootDelegates()
            => GetDirectoryRootDelegates(_realImplementation);

        public void GetDirectoryRootDelegates(IFilesystem delegatesTo)
            => Setup(m => m.GetDirectoryRoot(It.IsAny<string>())).Returns((Func<string, string>)(arg => delegatesTo.GetDirectoryRoot(arg)));

        public void SplitPathDelegates()
            => SplitPathDelegates(_realImplementation);

        public void SplitPathDelegates(IFilesystem delegatesTo)
            => Setup(m => m.SplitPath(It.IsAny<string>())).Returns((Func<string, string[]>)(arg => delegatesTo.SplitPath(arg)));

        public void IsPathRootedDelegates()
            => IsPathRootedDelegates(_realImplementation);

        public void IsPathRootedDelegates(IFilesystem delegatesTo)
            => Setup(m => m.IsPathRooted(It.IsAny<string>())).Returns((Func<string, bool>)(arg => delegatesTo.IsPathRooted(arg)));

        public void IsPathRootedReturns(string path, bool value)
            => Setup(m => m.IsPathRooted(path)).Returns(value);
            
        public void VerifyFileCopied(string sourceFileName, string destinationFileName, bool overwrite)
            => Verify(m => m.CopyFile(sourceFileName, destinationFileName, overwrite));

        public void VerifyFileContentRead(string fileNameAndPath)
        {
            var readContentMethods =
                from method in typeof(IFilesystem).GetMethods()
                where method.Name == nameof(IFilesystem.ReadAllBytes)
                || method.Name == nameof(IFilesystem.ReadAllBytesAsync)
                || method.Name == nameof(IFilesystem.ReadAllLines)
                || method.Name == nameof(IFilesystem.ReadAllText)
                || method.Name == nameof(IFilesystem.ReadAllTextAsync)
                select method;
            var invocations =
                from invocation in Invocations
                join readContentMethod in readContentMethods
                on invocation.Method equals readContentMethod
                where invocation.Arguments.Any(a => a != null && fileNameAndPath.Equals(a.ToString()))
                select invocation;
            if (!invocations.Any())
            {
                throw new VerifyFailedException("File content method not called");
            }
        }

        public void VerifyGetDirectoryNameCalled(int times)
            => Verify(m => m.GetDirectoryName(It.IsAny<string>()), Times.Exactly(times));


        private void AllContentMethodsThrow(string filePath, Exception exceptionToThrow)
        {
            Setup(m => m.FileExists(filePath)).Throws(exceptionToThrow);
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.AppendAllText(filePath, It.IsAny<string>(), It.IsAny<Encoding>())).Throws(exceptionToThrow);
            Setup(m => m.AppendLine(filePath, It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.AppendLineAsync(filePath, It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.ReadAllBytes(filePath)).Throws(exceptionToThrow);
            Setup(m => m.ReadAllBytesAsync(filePath)).Throws(exceptionToThrow);
            Setup(m => m.ReadAllLines(filePath)).Throws(exceptionToThrow);
            Setup(m => m.ReadAllText(filePath)).Throws(exceptionToThrow);
            Setup(m => m.ReadAllTextAsync(filePath)).Throws(exceptionToThrow);
            Setup(m => m.WriteAllBytes(filePath, It.IsAny<byte[]>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllBytesAsync(filePath, It.IsAny<byte[]>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllLines(filePath, It.IsAny<string[]>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllText(filePath, It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllTextAsync(filePath, It.IsAny<string>())).Throws(exceptionToThrow);
        }

        private void AllContentMethodsThrow(Exception exceptionToThrow)
        {
            Setup(m => m.FileExists(It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.AppendAllText(It.IsAny<string>(), It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.AppendAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>())).Throws(exceptionToThrow);
            Setup(m => m.AppendLine(It.IsAny<string>(), It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.AppendLineAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.ReadAllBytes(It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.ReadAllBytesAsync(It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.ReadAllLines(It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.ReadAllText(It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.ReadAllTextAsync(It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllLines(It.IsAny<string>(), It.IsAny<string[]>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Throws(exceptionToThrow);
            Setup(m => m.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(exceptionToThrow);
        }
    }
}
