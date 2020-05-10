using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CGetDataState : IState
    {
        
        private static InlineKeyboardMarkup InlineKeyboard;

        public int getID() { return 13; }

        private static void ChangeInlineKeyboard(int idUser)
        {
            Program.Conn.Open();
            var idStorage = Funcs.GetIdStorageFromIdUser(idUser, Program.Conn);
            var idFolder = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT id FROM Folders WHERE idStorage = {idStorage} and Name = \"{Program._selectedButton}\"", Program.Conn));
            Program.Conn.Close();
            var namesFiles = Funcs.GetListFiles(idUser, idFolder, Program.Conn);
            InlineKeyboard = new InlineKeyboardMarkup(Funcs.GetInlineKeyboard(namesFiles.ToArray()));
        }
        
        public void ActionMsg(TelegramBotClient Bot, Message message){}

        public async void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            Program.Conn.Open();
            var idStorage = Funcs.GetIdStorageFromIdUser(Funcs.GetIdUserFromUsername(callbackQuery.From.Username, Program.Conn), Program.Conn);
            var idFolder = Funcs.GetIdFolderFromIdStorage(idStorage, Program.Conn);
            var idMessage = Convert.ToInt32(SQLLiteDB.SQLiteSelect($"SELECT idMessage FROM Files WHERE Name = \"{callbackQuery.Data}\" and idFolder = {idFolder}", Program.Conn));
            var idChat = Convert.ToInt64(SQLLiteDB.SQLiteSelect($"SELECT idChat FROM Files WHERE idMessage = {idMessage} and idFolder = {idFolder}", Program.Conn));
            var test = new ChatId(idChat);
            Program.Conn.Close();
            try
            { 
                await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

                if (idChat == callbackQuery.Message.Chat.Id)
                {
                    await Bot.SendTextMessageAsync(
                        test,
                        "Your file",
                        replyToMessageId: idMessage,
                        replyMarkup: InlineKeyboard);
                }
                else
                {
                    await Bot.ForwardMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        test,
                        idMessage);
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "Your file",
                        replyMarkup: InlineKeyboard);

                }
            }
            catch { }
        }
        
        public IState ChangeOnPrevState()
        {
            return new CChoosedToShowState();
        }
        
        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            ChangeInlineKeyboard(idUser);
            return InlineKeyboard;
        }
    }
}