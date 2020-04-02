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

        public int getID() { return 1; }

        public async void ActionMsg(TelegramBotClient Bot, Message message)
        {
            if (message.Type != MessageType.Text)
            {
                Program.Conn.Open();
                var idFolder = Convert.ToInt32(SQLLiteDB.MysqlSelect($"SELECT id FROM Folders WHERE Name = \"{Program._selectedButton}\"", Program.Conn));
                
                string file_id_api = "";
                string filename = "";
                if (message.Type == MessageType.Document)
                {
                    file_id_api = message.Document.FileId;
                    filename = message.Document.FileName;
                }
                else if (message.Type == MessageType.Photo)
                {
                    if (message.Caption != null)
                    {
                        file_id_api = message.Photo[message.Photo.Length - 1].FileId;
                        filename = message.Caption;
                        if (!filename.Contains(".jpg") || !filename.Contains(".png") || !filename.Contains(".jpeg") || !filename.Contains(".bmp"))
                            filename += ".jpg";
                    }
                    else
                    {
                        Program.Conn.Close();
                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Your file didnt added.\n" +
                            "Please enter the caption, when you add photo(better with extension)\n" +
                            "Choose action:",
                            replyMarkup: new CShowState().GetInlineKeyboardFromState(message.From.Id));
                    }
                }
                
                SQLLiteDB.MysqlDeleteOrInsert($"INSERT INTO Files (idFolder, idMessage, Name, idChat, FSCreatedTime, FSAccessTime, FSWriteTime, idFileAPI) VALUES ({idFolder}, {message.MessageId}, \"{filename}\", {message.Chat.Id}, \"{DateTime.Now}\", \"{DateTime.Now}\", \"{DateTime.Now}\", \"{file_id_api}\")", Program.Conn);
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