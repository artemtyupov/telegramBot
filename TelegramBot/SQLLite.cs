using System.Data.SQLite;

namespace TelegramBot
{
    public static class SQLLiteDB
    {
        public static SQLiteConnection OpenMysqlConnection(string pathToDB)
        {
            SQLiteConnection objConnection = new SQLiteConnection(pathToDB);
            return objConnection;
        }
        
        public static string MysqlSelect(string sql, SQLiteConnection conn)
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

        public static void MysqlDeleteOrInsert(string sql, SQLiteConnection conn)
        {
            var command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }
    }
}