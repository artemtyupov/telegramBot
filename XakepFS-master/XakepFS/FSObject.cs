using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using Newtonsoft.Json;

namespace TelegramBotFS
{
    class TelegramBotFSObject
    {
        public enum ETypeFile
        {
            Unknown = -1,
            Root = 0,
            Storage = 1,
            Folder = 2,
            Document = 3,
            Photo = 4,
        }

        public String Name = "";
        public ETypeFile Type = ETypeFile.Unknown;
        public bool IsDirectory = false;
        public int Parent = 0;
        public int ObjectID = 0;
        public long Length = 0;
        public DateTime CreatedTime = DateTime.Now;
        public DateTime LastWriteTime = DateTime.Now;
        public DateTime LastAccessTime = DateTime.Now;
        public FileAttributes Attributes = FileAttributes.Normal;
        public FileSystemSecurity AccessControl;
        public String DataLocation = "";
        public bool IsDeleted = false;
        public int id_DB = 0;
    }
}
