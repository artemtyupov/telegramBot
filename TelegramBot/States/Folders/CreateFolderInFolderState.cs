using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CCreateFolderInFolderState : IState
    {
        private static readonly InlineKeyboardMarkup InlineKeyboard = Program.InlKey;

        public int getID() { return 16; }

        public async void ActionMsg(TelegramBotClient Bot, Message message)
        {
            Program.Conn.Open();
            var idStorage = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM Storage WHERE Name = \"{Program._selectedStorage}\"", Program.Conn));
            var idFolder = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM Folders WHERE Name = \"{Program._selectedButton}\"", Program.Conn));
            SQLLiteDB.SQLiteDeleteOrInsert($"INSERT INTO Folders (idStorage, idFolder, Name) VALUES({idStorage}, {idFolder}, \"{message.Text}\");", Program.Conn);
            Program.Conn.Close();
            await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "Папка создана. \n" +
                "Выберите действие:",
                replyMarkup: Program.InlKey);
        }

        public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery) { }

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