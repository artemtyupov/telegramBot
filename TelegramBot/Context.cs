using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CContext
    {
        private IState st;
        private IState prevSt;
        
        public CContext(IState state)
        {
            st = state;
        }

        public void SavePrevState()
        {
            prevSt = st;
        }
        public void ChangeOnPrevState()
        {
            st = st.ChangeOnPrevState();
        }
        
        public void ChangeState(IState state)
        {
            st = state;
        }

        public void ActionMsgContext(TelegramBotClient Bot, Message msg)
        {
            st.ActionMsg(Bot, msg);
        }
        
        public void ActionQueryContext(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            st.ActionQuery(Bot, callbackQuery);
        }

        public InlineKeyboardMarkup GetInlineKeyboardFromContext(int idUser)
        {
            return st.GetInlineKeyboardFromState(idUser);
        }

        public IState GetTypeState()
        {
            return st;
        }
    }
}