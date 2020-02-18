using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class CGetSharedState : IState
    {
        private static readonly InlineKeyboardMarkup InlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] // first row
            {
                InlineKeyboardButton.WithCallbackData("+"),
                InlineKeyboardButton.WithCallbackData("-"),
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

        public int getID() { return 9; }

        public async void ActionMsg(TelegramBotClient Bot, Message message)
        {
            //check key
            if (Funcs.CheckShareKey(message.Text, Program.Conn))
            {
                //share
                await Bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                Funcs.ShareProcess(message, Program.Conn);
                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Storage added. \n" +
                    "Choose action:",
                    replyMarkup: InlineKeyboard);
            }
            else
            {
                //error
                await Bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "Key is unknown. \n" +
                "Choose action:",
                replyMarkup: InlineKeyboard);
            }
        }

        public async void ActionQuery(TelegramBotClient Bot, CallbackQuery callbackQuery)
        {
            try
            { 
                await Bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                await Bot.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    $"Enter share key:");
            }
            catch { }
        }
        
        public IState ChangeOnPrevState()
        {
            return new CMenuStorage();
        }
        public InlineKeyboardMarkup GetInlineKeyboardFromState(int idUser)
        {
            return InlineKeyboard;
        }
    }
}