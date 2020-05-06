using System.Data.SQLite;
using System;

namespace TelegramBotFS
{
    public static class SQLLiteDB
    {
        public static SQLiteConnection OpenConnection(string pathToDB)
        {
            SQLiteConnection objConnection = new SQLiteConnection(pathToDB);
            return objConnection;
        }
        
        public static SQLiteDataReader SelectReader(string sql, SQLiteConnection conn)
        {
            var command = new SQLiteCommand(sql, conn);
            var reader = command.ExecuteReader();
            
            return reader;
        }

        public static void DeleteOrInsert(string sql, SQLiteConnection conn)
        {
            var command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public static string Select(string sql, SQLiteConnection conn)
        {
            var result = "";
            var command = new SQLiteCommand(sql, conn);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    result += reader[i].ToString();
                }
            }
            reader.Close();
            return result;
        }

        public static bool CheckOnFileExist(string path, SQLiteConnection conn)
        {
            //path = //test_storage1//test//t.txt
            String[] elements = path.Split("\\".ToCharArray()); // ["", "test_storage1", "test1", "test2", "t.txt"]
            var idStorage = $"SELECT id FROM Storage WHERE name = \"{elements[1]}\"";
            int idFolder = -1;
            try
            {
                for (int i = 2; i < elements.Length - 1; i++)
                {
                    if (idFolder != -1)
                    {
                        var sql1 = $"SELECT id FROM Folders WHERE idStorage = {idStorage} and name = \"{elements[i]}\"";
                        idFolder = Convert.ToInt32(SQLLiteDB.Select(sql1, Program.Conn)); //id test1
                    }
                    else
                    {
                        var sql2 = $"SELECT id FROM Folders where idStorage = {idStorage} and idFolder = {idFolder} and name = \"{elements[i]}\""; //id test2
                        idFolder = Convert.ToInt32(SQLLiteDB.Select(sql2, Program.Conn));
                    }
                }
                var sql = $"SELECT name FROM Files WHERE idFolder = {idFolder} and idStorage = {idStorage}";
                var reader = SQLLiteDB.Select(sql, Program.Conn);

                if (reader.Contains(elements[elements.Length - 1]))
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }
    }
}