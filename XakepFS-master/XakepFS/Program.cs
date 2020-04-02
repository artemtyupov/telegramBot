using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace TelegramBotFS
{

    class Program
    {
        static public string root_path = "";
        static public string username = "";
        private static readonly string pathToDB = @"URI=file:C:\Users\Artem\Desktop\Bot\TelegramBot\database.db";
        public static readonly SQLiteConnection Conn = SQLLiteDB.OpenMysqlConnection(pathToDB);

        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                root_path = args[0];
                username = args[1];
                DokanNet.Dokan.Unmount('M');
                DokanNet.Dokan.Mount(new TelegramBotFSClass(), "M:\\");
            }
        }
    }
}
