using MySql.Data.MySqlClient;

namespace TelegramBot
{
    public static class Database
    {
        public static MySqlConnection OpenMysqlConnection(string dbName)
        {
            string connString = $@"Server=localhost;port=3306;Database={dbName};Uid=root;Sslmode=none;";

            var conn = new MySqlConnection(connString);
            
            return conn;
        }

        public static string MysqlSelect(string sql, MySqlConnection conn)
        {
            conn.Open();
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
            conn.Close();
            return result;
        }
        
        public static void MysqlDeleteOrInsert(string sql, MySqlConnection conn)
        {
            conn.Open();
            var command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();
            conn.Close();
        }
    }
    
    
}