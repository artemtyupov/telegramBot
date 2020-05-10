using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CContext
    {
        private IState state;
        private IState prevSt;
        
        public CContext(IState state)
        {
            this.state = state;
        }

        public void SavePrevState()
        {
            prevSt = state;
        }
        public void ChangeOnPrevState()
        {
            state = state.ChangeOnPrevState();
        }
        
        public void ChangeState(IState state)
        {
            this.state = state;
        }

        public void ActionMsgContext(TelegramBotClient Bot, Message msg)
        {
            state.ActionMsg(Bot, msg);
        }
        
        public void ActionQueryContext(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            state.ActionQuery(Bot, callbackQuery);
        }

        public InlineKeyboardMarkup GetInlineKeyboardFromContext(int idUser)
        {
            return state.GetInlineKeyboardFromState(idUser);
        }

        public IState GetTypeState()
        {
            return state;
        }

        public int getID() { return state.getID(); }
    }
}