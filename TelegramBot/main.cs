using System;
using System.Linq;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MySql.Data.MySqlClient;

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
        private static readonly string Token = "632773726:AAE6L2o9zENbHrLKSTCByB_z4rpQ1-ZuMlY";
        private static readonly Telegram.Bot.TelegramBotClient Bot = new Telegram.Bot.TelegramBotClient(Token);
        public static readonly MySqlConnection Conn = Database.OpenMysqlConnection(databaseName);
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
                        Conn.Open();
                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "\n Choose action:",
                            replyMarkup: context.GetInlineKeyboardFromContext(Funcs.GetIdUserFromUsername(message.From.Username, Conn)));
                        Conn.Close();
                        break;
                    default:
                        context.ActionMsgContext(Bot, message);
                        break;
                }
            }
            else
            {
                context.ActionMsgContext(Bot, message);
            }
        }


        private static async void BotOnCallbackQueryReceived(object sender,
            CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            if (callbackQuery == null) return;
            Conn.Open();
            var idUser = Funcs.GetIdUserFromUsername(callbackQuery.From.Username, Conn);
            Conn.Close();
            switch (callbackQuery.Data)
            {
                case "Create storage":
                    context.SavePrevState();
                    context.ChangeState(new CCreateStorageState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Enter new storage name: ");
                    break;
                case "Delete storage":
                    context.SavePrevState();
                    context.ChangeState(new CDeleteStorageState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Choose storage to delete",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "Rename storage":
                    context.SavePrevState();
                    context.ChangeState(new CRenameStorageState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Choose storage to rename",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "Show storage's":
                    context.SavePrevState();
                    context.ChangeState(new CShowStorageState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Choose storage:",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "Create folder":
                    context.SavePrevState();
                    context.ChangeState(new CCreateState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Enter new folder name: ");
                    break;

                case "Delete folder":
                    context.SavePrevState();
                    context.ChangeState(new CDeleteState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Choose to delete folder",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "Rename folder":
                    context.SavePrevState();
                    context.ChangeState(new CRenameState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Choose to rename folder",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "Show folders":
                    context.SavePrevState();
                    context.ChangeState(new CShowState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Choose folder:",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "Add data":
                    context.SavePrevState();
                    context.ChangeState(new CAddDataState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Add some data");
                    break;
                case "Get data":
                    context.SavePrevState();
                    context.ChangeState(new CGetDataState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Choose file",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "<- Back": 
                    context.ChangeOnPrevState();
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Choose action:",
                        replyMarkup: context.GetInlineKeyboardFromContext(idUser));
                    break;
                case "Share storage":
                    context.SavePrevState();
                    context.ChangeState(new CShareState());
                    context.ActionQueryContext(Bot, callbackQuery);
                    context.ChangeState(new CMainMenuState());

                    break;
                case "Get shared storage":
                    context.SavePrevState();
                    context.ChangeState(new CGetSharedState());
                    await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Enter share key: ");
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
