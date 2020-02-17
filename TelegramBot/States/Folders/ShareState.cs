using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CShareState : IState
    {
        private static readonly InlineKeyboardMarkup InlineKeyboard = Program.InlKey;

        public void ActionMsg(TelegramBotClient Bot, Message message){}

        public async void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            var shareKey = Funcs.GetShareKey(callbackQuery.From.Username, Program._selectedStorage, Program.Conn);
            try
            { 
                await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                await Bot.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    $"Your share key: {shareKey}",
                replyMarkup: Program.InlKey);
            }
            catch { }
        }
        
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