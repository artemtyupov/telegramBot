using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DokanNet;
using System.Net;
using SharpCompress.Readers;
using SharpCompress.Common;
using FileAccess = DokanNet.FileAccess;

namespace TelegramBotFS
{
    class TelegramBotFSClass : IDokanOperations
    {
        private TelegramBotFSTree FSTree = new TelegramBotFSTree(Program.root_path, "filesystem.json");
        private const DokanNet.FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute | FileAccess.GenericWrite |
                                              FileAccess.GenericRead;

        private String GenerateName(String tgt)
        {
            string res = "";
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var b = md5.ComputeHash(Encoding.UTF8.GetBytes(tgt + DateTime.Now.ToBinary().ToString()));
            foreach (var cb in b) { res += cb.ToString("X2"); }
            return "\\" + res;
        }
        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;
        public void Cleanup(string fileName, IDokanFileInfo info)
        {
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            var filePath = "";
            var res_from_create = false;
            if (fileName == "\\")
            {
                return NtStatus.Success;
            }
            if (mode == FileMode.OpenOrCreate)
            {
                if (info.IsDirectory) return NtStatus.Error;
                if (FSTree.GetFSObject(fileName) == null)
                {
                    var gn = GenerateName(fileName);
                    FSTree.CreateFile(fileName, gn, true);
                    FSTree.AddFileInDB(fileName);
                    return NtStatus.Success;
                }
            }
            if (FSTree.reverse_search.ContainsKey(fileName))
            {
                filePath = Program.root_path  + "\\storage" + FSTree._fstree[FSTree.reverse_search[fileName]].DataLocation;
            }
            else
            {
                if (mode == FileMode.Create || mode == FileMode.CreateNew)
                {
                    var gn = GenerateName(fileName);
                    filePath = Program.root_path + "\\storage" + gn;
                    if (info.IsDirectory)
                    {
                        if (Directory.Exists(filePath)) return NtStatus.ObjectNameCollision;
                        try
                        {
                            File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
                            return NtStatus.ObjectNameCollision;
                        }
                        catch (IOException)
                        {
                        }
                        FSTree.CreateDirectory(fileName, gn);
                        return NtStatus.Success;
                    }
                    if (File.Exists(filePath)) return NtStatus.ObjectNameCollision;
                    res_from_create = true;
                    FSTree.CreateFile(fileName, gn, true);
                    FSTree.AddFileInDB(fileName);
                }
                else
                    return NtStatus.ObjectNameNotFound;
            }
            if (info.IsDirectory)
            {
                try
                {
                    switch (mode)
                    {
                        case FileMode.Open:
                            if (!Directory.Exists(filePath) && (FSTree.GetFSObject(fileName) == null || FSTree.GetFSObject(fileName).IsDirectory == false))
                            {
                                try
                                {
                                    if (!FSTree.GetFSObject(fileName).Attributes.HasFlag(FileAttributes.Directory)) return NtStatus.NotADirectory;
                                }
                                catch (Exception)
                                {
                                    return NtStatus.NoSuchFile;
                                }
                                return NtStatus.ObjectPathNotFound;
                            }
                            break;

                        case FileMode.CreateNew:
                            //if (Directory.Exists(filePath)) return NtStatus.ObjectNameCollision;
                            //try
                            //{
                            //    File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
                            //    return NtStatus.ObjectNameCollision;
                            //}
                            //catch (IOException)
                            //{
                            //}
                            //Directory.CreateDirectory(filePath);
                            //FSTree.CreateDirectory(fileName);
                            break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return NtStatus.AccessDenied;
                }
            }
            else
            {
                var pathExists = true;
                var pathIsDirectory = false;

                var readWriteAttributes = (access & DataAccess) == 0;
                var readAccess = (access & DataWriteAccess) == 0;

                try
                {
                    pathExists = (Directory.Exists(filePath) || File.Exists(filePath)) || res_from_create;
                    pathIsDirectory = pathExists ? File.GetAttributes(filePath).HasFlag(FileAttributes.Directory) : false;
                }
                catch (IOException)
                {
                }

                switch (mode)
                {
                    case FileMode.Open:

                        if (pathExists)
                        {
                            // check if driver only wants to read attributes, security info, or open directory
                            if (readWriteAttributes || pathIsDirectory)
                            {
                               // if (pathIsDirectory && (access & FileAccess.Delete) == FileAccess.Delete
                                    //&& (access & FileAccess.Synchronize) != FileAccess.Synchronize)
                                    //It is a DeleteFile request on a directory
                                    //return NtStatus.AccessDenied;

                                if (access == FileAccess.Delete)
                                {
                                    if (pathIsDirectory)
                                        FSTree.DeleteDirectory(fileName);
                                    else
                                        FSTree.DeleteFile(fileName);
                                }

                                info.IsDirectory = pathIsDirectory;
                                info.Context = new object();
                                // must set it to someting if you return DokanError.Success
                            }
                        }
                        else
                        {
                            return NtStatus.ObjectNameNotFound;
                        }
                        break;

                    case FileMode.CreateNew:
                        if (pathExists)
                            //return NtStatus.ObjectNameCollision;
                            return NtStatus.Success;
                        else
                        {
                            var gn = GenerateName(fileName);
                            filePath = Program.root_path + "\\storage" + gn;
                            if (info.IsDirectory)
                            {
                                if (Directory.Exists(filePath)) return NtStatus.ObjectNameCollision;
                                try
                                {
                                    File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
                                    return NtStatus.ObjectNameCollision;
                                }
                                catch (IOException)
                                {
                                }
                                FSTree.CreateDirectory(fileName, gn);
                                return NtStatus.Success;
                            }
                            if (File.Exists(filePath)) return NtStatus.ObjectNameCollision;
                            FSTree.CreateFile(fileName, gn, true);
                            FSTree.AddFileInDB(fileName);
                        }
                        break;

                    case FileMode.Truncate:
                        if (!pathExists)
                            return NtStatus.ObjectNameNotFound;
                        break;
                }

                try
                {
                    info.Context = null;//new FileStream(filePath, FileMode.OpenOrCreate,
                        //readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite, share, 4096, options);

                    if (pathExists && (mode == FileMode.OpenOrCreate
                                       || mode == FileMode.Create))
                        return NtStatus.ObjectNameCollision;

                    if (mode == FileMode.CreateNew || mode == FileMode.Create) //Files are always created as Archive
                        attributes |= FileAttributes.Archive;
                    File.SetAttributes(filePath, attributes);
                }
                catch (UnauthorizedAccessException) // don't have access rights
                {
                    if (info.Context is FileStream fileStream)
                    {
                        // returning AccessDenied cleanup and close won't be called,
                        // so we have to take care of the stream now
                        fileStream.Dispose();
                        info.Context = null;
                    }
                    return NtStatus.AccessDenied;
                }
                catch (DirectoryNotFoundException)
                {
                    return NtStatus.ObjectPathNotFound;
                }
            }
            return NtStatus.Success;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            FSTree.DeleteDirectory(fileName);
            return NtStatus.Success;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            FSTree.DeleteFile(fileName);
            return NtStatus.Success;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            if (fileName == "\\")
            {
                files = new List<FileInformation>();
                var _rf = FSTree._fstree.Values.ToList().FindAll(n => n.Parent == 0 && n.ObjectID != 0);
                foreach (var ci in _rf)
                {
                    var d = FSTree._fstree[FSTree.reverse_search[fileName + ci.Name]];
                    if (d != null && d.IsDeleted)
                    {
                        continue;
                    }
                    var co = new FileInformation()
                    {
                        FileName = ci.Name,
                        Attributes = ci.Attributes,
                        CreationTime = ci.CreatedTime,
                        LastAccessTime = ci.LastAccessTime,
                        LastWriteTime = ci.LastWriteTime,
                        Length = ci.Length
                    };
                    files.Add(co);
                }
                return NtStatus.Success;
            }
            var o = FSTree._fstree[FSTree.reverse_search[fileName]];
            files = new List<FileInformation>();
            if (o == null || o.IsDirectory || o.IsDeleted)
            {
                //return NtStatus.Error;
            }
            var l = FSTree._fstree.Values.ToList().FindAll(n => n.Parent == o.ObjectID);
            foreach (var ci in l)
            {
                var co = new FileInformation()
                {
                    FileName = ci.Name,
                    Attributes = ci.Attributes,
                    CreationTime = ci.CreatedTime,
                    LastAccessTime = ci.LastAccessTime,
                    LastWriteTime = ci.LastWriteTime,
                    Length = ci.Length
                };
                files.Add(co);
            }
            return NtStatus.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            if (searchPattern == "*") return FindFiles(fileName, out files, info);
            var fso0 = FSTree.GetFSObject(searchPattern);
            var fso1 = FSTree.GetFSObject("\\" + searchPattern);
            var fso2 = FSTree.GetFSObject("/" + searchPattern);
            var l = new List<TelegramBotFSObject>() { fso0, fso1, fso2 };
            foreach (var c in l.FindAll(e => e != null))
            {
                files = new List<FileInformation>()
                {
                    new FileInformation
                    {
                        CreationTime = c.CreatedTime,
                        FileName = c.Name,
                        Attributes = c.Attributes,
                        LastAccessTime = c.LastAccessTime,
                        LastWriteTime = c.LastWriteTime,
                        Length = c.Length
                    }
                };
                return NtStatus.Success;
            }
            return FindFiles(fileName, out files, info);
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = new List<FileInformation>();
            return NtStatus.Error;
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            try
            {
                if (info.Context != null && (info.Context as FileStream) != null)
                {
                    ((FileStream)(info.Context)).Flush();
                }
                return NtStatus.Success;
            }
            catch (IOException)
            {
                return NtStatus.DiskFull;
            }
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            freeBytesAvailable = new DriveInfo("C:\\").AvailableFreeSpace;
            totalNumberOfFreeBytes = new DriveInfo("C:\\").TotalFreeSpace;
            totalNumberOfBytes = new DriveInfo("C:\\").TotalSize;

            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(Program.root_path + "//storage", "*.*");

            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                FileInfo info1 = new FileInfo(name);
                b += info1.Length;
            }
            var all_space = totalNumberOfFreeBytes;
            var free_space = all_space - b;

            freeBytesAvailable = free_space;
            totalNumberOfFreeBytes = free_space;
            totalNumberOfBytes = all_space;

            return NtStatus.Success;
        }

        public void DownloadFileFromTelegram(string filename)
        {
            string[] elements = filename.Split("\\".ToCharArray());
            if (elements.Length <= 3)
                return;

            string _fname = elements[elements.Length - 1];
            Program.Conn.Open();
            var idStorage = Convert.ToInt32(SQLLiteDB.Select($"SELECT id FROM Storage WHERE Name = \"{elements[1]}\"", Program.Conn));
            var idFolder = Convert.ToInt32(SQLLiteDB.Select($"SELECT id FROM Folders WHERE Name = \"{elements[2]}\" and idStorage = {idStorage}", Program.Conn));
            var idLastFolder = idFolder;
            for (int i = 3; i <= elements.Length - 2; i++)
            {
                idLastFolder = Convert.ToInt32(SQLLiteDB.Select($"SELECT id FROM Folders WHERE Name = \"{elements[i]}\" and idFolder = {idLastFolder} and idStorage = {idStorage}", Program.Conn));
            }

            var idFileAPI =SQLLiteDB.Select($"SELECT idFileAPI FROM Files WHERE Name = \"{_fname}\" and idFolder = {idLastFolder}", Program.Conn);
            Program.Conn.Close();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebRequest request = WebRequest.Create($"https://api.telegram.org/bot{Program.Token}/getFile?file_id={idFileAPI}");
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            var arr = responseFromServer.Split('\"');
            foreach (var item in arr)
            {
                if (item.Contains("documents"))
                {
                    request = WebRequest.Create($"https://api.telegram.org/file/bot{Program.Token}/{item}");
                    response = (HttpWebResponse)request.GetResponse();
                    dataStream = response.GetResponseStream();
                    FSTree.DeleteFSObject(filename);
                    var gn = GenerateName(filename);
                    FSTree.CreateFile(filename, gn, false);
                    var new_obj = FSTree.GetFSObject(filename);

                    WebClient webClient = new WebClient();
                    webClient.DownloadFileAsync(new Uri($"https://api.telegram.org/file/bot{Program.Token}/{item}"), Program.root_path + "\\storage" + new_obj.DataLocation);
                }
            }
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            if (fileName == "\\" || fileName == "/")
            {
                fileInfo = new FileInformation
                {
                    FileName = "\\",
                    Length = 0,
                    Attributes = FileAttributes.Directory,
                    LastAccessTime = DateTime.Now
                };
                return NtStatus.Success;
            }
            //Тут нужно вернуть информацию по заданному файлу или каталогу. Не пытайся что-нибудь поменять, если тебе дорога жизнь
            var o = FSTree.GetFSObject(fileName);
            fileInfo = new FileInformation();
            if (o == null) return NtStatus.ObjectNameNotFound;
            if (o.Parent == 0)
            {

            }
            else if (!FSTree._fstree[o.Parent].IsDirectory) return NtStatus.Error;

            if (o.DataLocation == "")
                DownloadFileFromTelegram(fileName);

            fileInfo.Attributes = o.Attributes;
            fileInfo.CreationTime = o.CreatedTime;
            fileInfo.FileName = o.Name;
            fileInfo.LastAccessTime = o.LastAccessTime;
            fileInfo.LastWriteTime = o.LastWriteTime;
            try
            {
                o.Length = new FileInfo(Program.root_path + "\\storage" + o.DataLocation.Replace("\\\\\\", "\\")).Length;
            }
            catch { o.Length = 0; }
            fileInfo.Length = o.Length;
            if (new Random().Next(10) == 5) GC.Collect();
            return NtStatus.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            security = null;
            return NtStatus.NotImplemented;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = "TelegramBotFS storage";
            features = FileSystemFeatures.None;
            fileSystemName = "TelegramBotFS";
            maximumComponentLength = 512;
            return NtStatus.Success;
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            //Тут обработчик события, которое возникает при успешном монтировании. У меня тут ничего
            return NtStatus.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            var n_n = newName.Split("\\".ToCharArray());
            String nn = n_n[n_n.Length - 1];
            var o = FSTree._fstree[FSTree.reverse_search[oldName]];
            o.Name = nn;
            FSTree.reverse_search.Remove(oldName);
            FSTree.reverse_search.Add(newName, o.ObjectID);
            FSTree._fspaths.Remove(oldName);
            FSTree._fspaths.Add(newName);
            o.LastAccessTime = DateTime.Now;
            o.LastWriteTime = DateTime.Now;
            return NtStatus.Success;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            

            //Этот метод - просто копипаст из другого проекта. Могу сказать только то, что он работает.
            bytesRead = 0;
            //if (info.Context == null)
            //{
                var fn = "";
                if (FSTree.reverse_search.ContainsKey(fileName))
                {
                    fn = FSTree._fstree[FSTree.reverse_search[fileName]].DataLocation;
                }
                else
                {
                    return NtStatus.ObjectNameNotFound;
                }
                bool success = false;
                while (!success)
                {
                    try
                    {
                        using (var stream = new FileStream(Program.root_path + "\\storage" + fn, FileMode.Open, System.IO.FileAccess.Read))
                        {
                            stream.Position = offset;
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                        }
                        success = true;
                    } catch { success = false; }
                }
            //}
            //else
            //{
            //    var stream = info.Context as FileStream;
            //    lock (stream)
            //    {
            //        stream.Position = offset;
            //        bytesRead = stream.Read(buffer, 0, buffer.Length);
            //    }
            //}
            return NtStatus.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.NotImplemented;
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.NotImplemented;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            FSTree._fstree[FSTree.reverse_search[fileName]].Attributes = attributes;
            return NtStatus.Success;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            return NtStatus.NotImplemented;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            //Тут обработчики события отмонтирования. Можно, например, сбросить готовый JSON на диск. Но я этого делать не буду)
            return NtStatus.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            bytesWritten = 0;
            //if (info.Context == null)
            //{
                bool success = false;
                while (!success)
                {
                    try
                    {
                        using (var stream = new FileStream(Program.root_path + "\\storage" + FSTree._fstree[FSTree.reverse_search[fileName]].DataLocation, FileMode.Open, System.IO.FileAccess.Write))
                        {
                            stream.Position = offset;
                            stream.Write(buffer, 0, buffer.Length);
                            bytesWritten = buffer.Length;
                        }
                        success = true;
                    }
                    catch { success = false; }
                }
            //}
            //else
            //{
            //    var stream = info.Context as FileStream;
            //    lock (stream) 
            //    {
            //        stream.Position = offset;
            //        stream.Write(buffer, 0, buffer.Length);
            //    }
            //    bytesWritten = buffer.Length;
            //}
            return NtStatus.Success;
        }
    }
}
