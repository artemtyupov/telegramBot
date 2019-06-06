using System;
using System.Linq;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MySql.Data.MySqlClient;
//reddys для inmemmory database для использования флагов
//для тестов https://github.com/TelegramBots/Telegram.Bot/tree/master/test/Telegram.Bot.Tests.Integ
//одинаковые название файлов и папок
//сделать функцию для создания нескольких хранилищ для пользователя
//в функции регистрации вынести функцию созданию хранилища
namespace TelegramBot
{
    public static class Program
    {
        private const string Token = "632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY";
        private static readonly Telegram.Bot.TelegramBotClient Bot = new Telegram.Bot.TelegramBotClient(Token);
        private static readonly MySqlConnection Conn = Database.OpenMysqlConnection("mydb2"); 
        private static readonly InlineKeyboardMarkup InlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] // first row
            {
                InlineKeyboardButton.WithCallbackData("Create folder"),
                InlineKeyboardButton.WithCallbackData("Delete folder"),
                InlineKeyboardButton.WithCallbackData("Rename folder"),
                InlineKeyboardButton.WithCallbackData("Show folders"),
            },
            new[] // second row
            {
                InlineKeyboardButton.WithCallbackData("Share storage"),
                InlineKeyboardButton.WithCallbackData("Get shared storage"),
            }
        });
        
        private static bool _flagCreate;
        private static bool _flagDelete;
        private static bool _flagShare;
        private static bool _flagRename;
        private static bool _flagShow;
        private static bool _flagAddData;
        private static bool _flagGetData;
        private static string _selectedButton = "";
        
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
            if (message.Type == MessageType.Text)
            {
                switch (message.Text.Split(' ').First())
                {
                    case "/help":
                        const string usage = @"Usage:
                                               /start - to start
                                               /help - to help";

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            usage,
                            replyMarkup: new ReplyKeyboardRemove());
                        break;

                    case "/start":
                        var loginFlag = Funcs.Authorize(message.Chat.Username, Conn);
                        if (!loginFlag)
                        {
                            Funcs.Registration(message.Chat.Username, Conn);
                        }
                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            usage + "\n Choose",
                            replyMarkup: InlineKeyboard);
                        break;

                    default:

                        if (_flagCreate)
                        {
                            _flagCreate = false;
                            if (message.Text != "<- Back")
                            {
                                var idUser = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM User WHERE name = \"{message.Chat.Username}\"", Conn));
                                var idStorage = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM storage WHERE idUser = {idUser}", Conn));
                                Database.MysqlDeleteOrInsert($"INSERT INTO Folders (idStorage, Name) VALUES({idStorage}, \"{message.Text}\");", Conn);
                            }
                        }
                        if (_flagRename)
                        {
                            _flagRename = false;
                            Database.MysqlDeleteOrInsert($"UPDATE Folders SET Name = {message.Text} WHERE Name = \"{_selectedButton}\"", Conn);
                        }

                        if (_flagAddData)
                        {
                            _flagAddData = false;
                        }

                        if (_flagShare)
                        {
                            //Скачиваем зашаренное хранилище
                            _flagShare = false;
                        }

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Choose",
                            replyMarkup: InlineKeyboard);
                        break;
                }
            }
            else
            {
                if (!_flagAddData) return;
                _flagAddData = false;
                var idFolder = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM Folders WHERE Name = \"{_selectedButton}\"", Conn));
                //TODO Caption != name, скорее всего нужно делать switch по возможным типам файла и оттуда высовывать name вместо caption
                Database.MysqlDeleteOrInsert($"INSERT INTO Files (idFolder, idMessage, Name) VALUES ({idFolder}, {message.MessageId}, \"{message.Caption}\")", Conn);    
                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Choose",
                    replyMarkup: InlineKeyboard);
            }
        }
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            
            switch (callbackQuery.Data)
            {
                case "Create folder":
                    _flagCreate = true;
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Enter new folder name: ");
                    
                    break;
                
                case "Delete folder":
                    _flagDelete = true;
                    var buttonItem = Funcs.GetListFolders(-1, Conn).ToArray();
                    var keyboardMarkup = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Choose to delete {callbackQuery.Data} folder",
                        replyMarkup: keyboardMarkup);
                    break;
                case "Rename folder":
                    _flagRename = true;
                    buttonItem = Funcs.GetListFolders(-1, Conn).ToArray();
                    keyboardMarkup = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Choose to rename {callbackQuery.Data} folder",
                        replyMarkup: keyboardMarkup);
                    break;
                case "Show folders":
                    _flagShow = true;
                    buttonItem =  Funcs.GetListFolders(-1, Conn).ToArray();
                    keyboardMarkup = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Choose",
                        replyMarkup: keyboardMarkup);
                    break;
                case "Add data":
                    _flagAddData = true;
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Add some data");
                    break;
                case "Get data":
                    _flagGetData = true;
                    
                    var idFolder = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM Folders WHERE Name = \"{_selectedButton}\"", Conn));
                    var namesFiles = Funcs.GetListFolders(idFolder, Conn);
                    var keyboardMarkupNew = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(namesFiles.ToArray()));
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);        
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Choose file",
                        replyMarkup: keyboardMarkupNew);
                    break;
                case "<- Back":
                    _flagDelete = false;
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Choose",
                        replyMarkup: InlineKeyboard);
                    break;
                case "Share storage":
                    var shareKey = Guid.NewGuid().ToString();

                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Your share key: {shareKey}",
                        replyMarkup: InlineKeyboard);
                    
                    break;
                case "Get shared storage":
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Enter share key: ");
                    
                    break;
                default:
                    if (_flagDelete)
                    {
                        _flagDelete = false;
                        
                        if (callbackQuery.Data != "<- Back")
                        {
                            Database.MysqlDeleteOrInsert($"DELETE FROM Folders WHERE Name = \"{callbackQuery.Data}\" ", Conn);
                            await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                            await Bot.SendTextMessageAsync(
                                callbackQuery.Message.Chat.Id,
                                "Choose",
                                replyMarkup: InlineKeyboard);
                        }
                    }
                    
                    if (_flagRename)
                    {
                        
                        if (callbackQuery.Data != "<- Back")
                        {
                            _selectedButton = callbackQuery.Data;
                            await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                            await Bot.SendTextMessageAsync(
                                callbackQuery.Message.Chat.Id,
                                "Choose new name:");
                        }
                    }

                    if (_flagShow)
                    {
                        _flagShow = false;
                        if (callbackQuery.Data != "<- Back")
                        {
                            _selectedButton = callbackQuery.Data;
                            buttonItem =  new[] {"<- Back", "Add data", "Get data"};
                            keyboardMarkup = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
                            await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                            await Bot.SendTextMessageAsync(
                                callbackQuery.Message.Chat.Id,
                                "Choose",
                                replyMarkup: keyboardMarkup);
                        }
                    }

                    if (_flagGetData)
                    {
                        _flagGetData = false;
                        var idMessage = Convert.ToInt32(Database.MysqlSelect($"SELECT idMessage FROM Files WHERE Name = \"{callbackQuery.Data}\"", Conn));
                        await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                        await Bot.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            "Your file",
                            replyToMessageId: idMessage,
                            replyMarkup: InlineKeyboard
                            );
                    }
                    break;
            }
        }
    }
    
} 