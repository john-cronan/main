using System.IO;

namespace JPC.Common
{
    public class DriveInformation
    {
        private readonly long _availableFreeSpace;
        private readonly string _format;
        private readonly DriveType _driveType;
        private readonly bool _isReady;
        private readonly string _name;
        private readonly string _rootDirectory;
        private readonly long _totalFreeSpace;
        private readonly long _totalSize;
        private readonly string _volumeLabel;

        public DriveInformation(long availableFreeSpace, string format, DriveType driveType,
            bool isReady, string name, string rootDirectory, long totalFreeSpace,
            long totalSize, string volumeLabel)
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
            : this(driveInfo.AvailableFreeSpace, driveInfo.DriveFormat, driveInfo.DriveType,
                  driveInfo.IsReady, driveInfo.Name, driveInfo.RootDirectory.FullName,
                  driveInfo.TotalFreeSpace, driveInfo.TotalSize, driveInfo.VolumeLabel)
        {
        }

        public long AvailableFreeSpace => _availableFreeSpace;
        public string Format => _format;
        public DriveType DriveType => _driveType;
        public bool IsReady => _isReady;
        public string Name => _name;
        public string RootDirectory => _rootDirectory;
        public long TotalFreeSpace => _totalFreeSpace;
        public long TotalSize => _totalSize;
        public string VolumeLabel => _volumeLabel;
    }
}
