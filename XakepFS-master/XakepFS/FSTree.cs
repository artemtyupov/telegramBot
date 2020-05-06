using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Timers;
using DokanNet;

namespace TelegramBotFS
{
    class TelegramBotFSTree
    {
        public String RootDataDirectory = Program.root_path;
        public String JsonPath = "";
        public List<String> _fspaths = new List<String>();
        public Dictionary<int, TelegramBotFSObject> _fstree = new Dictionary<int, TelegramBotFSObject>();
        public Dictionary<int, int> idsStorages = new Dictionary<int, int>();
        public Dictionary<int, int> idsFolders = new Dictionary<int, int>();
        public Dictionary<int, int> idsFiles = new Dictionary<int, int>();
        public Dictionary<String, int> reverse_search = new Dictionary<string, int>();
        private TelegramBotFSObject _FSRoot = new TelegramBotFSObject();
        private Timer _filesystem_sync_timer = new Timer(10000); //Один раз в минуту
        public TelegramBotFSTree(String root, String json_path)
        {
            RootDataDirectory = root;
            JsonPath = json_path;
            ParseTreeFromDB();
            
            //TODO решить нужно ли тут делать апдейт для бд раз в минуту?!
            //_filesystem_sync_timer.AutoReset = true;
            //_filesystem_sync_timer.Elapsed += delegate
            //{
            //    List<String> _packed_fs_obj = new List<string>();
            //    foreach (var ck in _fstree.Keys)
            //    {
            //        _packed_fs_obj.Add(_fstree[ck].PackJson());
            //    }
            //    String _json = JsonConvert.SerializeObject(_packed_fs_obj);
            //    File.WriteAllText(JsonPath, _json);
            //};
            //_filesystem_sync_timer.Start();
        }

        private void InitializeFS()
        {
            TelegramBotFSObject fsobj = new TelegramBotFSObject();
            fsobj.AccessControl = new DirectorySecurity();
            fsobj.Attributes = FileAttributes.Directory;
            fsobj.CreatedTime = DateTime.Now;
            fsobj.DataLocation = Program.root_path + "\\storage\\";
            fsobj.IsDeleted = false;
            fsobj.IsDirectory = true;
            fsobj.LastAccessTime = DateTime.Now;
            fsobj.LastWriteTime = DateTime.Now;
            fsobj.Length = 0;
            fsobj.Name = "\\";
            fsobj.ObjectID = 0;
            fsobj.Parent = 0;
            fsobj.Type = TelegramBotFSObject.ETypeFile.Root;
            _fspaths.Add("\\");
            _fstree.Add(0, fsobj);
            reverse_search.Add("\\", 0);
            _FSRoot = fsobj;
        }

        private void ParseTreeFromDB()
        {
            //Init root
            idsStorages.Clear();
            idsFolders.Clear();
            idsFiles.Clear();
            _fstree.Clear();
            _fspaths.Clear();
            reverse_search.Clear();
            InitializeFS();

            //Init storages
            string sql_storages = "SELECT id, Name FROM Storage";
            Program.Conn.Open();
            var all_storages = SQLLiteDB.SelectReader(sql_storages, Program.Conn);
            while (all_storages.Read())
            {
                TelegramBotFSObject fs_obj_storage = new TelegramBotFSObject();
                fs_obj_storage.Attributes = FileAttributes.Directory;
                fs_obj_storage.Name = all_storages[1].ToString();
                fs_obj_storage.id_DB = Convert.ToInt32(all_storages[0]);
                int _objid = Convert.ToInt32(all_storages[0]);
                int ols_id_storage = _objid;
                while (_fstree.ContainsKey(_objid)) { _objid++; }
                if (ols_id_storage != _objid)
                    idsStorages.Add(ols_id_storage, _objid);
                fs_obj_storage.ObjectID = _objid;
                fs_obj_storage.Parent = 0;
                fs_obj_storage.IsDeleted = false;
                fs_obj_storage.IsDirectory = true;
                fs_obj_storage.Type = TelegramBotFSObject.ETypeFile.Storage;
                _fstree.Add(fs_obj_storage.ObjectID, fs_obj_storage);
                String path_storage = GetPathById(fs_obj_storage.ObjectID);
                _fspaths.Add(path_storage);
                reverse_search.Add(path_storage, fs_obj_storage.ObjectID);

                //Init folders
                string sql_folders = $"SELECT id, Name, idFolder FROM Folders WHERE idStorage = {fs_obj_storage.id_DB}";
                var all_folders = SQLLiteDB.SelectReader(sql_folders, Program.Conn);
                
                while (all_folders.Read())
                {
                    TelegramBotFSObject fs_obj_folder = new TelegramBotFSObject();
                    fs_obj_folder.Attributes = FileAttributes.Directory;
                    fs_obj_folder.Name = all_folders[1].ToString();
                    fs_obj_folder.id_DB = Convert.ToInt32(all_folders[0]);
                    _objid = Convert.ToInt32(all_folders[0]);
                    int old_id_folder = _objid;
                    while (_fstree.ContainsKey(_objid)) { _objid++; }
                    fs_obj_folder.ObjectID = _objid;
                    if (old_id_folder != _objid)
                        idsFolders.Add(old_id_folder, _objid);
                    if (Convert.ToInt32(all_folders[2]) == -1)
                        fs_obj_folder.Parent = fs_obj_storage.ObjectID;
                    else
                    {
                        if (!idsFolders.ContainsKey(Convert.ToInt32(all_folders[2])))
                            fs_obj_folder.Parent = Convert.ToInt32(all_folders[2]);
                        else
                            fs_obj_folder.Parent = idsFolders[Convert.ToInt32(all_folders[2])];
                    }
                    fs_obj_folder.IsDeleted = false;
                    fs_obj_folder.IsDirectory = true;
                    fs_obj_folder.Type = TelegramBotFSObject.ETypeFile.Folder;
                    _fstree.Add(fs_obj_folder.ObjectID, fs_obj_folder);
                    String path_folder = GetPathById(fs_obj_folder.ObjectID);
                    _fspaths.Add(path_folder);
                    reverse_search.Add(path_folder, fs_obj_folder.ObjectID);

                    //Init files
                    string sql_files = $"SELECT id, Name, FSAccessTime, FSWriteTime, FSCreatedTime FROM Files WHERE idFolder = {fs_obj_folder.id_DB}";
                    var all_files = SQLLiteDB.SelectReader(sql_files, Program.Conn);
                    
                    while (all_files.Read())
                    {
                        TelegramBotFSObject fs_obj_file = new TelegramBotFSObject();
                        fs_obj_file.Attributes = FileAttributes.Normal;
                        fs_obj_file.Name = all_files[1].ToString();
                        fs_obj_file.id_DB = Convert.ToInt32(all_files[0]);
                        _objid = Convert.ToInt32(all_files[0]);
                        int ols_id_files = _objid;
                        while (_fstree.ContainsKey(_objid)) { _objid++; }
                        if (ols_id_files != _objid)
                            idsFiles.Add(ols_id_files, _objid);
                        fs_obj_file.ObjectID = _objid;
                        fs_obj_file.Parent = fs_obj_folder.ObjectID;
                        fs_obj_file.IsDeleted = false;
                        fs_obj_file.IsDirectory = false;
                        string time = all_files[2].ToString();
                        fs_obj_file.LastAccessTime = Convert.ToDateTime(time);
                        fs_obj_file.LastWriteTime = Convert.ToDateTime(all_files[3].ToString());
                        fs_obj_file.CreatedTime = Convert.ToDateTime(all_files[4].ToString());
                        _fstree.Add(fs_obj_file.ObjectID, fs_obj_file);
                        String path_file = GetPathById(fs_obj_file.ObjectID);
                        _fspaths.Add(path_file);
                        reverse_search.Add(path_file, fs_obj_file.ObjectID);
                    }
                    all_files.Close();
                }
                all_folders.Close();
            }
            all_storages.Close();

            Program.Conn.Close();
        }

        private String GetPathById(int id)
        {
            if (id == 0)
            {
                return "\\";
            }
            return (GetPathById(_fstree[id].Parent) + "\\" + _fstree[id].Name).Replace("\\\\", "\\").Replace("\\\\", "\\");
        }

        public void CreateFile(String path, String gn)
        {
            int i = 0;
            if (reverse_search.ContainsKey(path))
                path += $" ({Convert.ToString(i)})";
            while (reverse_search.ContainsKey(path))
            {
                path.Replace($" ({Convert.ToString(i)})", $" ({Convert.ToString(i++)})");
            }

            int _objid = new Random().Next();
            while (_fstree.ContainsKey(_objid)) { _objid = new Random().Next(); }
            reverse_search.Add(path, _objid);

            String[] elements = path.Split("\\".ToCharArray());
            String _fname = elements[elements.Length - 1];
            String _p_dir = path.Remove(path.Length - _fname.Length - 1);
            TelegramBotFSObject fsobj = new TelegramBotFSObject();
            fsobj.AccessControl = new FileSecurity();
            fsobj.Attributes = FileAttributes.Normal;
            fsobj.CreatedTime = DateTime.Now;
            fsobj.IsDeleted = false;
            fsobj.IsDirectory = false;
            fsobj.LastAccessTime = DateTime.Now;
            fsobj.LastWriteTime = DateTime.Now;
            fsobj.Length = 0;
            fsobj.Name = _fname;
            fsobj.ObjectID = _objid;
            if (_p_dir == "")
            {
                
            }
            else if (_p_dir != "\\")
            {
                //_p_dir = _p_dir.Remove(_p_dir.Length - 2);
            }
            fsobj.DataLocation = $"{_p_dir}\\{gn}";
            //File.Create(RootDataDirectory + ((_p_dir == "" || _p_dir == "\\") ? "" : _p_dir) + gn);
            if (_p_dir == "")
            {
                _p_dir = "\\";
                fsobj.Parent = 0;
            }
            else
            {
                fsobj.Parent = reverse_search[_p_dir];
            }
            _fstree.Add(_objid, fsobj);
            _fspaths.Add(GetPathById(_objid));

            //Add in db
            //int idFolder = reverse_search[_p_dir];
            //if (idsFolders.ContainsKey(reverse_search[_p_dir]))
            //    idFolder = idsFolders[reverse_search[_p_dir]];

            //SQLLiteDB.MysqlDeleteOrInsert($"INSERT INTO Files (idFolder, idMessage, Name, idChat, FSCreatedTime, FSAccessTime, FSWriteTime, idFileAPI)" +
            //    $" VALUES ({idFolder}, {message.MessageId}, \"{filename}\", {message.Chat.Id}, \"{DateTime.Now}\", \"{DateTime.Now}\", \"{DateTime.Now}\", \"{file_id_api}\")", Program.Conn);


            return;
        }

        public void CreateDirectory(String path, String gn)
        {
            String[] elements = path.Split("\\".ToCharArray());
            String _fname = elements[elements.Length - 1];
            String _p_dir = path.Remove(path.Length - _fname.Length - 1);
            TelegramBotFSObject fsobj = new TelegramBotFSObject();
            fsobj.AccessControl = new DirectorySecurity();
            fsobj.Attributes = FileAttributes.Directory;
            fsobj.CreatedTime = DateTime.Now;
            fsobj.IsDeleted = false;
            fsobj.IsDirectory = true;
            fsobj.LastAccessTime = DateTime.Now;
            fsobj.LastWriteTime = DateTime.Now;
            fsobj.Length = 0;
            fsobj.Name = _fname;
            int _objid = new Random().Next();
            while (_fstree.ContainsKey(_objid)) { _objid = new Random().Next(); }
            fsobj.ObjectID = _objid;
            if (_p_dir == "")
            {
                _p_dir = "\\";
                fsobj.Parent = 0;
            }
            if (_p_dir != "\\")
            {
                _p_dir = _p_dir.Remove(_p_dir.Length - 2);//TODO Здесь ошибка при создании папки в папке!!!!!
            }
            fsobj.DataLocation = $"{_p_dir}\\{gn}";
            fsobj.Parent = reverse_search[_p_dir];
            _fstree.Add(_objid, fsobj);
            _fspaths.Add(GetPathById(_objid));
            reverse_search.Add(path, _objid);
            return;
        }

        public void DeleteFile(String path)
        {
            int id = reverse_search[path];
            _fstree[id].IsDeleted = true;
        }

        public void DeleteDirectory(String path)
        {
            DeleteFile(path);
        }

        public List<FileInformation> EnumerateFSEntries(String path)
        {
            var result = new List<FileInformation>();
            var fsobj = _fstree[reverse_search[path]];
            //Объект папки
            foreach (var ci in _fstree.Values.ToList().FindAll(n => n.Parent == fsobj.ObjectID && n.IsDeleted == false)) //Выбираем все объекты-наследники данного
            {
                FileInformation fi = new FileInformation();
                fi.Attributes = ci.Attributes;
                fi.CreationTime = ci.CreatedTime;
                fi.FileName = ci.Name;
                fi.LastAccessTime = ci.LastAccessTime;
                fi.LastWriteTime = ci.LastWriteTime;
                fi.Length = ci.Length;
                result.Add(fi);
            }
            return result;
        }

        public TelegramBotFSObject GetFSObject(String path)
        {
            if (!reverse_search.ContainsKey(path)) return null;
            return _fstree[reverse_search[path]];
        }

        public FileSystemSecurity GetSecurity(int id)
        {
            return _fstree[id].AccessControl;
        }

        public void AddFileInDB(string filename)
        {
            var obj = _fstree[reverse_search[filename]];
            //TODO !!! Проанализировать ситуацию, когда файл уже существует в бд, здесь или в updatestoragefromarchive.
            int idMessage = -1;//TODO Можно поменять GetData. Bot будет не пересылать сообщения, а отправлять через SendMessage по fileidAPI из бд.
            string idFileAPI = "";//TODO здесь его оставляем пустым. потом когда пользователь отправит архив storage в телегу, мы там пройдемся по всем файлам и заполним этот параметр
            int idChat = Convert.ToInt32(SQLLiteDB.Select($"SELECT idChat FROM User WHERE Name = \"{Program.username}\"", Program.Conn));
            SQLLiteDB.DeleteOrInsert($"INSERT INTO Files (idFolder, idMessage, Name, idChat, FSCreatedTime, FSAccessTime, FSWriteTime, idFileAPI, FSHash)" +
                $" VALUES ({obj.Parent}, {idMessage}, \"{obj.Name}\", {idChat}, \"{obj.CreatedTime}\", \"{obj.LastAccessTime}\", \"{obj.LastWriteTime}\", \"{idFileAPI}\", \"{obj.DataLocation.Split('\\').Last()}\")", Program.Conn);
        }
    }
}
