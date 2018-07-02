﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.RCloneClient.Persistence
{
    public class InProgressFileDto
    {
        public int Id { get; set; }
        public int BackupFolderId { get; set; }
        public BackupFolderDto BackupFolder { get; set; }
        public DateTime InsertedAt { get; set; }
        public string FileName { get; set; }
    }
}
