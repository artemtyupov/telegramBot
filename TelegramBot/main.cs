using System;
using System.Linq;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MySql.Data.MySqlClient;
using System.Data.SQLite;

namespace TelegramBot
{
    public class Program
    {
        public static readonly InlineKeyboardMarkup InlKey = new InlineKeyboardMarkup(new[]
        {
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("Create folder"),
                InlineKeyboardButton.WithCallbackData("Delete folder"),
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
        private static readonly string databaseName = "TelegramBot";
        private static readonly string pathToDB = @"URI=file:C:\Users\Artem\Desktop\Bot\TelegramBot\database.db";
        private static readonly string Token = "632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY";
        private static readonly Telegram.Bot.TelegramBotClient Bot = new Telegram.Bot.TelegramBotClient(Token);
        public static readonly SQLiteConnection Conn = SQLLiteDB.OpenMysqlConnection(pathToDB);
        public static readonly MySqlConnection Conn1 = Database.OpenMysqlConnection(databaseName);
        public static string _selectedButton = "";
        public static string _selectedStorage = "";

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
                        const string usage = @"Usage:
                                               /start - to start
                                               /help - to help";

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            usage,
                            replyMarkup: new ReplyKeyboardRemove());
                        break;

                    case "/start":
                        context = new CContext(new CMenuStorage());
                        var loginFlag = Funcs.Authorize(message.Chat.Username, Conn);
                        if (!loginFlag)
                        {
                            Funcs.Registration(message.Chat.Username, Conn);
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
                if (context == null)
                    return;
                context.ActionMsgContext(Bot, message);
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
            EnumActions.EActions act = EnumActions.GetEnumActionFromString(callbackQuery.Data);
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
    }
}
