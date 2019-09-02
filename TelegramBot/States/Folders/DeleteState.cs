using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CDeleteState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;

        private static void ChangeInlineKeyboard(int idUser)
        {
            var buttonItem = Funcs.GetListFolders(idUser, Program.Conn).ToArray();
            InlineKeyboard = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
        }
        
        public void ActionMsg(TelegramBotClient Bot, Message message){}

        public async void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            Program.Conn.Open();
            Database.MysqlDeleteOrInsert($"DELETE FROM Folders WHERE Name = \"{callbackQuery.Data}\" ", Program.Conn);
            Program.Conn.Close();
            await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                "Choose action:",
                replyMarkup: Program.InlKey);
        }
        
        public IState ChangeOnPrevState()
        {
            return new CMainMenuState();
        }
        
        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            ChangeInlineKeyboard(idUser);
            return InlineKeyboard;
        }
    }
}