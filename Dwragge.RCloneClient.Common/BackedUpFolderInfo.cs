using System;

namespace Dwragge.RCloneClient.Common
{
    public class BackedUpFolderInfo
    {
        public BackedUpFolderInfo(string fileName, string parentFolder)
        {
            FileName = fileName;
            ParentFolder = parentFolder;
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public string ParentFolder { get; set; }
        public string RemoteLocation { get; set; }
        public bool IsArchived { get; set; }
        public DateTime LastModified { get; private set; } = DateTime.UtcNow;
        public DateTime FirstBackedUp { get; private set; } = DateTime.UtcNow;
    }
}