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
            var idFolder = Convert.ToInt32(SQLLiteDB.MysqlSelect($"SELECT id FROM Folders WHERE name = \"{Program._selectedButton}\"", Program.Conn));
            Program.Conn.Close();
            System.Collections.Generic.List<string> buttonItem = Funcs.GetListFolders(idUser, idFolder, Program.Conn);
            buttonItem.Insert(0, "Add data");
            buttonItem.Insert(0, "Get data");
            buttonItem.Insert(0, "Add folder");
            InlineKeyboard = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem.ToArray()));
            //InlineKeyboard = new InlineKeyboardMarkup(new[]
            //{
            //    new[] // first row
            //    {
            //        InlineKeyboardButton.WithCallbackData("<- Back"),
            //        InlineKeyboardButton.WithCallbackData("Add data"),
            //        InlineKeyboardButton.WithCallbackData("Get data"),
            //    },
            //     new[] // second row
            //    {
            //        InlineKeyboardButton.WithCallbackData("Add folder"),

            //    },
            //});
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
                    "Choose action:",
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