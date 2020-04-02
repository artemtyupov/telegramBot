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
        public Dictionary<String, int> reverse_search = new Dictionary<string, int>();
        private TelegramBotFSObject _FSRoot = new TelegramBotFSObject();
        private Timer _filesystem_sync_timer = new Timer(10000); //Один раз в минуту
        public TelegramBotFSTree(String root, String json_path)
        {
            RootDataDirectory = root;
            JsonPath = json_path;
            //ParseTreeFromJson();
            ParseTreeFromDB();
            /*
            _filesystem_sync_timer.AutoReset = true;
            _filesystem_sync_timer.Elapsed += delegate
            {
                List<String> _packed_fs_obj = new List<string>();
                foreach (var ck in _fstree.Keys)
                {
                    _packed_fs_obj.Add(_fstree[ck].PackJson());
                }
                String _json = JsonConvert.SerializeObject(_packed_fs_obj);
                File.WriteAllText(JsonPath, _json);
            };
            _filesystem_sync_timer.Start();*/
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
            _fspaths.Add("\\");
            _fstree.Add(0, fsobj);
            reverse_search.Add("\\", 0);
            _FSRoot = fsobj;
        }

        private void ParseTreeFromDB()
        {
            //Init root
            _fstree.Clear();
            _fspaths.Clear();
            reverse_search.Clear();
            InitializeFS();

            //Init storages
            string sql_storages = "SELECT id, Name FROM Storage";
            Program.Conn.Open();
            var all_storages = SQLLiteDB.MysqlSelectReader(sql_storages, Program.Conn);
            int last_id = 0;
            while (all_storages.Read())
            {
                TelegramBotFSObject fs_obj_storage = new TelegramBotFSObject();
                fs_obj_storage.Attributes = FileAttributes.Directory;
                fs_obj_storage.Name = all_storages[1].ToString();
                fs_obj_storage.id_DB = Convert.ToInt32(all_storages[0]);
                int _objid = Convert.ToInt32(all_storages[0]);
                while (_fstree.ContainsKey(_objid)) { _objid++; }
                fs_obj_storage.ObjectID = _objid;
                fs_obj_storage.Parent = 0;
                fs_obj_storage.IsDeleted = false;
                fs_obj_storage.IsDirectory = true;
                _fstree.Add(fs_obj_storage.ObjectID, fs_obj_storage);
                String path_storage = GetPathById(fs_obj_storage.ObjectID);
                _fspaths.Add(path_storage);
                reverse_search.Add(path_storage, fs_obj_storage.ObjectID);

                //Init folders
                string sql_folders = $"SELECT id, Name FROM Folders WHERE idStorage = {fs_obj_storage.id_DB}";
                var all_folders = SQLLiteDB.MysqlSelectReader(sql_folders, Program.Conn);
                
                while (all_folders.Read())
                {
                    TelegramBotFSObject fs_obj_folder = new TelegramBotFSObject();
                    fs_obj_folder.Attributes = FileAttributes.Directory;
                    fs_obj_folder.Name = all_folders[1].ToString();
                    fs_obj_folder.id_DB = Convert.ToInt32(all_folders[0]);
                    _objid = Convert.ToInt32(all_folders[0]);
                    while (_fstree.ContainsKey(_objid)) { _objid++; }
                    fs_obj_folder.ObjectID = _objid;
                    fs_obj_folder.Parent = fs_obj_storage.ObjectID;
                    fs_obj_folder.IsDeleted = false;
                    fs_obj_folder.IsDirectory = true;
                    _fstree.Add(fs_obj_folder.ObjectID, fs_obj_folder);
                    String path_folder = GetPathById(fs_obj_folder.ObjectID);
                    _fspaths.Add(path_folder);
                    reverse_search.Add(path_folder, fs_obj_folder.ObjectID);

                    //Init files
                    string sql_files = $"SELECT id, Name, FSAccessTime, FSWriteTime, FSCreatedTime FROM Files WHERE idFolder = {fs_obj_folder.id_DB}";
                    var all_files = SQLLiteDB.MysqlSelectReader(sql_files, Program.Conn);
                    
                    while (all_files.Read())
                    {
                        TelegramBotFSObject fs_obj_file = new TelegramBotFSObject();
                        fs_obj_file.Attributes = FileAttributes.Normal;
                        fs_obj_file.Name = all_files[1].ToString();
                        fs_obj_file.id_DB = Convert.ToInt32(all_files[0]);
                        _objid = Convert.ToInt32(all_files[0]);
                        while (_fstree.ContainsKey(_objid)) { _objid++; }
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

        private void ParseTreeFromJson()
        {
            String _jc = File.ReadAllText(JsonPath);
            if (String.IsNullOrWhiteSpace(_jc.Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "")))
            {
                _fstree.Clear();
                _fspaths.Clear();
                reverse_search.Clear();
                InitializeFS();
                List<String> _packed_fs_obj = new List<string>();
                foreach (var ck in _fstree.Keys)
                {
                    _packed_fs_obj.Add(_fstree[ck].PackJson());
                }
                String _json = JsonConvert.SerializeObject(_packed_fs_obj);
                File.WriteAllText(JsonPath, _json);
                return;
            }
            List<String> _fsobjects = JsonConvert.DeserializeObject<List<String>>(_jc);
            //Каждая строка в списке - сериализованный элемент TelegramBotFSObject

            List<TelegramBotFSObject> _not_processed_objects = new List<TelegramBotFSObject>();
            foreach (var ce in _fsobjects)
            {
                TelegramBotFSObject fsObject = new TelegramBotFSObject();
                fsObject.UnpackJson(ce);
                if (fsObject.Name == "/" && fsObject.Parent == 0)
                {
                    //Filesystem root
                    _FSRoot = fsObject;
                    _fstree.Add(0, fsObject); //У корневой папки ObjectID = 0 и Parent = 0
                    _fspaths.Add("\\");
                    continue;
                }
                //Regular object
                //if (_fstree.ContainsKey(fsObject.Parent))
                //{
                    //Всё в порядке, родитель уже найден
                    _fstree.Add(fsObject.ObjectID, fsObject);
                    String _ap = GetPathById(fsObject.ObjectID);
                    _fspaths.Add(_ap);
                    reverse_search.Add(_ap, fsObject.ObjectID);
                    continue;
                //}
                //else
                //{
                //    //Родитель объекта ещё не найден, объект потерян и будет обработан позже
                //    _not_processed_objects.Add(fsObject);
                //    continue;
                //}
            }
            //Process missing objects
            //bool _object_changed = true;
            //while (_object_changed)
            //{
            //    _object_changed = false;
            //    foreach (var ci in _not_processed_objects)
            //    {
            //        if (_fstree.ContainsKey(ci.Parent))
            //        {
            //            //Всё в порядке, родитель уже найден
            //            _fstree.Add(ci.ObjectID, ci);
            //            String _ap = GetPathById(ci.ObjectID);
            //            _fspaths.Add(_ap);
            //            reverse_search.Add(_ap, ci.ObjectID);
            //            _not_processed_objects.Remove(ci);
            //            _object_changed = true;
            //        }
            //    }
            //}
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
            int _objid = new Random().Next();
            while (_fstree.ContainsKey(_objid)) { _objid = new Random().Next(); }
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
            reverse_search.Add(path, _objid);
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
                _p_dir = _p_dir.Remove(_p_dir.Length - 2);
            }
            fsobj.DataLocation = $"{_p_dir}\\{gn}";
            fsobj.Parent = reverse_search[_p_dir];
            Directory.CreateDirectory(RootDataDirectory + ((_p_dir == "" || _p_dir == "\\") ? "" : _p_dir) + gn);
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
    }
}
