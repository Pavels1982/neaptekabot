using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace NeAptekaBot
{
    class Program
    {
        private static ITelegramBotClient client;
        static void Main(string[] args)
        {
            client = new TelegramBotClient("1506064902:AAH1x0iPMJsZIrb1elwSDzgCyejh2IXvFZo") { Timeout = TimeSpan.FromSeconds(10) };
            var me = client.GetMeAsync().Result;
            Console.WriteLine(me.FirstName + " id: " + me.Id ) ;
            client.OnMessage += Client_OnMessage;
            client.StartReceiving();
            while (true) Thread.Sleep(10000);
            //Console.ReadKey();
        }

        private static async void Client_OnMessage(object sender, MessageEventArgs e)
        {
            var text = e?.Message?.Text;
            if (text == null) return;
            Console.WriteLine($"recived text '{text}' in chat '{e.Message.Chat.Id}'");

            await client.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: $"Привет '{text}'"
                ).ConfigureAwait(false);

        }
    }
}
