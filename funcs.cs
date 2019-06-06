using System;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TelegramBot
{
    public static class Funcs
    {
        private static readonly MySqlConnection Conn = Database.OpenMysqlConnection("mydb2");

        public static List<string> GetListFolders(int idFolder, MySqlConnection conn)
        {
            var namesFolders = new List<string> {"<- Back"};
            conn.Open();
            var sqlToShowFolders = "";
            if (idFolder == -1)
                sqlToShowFolders = "SELECT Name FROM Folders";
            else
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
    
        
        public static void Registration(string name, MySqlConnection conn)
        {
            conn.Open(); 
            
            var sqlToInsertNewUser = $"INSERT INTO User (name) VALUES (\"{name}\")";
            var command = new MySqlCommand(sqlToInsertNewUser, conn);
            command.ExecuteNonQuery();
            var shareKey = Guid.NewGuid().ToString();
            var sqlCheck =$"Select idShared FROM Storage WHERE idShared = \"{shareKey}\"";
            command = new MySqlCommand(sqlCheck, conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
                while (reader[0].ToString() == shareKey.ToString())
                    shareKey = Guid.NewGuid().ToString();
            else
            {
                reader.Close();
                conn.Close();
                var idUser = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM User WHERE name = \"{name}\"", conn));
                Database.MysqlDeleteOrInsert($"INSERT INTO Storage (idUser, idShared) VALUES ({idUser}, \"{shareKey}\")", conn);
                
            }
            
            
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
    }
}