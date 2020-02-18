using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CRenameStorageState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;

        public int getID() { return 11; }

        private static void ChangeInlineKeyboard(int idUser)
        {
            var buttonItem = Funcs.GetListStorages(idUser, Program.Conn).ToArray();
            InlineKeyboard = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(buttonItem));
        }

        public void ActionMsg(TelegramBotClient Bot, Message message)
        {
            Program.Conn.Open();
            SQLLiteDB.MysqlDeleteOrInsert($"UPDATE Storage SET Name = \"{message.Text}\" WHERE Name = \"{Program._selectedButton}\"", Program.Conn);
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
            return new CMenuStorage();
        }
        
        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            ChangeInlineKeyboard(idUser);
            return InlineKeyboard;
        }
    }
}