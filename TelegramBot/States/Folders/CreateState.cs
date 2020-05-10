using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CCreateState : IState
    {
        private static readonly InlineKeyboardMarkup InlineKeyboard = Program.InlKey;

        public int getID() { return 15; }

        public async void ActionMsg(TelegramBotClient Bot, Message message)
        {   
            Program.Conn.Open();
            var idStorage = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM storage WHERE Name = \"{Program._selectedStorage}\"", Program.Conn));
            SQLLiteDB.SQLiteDeleteOrInsert($"INSERT INTO Folders (idStorage, idFolder, Name) VALUES({idStorage}, {-1}, \"{message.Text}\");", Program.Conn);
            Program.Conn.Close();
            await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "Folder is created. \n" +
                "Choose action:",
                replyMarkup: Program.InlKey);
        }

        public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery){}
        
        public IState ChangeOnPrevState()
        {
            return new CMainMenuState();
        }
        
        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            return InlineKeyboard;
        }
    }
}