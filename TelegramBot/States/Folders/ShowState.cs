using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using System;

namespace TelegramBot
{
    public class CShowState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;

        public int getID() { return 5; }

        private static void ChangeInlineKeyboard(int idUser)
        {
            var buttonItem = Funcs.GetListFolders(idUser, -1, Program.Conn).ToArray();
            InlineKeyboard = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
        }

        public void ActionMsg(TelegramBotClient Bot, Message message){}

        public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery){}

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
    
    public class CChoosedToShowState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;

        public int getID() { return 6; }

        private static void ChangeInlineKeyboard(int idUser)
        {
            Program.Conn.Open();
            var idFolder = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM Folders WHERE name = \"{Program._selectedButton}\"", Program.Conn));
            Program.Conn.Close();
            System.Collections.Generic.List<string> buttonItem = Funcs.GetListFolders(idUser, idFolder, Program.Conn);
            buttonItem.Insert(0, "+");
            buttonItem.Insert(1, "-");
            buttonItem.Insert(2, "Получение файла");
            buttonItem.Insert(3, "Создание папки");
            InlineKeyboard = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem.ToArray()));
        }
        
        public void ActionMsg(TelegramBotClient Bot, Message message){}

        public async void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            Program._selectedButton = callbackQuery.Data;
            Program.Conn.Open();
            int idUser = Funcs.GetIdUserFromUsername(callbackQuery.From.Username, Program.Conn);
            Program.Conn.Close();
            ChangeInlineKeyboard(idUser);
            try
            { 
                await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                await Bot.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "Выберите действие:",
                    replyMarkup: InlineKeyboard);
            }
            catch { }
        }
        
        public IState ChangeOnPrevState()
        {
            return new CShowState();
        }
        
        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            ChangeInlineKeyboard(idUser);
            return InlineKeyboard;
        }
    }
}