using System;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Telegram.Bot.Types;
using System.Data.SQLite;
using System.IO;


namespace TelegramBot
{
    public static class Funcs
    {
        public static Program Program
        {
            get => default(Program);
            set
            {
            }
        }

        public static List<string> GetListFolders(int idUser, int idFolder, SQLiteConnection conn)
        {
            var namesFolders = new List<string> {"<- Back"};
            conn.Open();
            var idStorage = GetIdStorageFromIdUser(idUser, conn);
            var sqlToShowFolders = $"SELECT Name FROM Folders WHERE idStorage = {idStorage} and idFolder = {idFolder}";
            var command = new SQLiteCommand(sqlToShowFolders, conn);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    namesFolders.Add(reader[i].ToString());
                }
            }

            reader.Close();
            conn.Close();
            return namesFolders;
        }

        public static List<string> GetListFiles(int idUser, int idFolder, SQLiteConnection conn)
        {
            var namesFolders = new List<string> {"<- Back"};
            conn.Open();
            var idStorage = GetIdStorageFromIdUser(idUser, conn);
            var sqlToShowFolders = "";
            sqlToShowFolders = $"SELECT Name FROM Files WHERE idFolder = {idFolder}";
            var command = new SQLiteCommand(sqlToShowFolders, conn);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    namesFolders.Add(reader[i].ToString());
                }
            }

            reader.Close();
            conn.Close();
            return namesFolders;
        }
            
        public static List<string> GetListStorages(int idUser, SQLiteConnection conn)
        {
            var namesStorages = new List<string> {"<- Back"};
            conn.Open();
            var sqlToShowFolders = "";
            sqlToShowFolders = $"SELECT Name FROM Storage WHERE idUser = {idUser}";
            var command = new SQLiteCommand(sqlToShowFolders, conn);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    namesStorages.Add(reader[i].ToString());
                }
            }

            reader.Close();
            conn.Close();
            return namesStorages;
        }

        public static int GetIdStorageFromIdUser(int idUser, SQLiteConnection conn)
        {
            var idStorage = 0;
            var sql = $"SELECT id FROM Storage WHERE idUser = {idUser} and name = \"{Program._selectedStorage}\"";
            var command = new SQLiteCommand(sql, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                idStorage = Convert.ToInt32(reader[0]);
            }

            reader.Close();
            return idStorage;
        }
        
        public static int GetIdFolderFromIdStorage(int idStorage, SQLiteConnection conn)
        {
            var idFolder = 0;
            var sql = $"SELECT id FROM Folders WHERE name = \"{Program._selectedButton}\" and idStorage = {idStorage}";
            var command = new SQLiteCommand(sql, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                idFolder = Convert.ToInt32(reader[0]);
            }

            reader.Close();
            return idFolder;
        }
        
        public static int GetIdUserFromUsername(string username, SQLiteConnection conn)
        {
            var idUser = 0;
            var sql = $"SELECT id FROM User WHERE name = \"{username}\"";
            var command = new SQLiteCommand(sql, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                idUser = Convert.ToInt32(reader[0]);
            }

            reader.Close();
            return idUser;
        }
        
        public static string GetNewShareKey(SQLiteConnection conn)
        {
            var shareKey = Guid.NewGuid().ToString();
            var sqlCheck =$"Select idShared FROM Storage";
            var command = new SQLiteCommand(sqlCheck, conn);
            var reader = command.ExecuteReader();
            var flag = true;
            if (reader.Read())
                do
                {
                    shareKey = Guid.NewGuid().ToString();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader[i].ToString() == shareKey.ToString())
                            flag = false;
                    }
                } while (flag == false);

            reader.Close();
            return shareKey;
        }

        public static void Registration(string name, long idChat, SQLiteConnection conn)
        {
            try
            {
                conn.Open();
                var sqlToInsertNewUser = $"INSERT INTO User (name, idChat) VALUES (\"{name}\", {idChat})";
                var command = new SQLiteCommand(sqlToInsertNewUser, conn);
                command.ExecuteNonQuery();
                conn.Close();
            }
            catch {}
        }
         
        public static bool Authorize(string name, SQLiteConnection conn)
        {
            try
            {
                conn.Open();
                var sqlName = $"Select * FROM User WHERE name = \"{name}\"";
                var command = new SQLiteCommand(sqlName, conn);
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    conn.Close();
                    return true;
                }
                reader.Close();
                conn.Close();
                return false;
            }
            catch { return false; }
        }

        public static string GetShareKey(string userName, string storageName, SQLiteConnection conn)
        {
            conn.Open();
            var idUser = GetIdUserFromUsername(userName, conn);
            var sqlName = $"Select idShared FROM Storage WHERE name = \"{storageName}\" and idUser = {idUser}";
            var command = new SQLiteCommand(sqlName, conn);
            var reader = command.ExecuteReader();
            var res = "";
            while (reader.Read())
            {
                res = reader[0].ToString();
            }
            reader.Close();
            conn.Close();
            return res;
        }

        public static IEnumerable<InlineKeyboardButton[]> GetInlineKeyboard(IReadOnlyList<string> stringArray)
        {
            var rows = stringArray.Count / 4;
            var haveMod = false;
            var columns = 4;
            var mod = stringArray.Count % 4;
            if (mod != 0)
            {
                rows++;
                haveMod = true;
            }

            var k = 0;
            var keyboardInline = new InlineKeyboardButton[rows][];
            for (var i = 0; i < rows; i++)
            {
                InlineKeyboardButton[] keyboardButton;

                if (i == rows - 1 && haveMod)
                {
                    keyboardButton = new InlineKeyboardButton[mod];
                    columns = mod;
                }
                else
                {
                    keyboardButton = new InlineKeyboardButton[4];
                }

                for (var j = 0; j < columns; j++)
                {
                    keyboardButton[j] = InlineKeyboardButton.WithCallbackData(stringArray[k]);
                    k++;
                }

                keyboardInline[i] = keyboardButton;
            }
            return keyboardInline;
        }

        public static bool CheckShareKey(string key, SQLiteConnection conn)
        {
            conn.Open();
            var sqlKey = $"Select * FROM Storage WHERE idShared = \"{key}\"";
            var command = new SQLiteCommand(sqlKey, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                conn.Close();
                return true;
            }
            conn.Close();
            return false;
        }

        public static void ShareProcess(Message message, SQLiteConnection conn)
        {
            conn.Open();
            //Add copy storage
            var sqlKey = $"Select Name FROM Storage WHERE idShared = \"{message.Text}\"";
            var command = new SQLiteCommand(sqlKey, conn);
            var reader = command.ExecuteReader();
            reader.Read();
            var name = reader[0];
            reader.Close();

            var idUser = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM User WHERE name = \"{message.Chat.Username}\"", Program.Conn));
            var shareKey = Funcs.GetNewShareKey(Program.Conn);
            SQLLiteDB.SQLiteDeleteOrInsert($"INSERT INTO Storage (Name, idUser, idShared) VALUES(\"{name + " (Shared)"}\", {idUser}, \"{shareKey}\");", Program.Conn);

            //Add folders and files
            var idStorageOld = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM storage WHERE idShared = \"{message.Text}\"", Program.Conn));
            sqlKey = $"Select Name FROM Folders WHERE idStorage = \"{idStorageOld}\"";
            command = new SQLiteCommand(sqlKey, conn);
            var readerFolder = command.ExecuteReader();
            List<string> namesFolder = new List<string>();
            while (readerFolder.Read())
            {
                namesFolder.Add(readerFolder.GetString(0));
            }
            readerFolder.Close();
            var idStorageNew = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM storage WHERE idShared = \"{shareKey}\"", Program.Conn));
            foreach (var val in namesFolder)
            {
                SQLLiteDB.SQLiteDeleteOrInsert($"INSERT INTO Folders (idStorage, idFolder, Name) VALUES({idStorageNew}, {-1}, \"{val}\");", Program.Conn);//TODO изменение при шаринге когда папка в папке
                var idFolderOld = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM Folders WHERE idStorage = {idStorageOld} and Name = \"{val}\"", Program.Conn));
                var idFolderNew = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM Folders WHERE idStorage = {idStorageNew} and Name = \"{val}\"", Program.Conn));
                sqlKey = $"Select idMessage, Name, idChat FROM Files WHERE idFolder = \"{idFolderOld}\"";
                command = new SQLiteCommand(sqlKey, conn);
                var readerFile = command.ExecuteReader();
                var mapFiles = new Dictionary<int, List<KeyValuePair<int, string>>>();
                var dict = new List<KeyValuePair<int, string>>();

                while (readerFile.Read())
                {
                    dict.Add(new KeyValuePair<int, string>(readerFile.GetInt32(2), readerFile.GetString(1)));
                    mapFiles.Add(readerFile.GetInt32(0), dict);
                }
                readerFile.Close();

                foreach (var pair in mapFiles.Zip(dict, Tuple.Create))
                {
                    SQLLiteDB.SQLiteDeleteOrInsert($"INSERT INTO Files (idFolder, idMessage, Name, idChat) VALUES ({idFolderNew}, {pair.Item1.Key}, \"{pair.Item2.Value}\", {pair.Item2.Key})", Program.Conn);
                }

            }
            conn.Close();
        }

        
    }
}