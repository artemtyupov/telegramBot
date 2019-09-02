using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CMainMenuState : IState
    {
        private static readonly InlineKeyboardMarkup InlineKeyboard = Program.InlKey;
        
        public void ActionMsg(TelegramBotClient Bot, Message message){}

        public void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery){}

        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            return InlineKeyboard;
        }
        
        public IState ChangeOnPrevState()
        {
            return new CShowStorageState();
        }
    }
}