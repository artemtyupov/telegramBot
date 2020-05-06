using System;
using System.Linq;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Telegram.Bot.Types.InputFiles;
using System.Net;
using SharpCompress.Readers;
using SharpCompress.Common;


namespace TelegramBot
{
    public class Program
    {
        public static readonly InlineKeyboardMarkup InlKey = new InlineKeyboardMarkup(new[]
        {
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("+"),
                InlineKeyboardButton.WithCallbackData("-"),
            },
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("Rename folder"),
                InlineKeyboardButton.WithCallbackData("Show folders"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("<- Back"),
                InlineKeyboardButton.WithCallbackData("Share storage"),
            }
        });

        private static CContext context;
        private static readonly string pathToDB = @"URI=file:C:\Users\Artem\Desktop\Bot\TelegramBot\database.db";
        private static readonly string Token = "632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY";
        private static readonly Telegram.Bot.TelegramBotClient Bot = new Telegram.Bot.TelegramBotClient(Token);
        public static readonly SQLiteConnection Conn = SQLLiteDB.OpenMysqlConnection(pathToDB);
        public static string _selectedButton = "";
        public static string _selectedStorage = "";
        public static bool FSState = false;
        public static string root_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static void Main()
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
        }

        
        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null) return;
            var chat = Bot.GetChatAsync(message.Chat.Id);
            if (message.Type == MessageType.Text)
            {
                switch (message.Text.Split(' ').First())
                {
                    case "/help":
                        const string usage = @"Usage:\n
                                               /start - to start\n
                                               /help - to help\n
                                               /fson, /fsoff - to on/off filesystem(only for Windows).\n";

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            usage,
                            replyMarkup: new ReplyKeyboardRemove());
                        break;

                    case "/fson":
                        
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            
                            string selectedPath = "";

                            Thread t = new Thread((ThreadStart)(() => {
                                OpenFileDialog saveFileDialog1 = new OpenFileDialog();

                                saveFileDialog1.Filter = "Exe Files (*.exe)|*.exe";
                                saveFileDialog1.FilterIndex = 2;
                                saveFileDialog1.RestoreDirectory = true;

                                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                                {
                                    selectedPath = saveFileDialog1.FileName;
                                }
                            }));

                            // Run your code from a thread that joins the STA Thread
                            t.SetApartmentState(ApartmentState.STA);
                            t.Start();
                            t.Join();

                            string cut_path = selectedPath.Replace("\\TelegramBotFS.exe", "");

                            if (selectedPath.Contains("TelegramBotFS.exe"))
                                Process.Start(selectedPath, cut_path + " \"" + message.Chat.Username);

                            FSState = true;
                            await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Succsessfully. Disk M:/ mounted.\n" +
                            "All your storages available there.\n" +
                            "After completion, send storage rar archive in chat, for save all changes.\n" +
                            "After these write command /fsoff \n");
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Error: working only for Windows:");
                        }
                        break;

                    case "/fsoff":
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            DokanNet.Dokan.Unmount('M');
                            FSState = false;

                            await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Succsessfully. Disk M:/ unmounted.");
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Error: working only for Windows:");
                        }
                        break;

                    case "/start":
                        context = new CContext(new CMenuStorage());
                        var loginFlag = Funcs.Authorize(message.Chat.Username, Conn);
                        if (!loginFlag)
                        {
                            Funcs.Registration(message.Chat.Username, message.Chat.Id, Conn);
                        }
                        try
                        {
                            Conn.Open();
                            await Bot.SendTextMessageAsync(
                                message.Chat.Id,
                                "\n Choose action:",
                                replyMarkup: context.GetInlineKeyboardFromContext(Funcs.GetIdUserFromUsername(message.From.Username, Conn)));
                            Conn.Close();
                        }
                        catch { }
                        break;

                    default:
                        if (context == null)
                            return;
                        context.ActionMsgContext(Bot, message);
                        break;
                }
            }
            else
            {
                if (context == null && !FSState)
                    return;

                if (!FSState)
                    context.ActionMsgContext(Bot, message);
                else
                {
                    if (message.Type != MessageType.Document)
                        await Bot.SendTextMessageAsync(
                                message.Chat.Id,
                                "\n FS mode on." +
                                "\n Please sent storage rar archive");
                    else
                    {

                        string idFile = message.Document.FileId;

                        //отправка сообщения
                        // Create a request for the URL. 		
                        WebRequest request = WebRequest.Create($"https://api.telegram.org/bot{Token}/getFile?file_id={idFile}");
                        request.Credentials = CredentialCache.DefaultCredentials;
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Console.WriteLine(response.StatusDescription);
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();
                        response.Close();
                        //"{\"ok\":true,\"result\":{\"file_id\":\"BQACAgIAAxkBAAINiV6iuefh4VCLiUI7XFbe9udZM5TsAAIEBQAC5_sYSflky45MKvdUGQQ\",\"file_unique_id\":\"AgADBAUAAuf7GEk\",\"file_size\":5095564,\"file_path\":\"documents/file_6.rar\"}}"
                        var arr = responseFromServer.Split('\"');
                        foreach (var item in arr)
                        {
                            if (item.Contains("documents"))
                            {
                                request = WebRequest.Create($"https://api.telegram.org/file/bot{Token}/{item}");
                                response = (HttpWebResponse)request.GetResponse();
                                dataStream = response.GetResponseStream();
                                
                                var reader1 = ReaderFactory.Open(dataStream);
                                while (reader1.MoveToNextEntry())
                                {
                                    if (!reader1.Entry.IsDirectory)
                                    {
                                        Console.WriteLine(reader1.Entry.Key);
                                        reader1.WriteEntryToDirectory(root_path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                    }
                                }
                            }
                        }
                        await UpdateStoragesFromArchive(message);
                    }
                }
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender,
            CallbackQueryEventArgs callbackQueryEventArgs)
        {
            if (context == null) return;
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            if (callbackQuery == null) return;
            Conn.Open();
            var idUser = Funcs.GetIdUserFromUsername(callbackQuery.From.Username, Conn);
            Conn.Close();

            EnumActions.EActions act = EnumActions.EActions.MenuStorage;
            if (callbackQuery.Data == "+" || callbackQuery.Data == "-")
            {
                int id = context.getID();

                if (callbackQuery.Data == "+")
                {
                    switch (id)
                    {
                        case 10:
                        case 7:
                        case 8:
                        case 11:
                        case 9:
                            act = EnumActions.EActions.CreateStorage;
                            break;

                        case 2:
                        case 12:
                        case 15:
                        case 14:
                        case 3:
                            act = EnumActions.EActions.CreateFolder;
                            break;
                    }
                }
                else
                {
                    switch (id)
                    {
                        case 10:
                        case 7:
                        case 8:
                        case 11:
                        case 9:
                            act = EnumActions.EActions.DeleteStorage;
                            break;

                        case 2:
                        case 12:
                        case 15:
                        case 14:
                        case 3:
                            act = EnumActions.EActions.DeleteFolder;
                            break;
                    }
                }
            }
            else
                act = EnumActions.GetEnumActionFromString(callbackQuery.Data);

            switch (act)
            {
                case EnumActions.EActions.DeleteStorage:
                case EnumActions.EActions.RenameStorage:
                case EnumActions.EActions.ShowStorage:
                case EnumActions.EActions.DeleteFolder:
                case EnumActions.EActions.RenameFolder:
                case EnumActions.EActions.ShowFolder:
                case EnumActions.EActions.GetData:
                    context.SavePrevState();
                    context.ChangeState(EnumActions.GetStateObjectFromEAction(act));
                    try
                    {
                        await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                        await Bot.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            EnumActions.GetStringFromEAction(act),
                            replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    }
                    catch { }
                    break;

                case EnumActions.EActions.CreateStorage:
                case EnumActions.EActions.CreateFolder:
                case EnumActions.EActions.CreateFolderInFolder:
                case EnumActions.EActions.AddData:
                case EnumActions.EActions.GetSharedStorage:
                    context.SavePrevState();
                    context.ChangeState(EnumActions.GetStateObjectFromEAction(act));
                    try
                    {
                        await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                        await Bot.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            EnumActions.GetStringFromEAction(act));
                    }
                    catch { }
                    break;
                    
                case EnumActions.EActions.Back: 
                    context.ChangeOnPrevState();
                    try
                    { 
                        await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                        await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        EnumActions.GetStringFromEAction(act),
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    }
                    catch { }
                    break;

                case EnumActions.EActions.ShareStorage:
                    context.SavePrevState();
                    context.ChangeState(new CShareState());
                    context.ActionQueryContext(Bot, callbackQuery);
                    context.ChangeState(new CMainMenuState());
                    break;

                default:
                    if (context.GetTypeState().GetType() == new CShowState().GetType())
                    {
                        context.SavePrevState();
                        context.ChangeState(new CChoosedToShowState());
                    }

                    context.ActionQueryContext(Bot, callbackQuery);
                    break;
            }
        }

        private static async Task UpdateStoragesFromArchive(Telegram.Bot.Types.Message msg)
        {
            var files = Directory.GetFiles(Program.root_path + "\\storage");
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                var dir_filename = Program.root_path + "\\storage\\" + name;
                using (FileStream stream = File.OpenRead(dir_filename))
                {
                    var x = stream.Length;
                    if (x != 0)
                        await Bot.SendDocumentAsync(msg.Chat.Id, stream);
                }

                Program.Conn.Open();
                //TODO находим файл по хешу, также нужно каждый файл переотправить обратно в телегу, записать его fileid и прочее. Еще разобраться с разрешением и именами,
                //стоит это в бд сохранять и тут хеши менять на нормалньые имена при отправке
                //int idFile = Convert.ToInt32(SQLLiteDB.MysqlSelect($"SELECT id FROM Files WHERE FSHash = \"{name}\"", Program.Conn));
                //SQLLiteDB.MysqlDeleteOrInsert($"UPDATE Files SET Name = \"{msg.Text}\" WHERE Name = \"{Program._selectedButton}\"", Program.Conn);
                Program.Conn.Close();
            }
        }
    }
}


//https://stackoverflow.com/questions/34170546/getfile-method-in-telegram-bot-api
//1. https://api.telegram.org/bot632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY/getUpdates - не нужен, файл ид уже будем хранить в бд
//2. https://api.telegram.org/bot632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY/getFile?file_id=AgACAgIAAxkBAAIMIV6GVty_BNsJGB2EJZAMBz8aLr7qAALvrTEblQoxSLCYYcGwWrrENqbskS4AAwEAAwIAA3kAA4tZAAIYBA
//3. https://api.telegram.org/file/bot632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY/photos/file_3.jpg



//TODO:
/*
 * 
 * 1. Поменять функции Read\Write file в FSClass.
 * Write запускается при - изменение существующего файла\копировании нового.
 * Соответственно при копировании нужно:
 * отправлять файл в телеграмм и сохранять данные в бд
 * При изменении файла, просто добиться успешного сохранения.
 * 
 * 
 * 2. Добавить наконец-то удаление файлов в клиенте телеги.
 * 
 * 3. Перенести все отсюда в гит(включая туду) и закрыть нужные ишью.
 * 
 * 4. Проводить работу по несколько часов в день по нужным ишью!
 * 
 * 5. Нужно добавить поддержку того, чтобы в клиенте телеги можно было создавать папку в папке.!!!!
 * 
 * 6. Хранить chatid в бд к юзернейму!
 * */
