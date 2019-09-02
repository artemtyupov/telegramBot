using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CMenuStorage : IState
    {
        private static readonly InlineKeyboardMarkup InlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] // first row
            {
                InlineKeyboardButton.WithCallbackData("Create storage"),
                InlineKeyboardButton.WithCallbackData("Delete storage"),
            },
            new[] // first row
            {
                InlineKeyboardButton.WithCallbackData("Rename storage"),
                InlineKeyboardButton.WithCallbackData("Show storage's"),
            },
            new[] // second row
            {
                InlineKeyboardButton.WithCallbackData("Get shared storage"),
            }
        });

        private static void ChangeInlineKeyboard(int idUser) {}

        public void ActionMsg(TelegramBotClient Bot, Message message) {}

        public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery){}

        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            ChangeInlineKeyboard(idUser);
            return InlineKeyboard;
        }
        
        public IState ChangeOnPrevState()
        {
            return new CMenuStorage();
        }
    }
}