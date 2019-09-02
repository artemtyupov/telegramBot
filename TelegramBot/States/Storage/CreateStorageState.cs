using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using MySql.Data.MySqlClient;

namespace TelegramBot
{
    public class CCreateStorageState : IState
    {
        private static readonly InlineKeyboardMarkup InlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] // first row
            {
                InlineKeyboardButton.WithCallbackData("Create storage"),
                InlineKeyboardButton.WithCallbackData("Delete storage"),
            },
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("Rename storage"),
                InlineKeyboardButton.WithCallbackData("Show storage's"),
            },
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("Get shared storage"),
            }
        });
        public async void ActionMsg(TelegramBotClient Bot, Message message)
        {
            Program.Conn.Open();
            var shareKey = Funcs.GetNewShareKey(Program.Conn);
            var idUser = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM User WHERE name = \"{message.Chat.Username}\"", Program.Conn));
            Database.MysqlDeleteOrInsert($"INSERT INTO Storage (Name, idUser, idShared) VALUES(\"{message.Text}\", {idUser}, \"{shareKey}\");", Program.Conn);
            Program.Conn.Close();
            await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "Storage is created. \n" +
                "Choose action:",
                replyMarkup: InlineKeyboard);
        }
        public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery){}
        
        public IState ChangeOnPrevState()
        {
            return new CMenuStorage();
        }
        
        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            return InlineKeyboard;
        }
    
    }
}