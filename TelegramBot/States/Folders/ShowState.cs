using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CShowState : IState
    {
        private static InlineKeyboardMarkup InlineKeyboard;

        private static void ChangeInlineKeyboard(int idUser)
        {
            var buttonItem = Funcs.GetListFolders(idUser, Program.Conn).ToArray();
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

        private static void ChangeInlineKeyboard()
        {
            InlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] // first row
                {
                    InlineKeyboardButton.WithCallbackData("<- Back"),
                    InlineKeyboardButton.WithCallbackData("Add data"),
                    InlineKeyboardButton.WithCallbackData("Get data"),
                    
                }
            });
        }
        
        public void ActionMsg(TelegramBotClient Bot, Message message){}

        public async void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            ChangeInlineKeyboard();
            Program._selectedButton = callbackQuery.Data;
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
            ChangeInlineKeyboard();
            return InlineKeyboard;
        }
    }
}