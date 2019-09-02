using System;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using MySql.Data.MySqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public static class Funcs
    {
        public static List<string> GetListFolders(int idUser, MySqlConnection conn)
        {
            var namesFolders = new List<string> {"<- Back"};
            conn.Open();
            var idStorage = GetIdStorageFromIdUser(idUser, conn);
            var sqlToShowFolders = "";
            sqlToShowFolders = $"SELECT Name FROM Folders WHERE idStorage = {idStorage}";
            var command = new MySqlCommand(sqlToShowFolders, conn);
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
        
        public static List<string> GetListFiles(int idUser, int idFolder, MySqlConnection conn)
        {
            var namesFolders = new List<string> {"<- Back"};
            conn.Open();
            var idStorage = GetIdStorageFromIdUser(idUser, conn);
            var sqlToShowFolders = "";
            sqlToShowFolders = $"SELECT Name FROM Files WHERE idFolder = {idFolder}";
            var command = new MySqlCommand(sqlToShowFolders, conn);
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
            
        public static List<string> GetListStorages(int idUser, MySqlConnection conn)
        {
            var namesStorages = new List<string> {"<- Back"};
            conn.Open();
            var sqlToShowFolders = "";
            sqlToShowFolders = $"SELECT Name FROM Storage WHERE idUser = {idUser}";
            var command = new MySqlCommand(sqlToShowFolders, conn);
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

        public static int GetIdStorageFromIdUser(int idUser, MySqlConnection conn)
        {
            var idStorage = 0;
            var sql = $"SELECT id FROM Storage WHERE idUser = {idUser} and name = \"{Program._selectedStorage}\"";
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                idStorage = Convert.ToInt32(reader[0]);
            }

            reader.Close();
            return idStorage;
        }
        
        public static int GetIdFolderFromIdStorage(int idStorage, MySqlConnection conn)
        {
            var idFolder = 0;
            var sql = $"SELECT id FROM Folders WHERE name = \"{Program._selectedButton}\" and idStorage = {idStorage}";
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                idFolder = Convert.ToInt32(reader[0]);
            }

            reader.Close();
            return idFolder;
        }
        
        public static int GetIdUserFromUsername(string username, MySqlConnection conn)
        {
            var idUser = 0;
            var sql = $"SELECT id FROM User WHERE name = \"{username}\"";
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                idUser = Convert.ToInt32(reader[0]);
            }

            reader.Close();
            return idUser;
        }
        
        public static string GetNewShareKey(MySqlConnection conn)
        {
            var shareKey = Guid.NewGuid().ToString();
            var sqlCheck =$"Select idShared FROM Storage";
            var command = new MySqlCommand(sqlCheck, conn);
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

        public static void Registration(string name, MySqlConnection conn)
        {
            conn.Open(); 
            var sqlToInsertNewUser = $"INSERT INTO User (name) VALUES (\"{name}\")";
            var command = new MySqlCommand(sqlToInsertNewUser, conn);
            command.ExecuteNonQuery();
            conn.Close();
        }
         
        public static bool Authorize(string name, MySqlConnection conn)
        {
            conn.Open();
            var sqlName = $"Select * FROM User WHERE name = \"{name}\"";
            var command = new MySqlCommand(sqlName, conn);
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

        public static string GetShareKey(string userName, string storageName, MySqlConnection conn)
        {
            conn.Open();
            var idUser = GetIdUserFromUsername(userName, conn);
            var sqlName = $"Select idShared FROM Storage WHERE name = \"{storageName}\" and idUser = {idUser}";
            var command = new MySqlCommand(sqlName, conn);
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

        public static bool CheckShareKey(string key, MySqlConnection conn)
        {
            conn.Open();
            var sqlKey = $"Select * FROM Storage WHERE idShared = \"{key}\"";
            var command = new MySqlCommand(sqlKey, conn);
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

        public static void ShareProcess(Message message, MySqlConnection conn)
        {
            conn.Open();
            //Add copy storage
            var sqlKey = $"Select Name FROM Storage WHERE idShared = \"{message.Text}\"";
            var command = new MySqlCommand(sqlKey, conn);
            var reader = command.ExecuteReader();
            reader.Read();
            var name = reader[0];
            reader.Close();

            var idUser = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM User WHERE name = \"{message.Chat.Username}\"", Program.Conn));
            var shareKey = Funcs.GetNewShareKey(Program.Conn);
            Database.MysqlDeleteOrInsert($"INSERT INTO Storage (Name, idUser, idShared) VALUES(\"{name + " (Shared)"}\", {idUser}, \"{shareKey}\");", Program.Conn);

            //Add folders and files
            var idStorageOld = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM storage WHERE idShared = \"{message.Text}\"", Program.Conn));
            sqlKey = $"Select Name FROM Folders WHERE idStorage = \"{idStorageOld}\"";
            command = new MySqlCommand(sqlKey, conn);
            var readerFolder = command.ExecuteReader();
            List<string> namesFolder = new List<string>();
            while (readerFolder.Read())
            {
                namesFolder.Add(readerFolder.GetString(0));
            }
            readerFolder.Close();
            var idStorageNew = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM storage WHERE idShared = \"{shareKey}\"", Program.Conn));
            foreach (var val in namesFolder)
            {
                Database.MysqlDeleteOrInsert($"INSERT INTO Folders (idStorage, Name) VALUES({idStorageNew}, \"{val}\");", Program.Conn);
                var idFolderOld = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM Folders WHERE idStorage = {idStorageOld} and Name = \"{val}\"", Program.Conn));
                var idFolderNew = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM Folders WHERE idStorage = {idStorageNew} and Name = \"{val}\"", Program.Conn));
                sqlKey = $"Select idMessage, Name, idChat FROM Files WHERE idFolder = \"{idFolderOld}\"";
                command = new MySqlCommand(sqlKey, conn);
                var readerFile = command.ExecuteReader();
                var mapFiles = new Dictionary<int, Dictionary <int, string>>();
                var dict = new Dictionary<int, string>();

                while (readerFile.Read())
                {
                    dict.Add(readerFile.GetInt32(2), readerFile.GetString(1));
                    mapFiles.Add(readerFile.GetInt32(0), dict);
                }
                readerFile.Close();

                foreach (var pair in mapFiles.Zip(dict, Tuple.Create))
                {
                    Database.MysqlDeleteOrInsert($"INSERT INTO Files (idFolder, idMessage, Name, idChat) VALUES ({idFolderNew}, {pair.Item1.Key}, \"{pair.Item2.Value}\", {pair.Item2.Key})", Program.Conn);
                }

            }
            conn.Close();
        }
    }
}