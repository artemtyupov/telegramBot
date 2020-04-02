using System.Data.SQLite;

namespace TelegramBotFS
{
    public static class SQLLiteDB
    {
        public static SQLiteConnection OpenMysqlConnection(string pathToDB)
        {
            SQLiteConnection objConnection = new SQLiteConnection(pathToDB);
            return objConnection;
        }
        
        public static SQLiteDataReader MysqlSelectReader(string sql, SQLiteConnection conn)
        {
            var result = "";
            var command = new SQLiteCommand(sql, conn);
            var reader = command.ExecuteReader();
            
            return reader;
        }

        public static void MysqlDeleteOrInsert(string sql, SQLiteConnection conn)
        {
            var command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }
    }
}