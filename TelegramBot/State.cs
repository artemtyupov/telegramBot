using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public interface IState
    {
        void ActionMsg(TelegramBotClient Bot, Message message);
        void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery);

        IState ChangeOnPrevState();

        InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser);
    }
}