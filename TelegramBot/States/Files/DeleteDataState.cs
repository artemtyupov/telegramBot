using System;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CDeleteDataState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;

        public int getID() { return 17; }

        public async void ActionMsg(TelegramBotClient Bot, Message message) { }

        async public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            Program.Conn.Open();
            var idFolder = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM Files WHERE Name = \"{Program._selectedButton}\"", Program.Conn));
            SQLLiteDB.SQLiteDeleteOrInsert($"DELETE FROM Files WHERE Name = \"{callbackQuery.Data}\" and idFolder = {idFolder} ", Program.Conn);
            Program.Conn.Close();
            try
            {
                await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                await Bot.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "Выберите действие:",
                    replyMarkup: Program.InlKey);
            }
            catch { }
        }

        public IState ChangeOnPrevState()
        {
            return new CChoosedToShowState();
        }

        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            return InlineKeyboard;
        }
    }
}