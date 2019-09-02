using MySql.Data.MySqlClient;

namespace TelegramBot
{
    public static class Database
    {
        public static MySqlConnection OpenMysqlConnection(string dbName)
        {
            string connString = $@"Server=127.0.0.1;port=3306;Database={dbName};Uid=root;password=Tosha007";
            var conn = new MySqlConnection(connString);
            return conn;
        }

        public static string MysqlSelect(string sql, MySqlConnection conn)
        {
            var result = "";
            var command = new MySqlCommand(sql, conn);
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
        
        public static void MysqlDeleteOrInsert(string sql, MySqlConnection conn)
        {
            var command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }
    }
}