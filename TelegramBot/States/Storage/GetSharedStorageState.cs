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
                InlineKeyboardButton.WithCallbackData("Переименовать хранилище"),
                InlineKeyboardButton.WithCallbackData("Показать хранилища"),
            },
            new[] // second row
            {
                InlineKeyboardButton.WithCallbackData("Получение приватного хранилища"),
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
                    "Хранилище добавлено. \n" +
                    "Выберите действие:",
                    replyMarkup: InlineKeyboard);
            }
            else
            {
                //error
                await Bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                await Bot.SendTextMessageAsync(
                message.Chat.Id,
                "Неизвестный ключ. \n" +
                "Выберите действие:",
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
                    $"Введите приватный ключ:");
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