using System.IO;

namespace JPC.Common
{
    public class DriveInformation
    {
        private readonly long? _availableFreeSpace;
        private readonly string _format;
        private readonly DriveType _driveType;
        private readonly bool _isReady;
        private readonly string _name;
        private readonly string _rootDirectory;
        private readonly long? _totalFreeSpace;
        private readonly long? _totalSize;
        private readonly string _volumeLabel;

        public DriveInformation(long? availableFreeSpace, string format, DriveType driveType,
            bool isReady, string name, string rootDirectory, long? totalFreeSpace,
            long? totalSize, string volumeLabel)
        {
            _availableFreeSpace = availableFreeSpace;
            _format = format;
            _driveType = driveType;
            _isReady = isReady;
            _name = name;
            _rootDirectory = rootDirectory;
            _totalFreeSpace = totalFreeSpace;
            _totalSize = totalSize;
            _volumeLabel = volumeLabel;
        }

        public DriveInformation(DriveInfo driveInfo)
        {
            _driveType = driveInfo.DriveType;
            _isReady = driveInfo.IsReady;
            _name = driveInfo.Name;
            _rootDirectory = driveInfo.RootDirectory.FullName;
            if (driveInfo.IsReady)
            {
                _availableFreeSpace = driveInfo.AvailableFreeSpace;
                _format = driveInfo.DriveFormat;
                _totalFreeSpace = driveInfo.TotalFreeSpace;
                _totalSize = driveInfo.TotalSize;
                _volumeLabel = driveInfo.VolumeLabel;
            }
        }

        public long? AvailableFreeSpace => _availableFreeSpace;
        public string Format => _format;
        public DriveType DriveType => _driveType;
        public bool IsReady => _isReady;
        public string Name => _name;
        public string RootDirectory => _rootDirectory;
        public long? TotalFreeSpace => _totalFreeSpace;
        public long? TotalSize => _totalSize;
        public string VolumeLabel => _volumeLabel;
    }
}
