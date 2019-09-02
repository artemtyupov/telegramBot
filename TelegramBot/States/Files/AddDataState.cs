using System;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CAddDataState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;
        public async void ActionMsg(TelegramBotClient Bot, Message message)
        {
            if (message.Type != MessageType.Text)
            {
                Program.Conn.Open();
                var idFolder = Convert.ToInt32(Database.MysqlSelect($"SELECT id FROM Folders WHERE Name = \"{Program._selectedButton}\"", Program.Conn));
                //TODO Caption != name, скорее всего нужно делать switch по возможным типам файла и оттуда высовывать name вместо caption
                Database.MysqlDeleteOrInsert($"INSERT INTO Files (idFolder, idMessage, Name, idChat) VALUES ({idFolder}, {message.MessageId}, \"{message.Caption}\", {message.Chat.Id})", Program.Conn);    
                Program.Conn.Close();
                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Your file added.\n" +
                    "Choose action:",
                    replyMarkup: new CShowState().GetInlineKeyboardFromState(message.From.Id));
            }
        }

        public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery){}
        
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