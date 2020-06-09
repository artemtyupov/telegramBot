using System;
using System.IO;
using System.Net;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotFS
{

    class Program
    {
        static public string root_path = "";
        static public string username = "";
        static public readonly string Token = "632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY";
        private static readonly string pathToDB = @"URI=file:C:\Users\Artem\Desktop\Bot\TelegramBot\database.db";
        public static readonly SQLiteConnection Conn = SQLLiteDB.OpenConnection(pathToDB);


        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                root_path = args[0];
                username = args[1];
                if (Directory.Exists(root_path + "\\storage"))
                    Directory.Delete(root_path + "\\storage", true);

                Thread.Sleep(1000); //any time need after delete to create new dir(i dont know why)

                Directory.CreateDirectory(root_path + "\\storage");
                DokanNet.Dokan.Unmount('M');
                DokanNet.Dokan.Mount(new TelegramBotFSClass(), "M:\\");
            }
        }
    }
}