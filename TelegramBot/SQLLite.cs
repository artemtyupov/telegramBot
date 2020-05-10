using System.Data.SQLite;

namespace TelegramBot
{
    public static class SQLLiteDB
    {
        public static Program Program
        {
            get => default(Program);
            set
            {
            }
        }

        public static SQLiteConnection OpenSQLiteConnection(string pathToDB)
        {
            SQLiteConnection objConnection = new SQLiteConnection(pathToDB);
            return objConnection;
        }
        
        public static string SQLiteSelect(string sql, SQLiteConnection conn)
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

        public static void SQLiteDeleteOrInsert(string sql, SQLiteConnection conn)
        {
            var command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }
    }
}