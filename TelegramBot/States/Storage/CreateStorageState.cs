using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using MySql.Data.MySqlClient;

namespace TelegramBot
{
    public class CCreateStorageState : IState
    {

        public int getID() { return 7; }

        private static readonly InlineKeyboardMarkup InlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] // first row
            {
                InlineKeyboardButton.WithCallbackData("+"),
                InlineKeyboardButton.WithCallbackData("-"),
            },
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("Переименовать хранилище"),
                InlineKeyboardButton.WithCallbackData("Показать хранилища"),
            },
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("Получение приватного хранилища"),
            }
        });

        public async void ActionMsg(TelegramBotClient Bot, Message message)
        {
            Program.Conn.Open();
            var shareKey = Funcs.GetNewShareKey(Program.Conn);
            var idUser = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM User WHERE name = \"{message.Chat.Username}\"", Program.Conn));
            SQLLiteDB.SQLiteDeleteOrInsert($"INSERT INTO Storage (Name, idUser, idShared) VALUES(\"{message.Text}\", {idUser}, \"{shareKey}\");", Program.Conn);
            Program.Conn.Close();
            await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "Хранилище создано. \n" +
                "Выберите действие:",
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