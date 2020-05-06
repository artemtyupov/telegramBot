using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CRenameState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;

        public int getID() { return 3; }

        private static void ChangeInlineKeyboard(int idUser)
        {
            var buttonItem = Funcs.GetListFolders(idUser, -1, Program.Conn).ToArray();
            InlineKeyboard = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
        }
        public void ActionMsg(TelegramBotClient Bot, Message message)
        {
            Program.Conn.Open();
            SQLLiteDB.MysqlDeleteOrInsert($"UPDATE Folders SET Name = \"{message.Text}\" WHERE Name = \"{Program._selectedButton}\"", Program.Conn);
            Program.Conn.Close();
        }

        public async void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            Program._selectedButton = callbackQuery.Data;
            try
            { 
                await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                await Bot.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "Choose new name:");
            }
            catch { }
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