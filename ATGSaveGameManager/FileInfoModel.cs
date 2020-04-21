using System;

namespace ATGSaveGameManager
{
    public class FileInfoModel
    {

        public string FullPath { get; set; }
        public string Name { get; set; }

        public string Md5 { get; set; }

        public DateTime LastWrite { get; set; }
        public DateTimeOffset? LastModified { get; set; }

        public FileStatus FileStatus { get; set; }
    }
}
