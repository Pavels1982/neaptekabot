using System;
using System.IO;
using System.Net;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace NeAptekaBot
{
    public class Offer
    {
        public string Image = String.Empty;
        public string Title = String.Empty;
        public string Analog_Link = String.Empty;
        public string Manufacturer = String.Empty;
        public string Price = String.Empty;
        public string Description = String.Empty;
    }

    class Program
    {
        private static ITelegramBotClient client;
        static HttpWebResponse response;
        static string page;
        static JObject json;
        static HtmlAgilityPack.HtmlDocument html;
        static List<Offer> nodes = new List<Offer>();
        static HtmlNodeCollection links;
        static void Main(string[] args)
        {

    
            client = new TelegramBotClient("1503814220:AAF7ZZ1Q4h71JOE2cjCTkkeyTaOnKkfkSbw") { Timeout = TimeSpan.FromSeconds(10) };
            var me = client.GetMeAsync().Result;
            Console.WriteLine(me.FirstName + " id: " + me.Id ) ;
            client.OnMessage += Client_OnMessage;
            client.StartReceiving();
            while (true) Thread.Sleep(10000);
            //Console.ReadKey();
        }

        private static async void Client_OnMessage(object sender, MessageEventArgs e)
        {
            var typeMsg = e.Message.Type;
            Console.WriteLine(typeMsg);
            var text = e?.Message?.Text;
            if (text == null) return;
            Console.WriteLine($"recived text '{text}' in chat '{e.Message.Chat.Id}'");
            //https://монастырёв.рф/search?term=

            // HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.apteka.ru/Search/ByPhrase?pageSize=25&page=0&iPharmTownId=&withprice=false&withprofit=false&withpromovits=false&phrase=" + text + "&cityId=5e57803249af4c0001d64407");
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://монастырёв.рф/search?term=" + text);
            //response = (HttpWebResponse)request.GetResponse();

            //StreamReader sr = new StreamReader(response.GetResponseStream());
            //var page = sr.ReadToEnd();
            //sr.Close();

            Uri url3 = new Uri("https://монастырёв.рф/search?term=" + text);
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            web.AutoDetectEncoding = true;
            _ = await client.SendTextMessageAsync(e.Message.Chat, "Подождите, я ищу");
            html = web.Load(url3.AbsoluteUri);
            links = html.DocumentNode.SelectNodes(".//div[@class='offer ']");


            nodes.Clear();

            foreach (HtmlNode node in links)
            {
                var offer = new Offer();
                var analoglink = node.SelectSingleNode(".//a[@class='offer__analogues-link']")?.GetAttributeValue("href", ""); ;
                if (analoglink == null) break;
                offer.Title = node.SelectSingleNode(".//div[@class='offer__title link__text']")?.InnerText.Trim();
                offer.Image = node.SelectSingleNode(".//img")?.GetAttributeValue("src", "");
                offer.Analog_Link = analoglink;
                offer.Description = node.SelectSingleNode(".//div[@class='offer__description']")?.InnerText.Trim();
                offer.Manufacturer = node.SelectSingleNode(".//div[@class='offer__manufacturer']")?.InnerText.Trim();
                offer.Price = "Цена от " + node.SelectSingleNode(".//div[@class='offer__price-old']")?.InnerText.Trim();
                if (offer.Price == "")
                    offer.Price = "Цена неизвестна.";
              

                nodes.Add(offer);
            }
            foreach (Offer offer in nodes)
            {
                try
                {
                    if (offer.Image != "")
                {

                        List<InlineKeyboardButton> ButtonList = new List<InlineKeyboardButton>();

                        ButtonList.Add(new InlineKeyboardButton() { Text = "Аналоги", CallbackData = "https://монастырёв.рф" +offer.Analog_Link });

                        InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(ButtonList);

                        _ = await client.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        disableWebPagePreview: false,
                        text: $"{offer.Title}\n{offer.Description}\n Производитель:{offer.Manufacturer}\n{offer.Price}[.]({ offer.Image})",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        replyMarkup: inlineKeyboardMarkup

                       ).ConfigureAwait(false);
                    }

                }
                catch
                {
                    _ = await client.SendTextMessageAsync(e.Message.Chat, "Catch in offer");
                }

            }

            //      <b> bold </b>, <strong  bold </strong>
            //<i> italic </i >, <em> italic </em >
            //      < a href = "http://www.example.com/" > inline URL </ a >
            //           < a href = "tg://user?id=123456789" > inline mention of a user</ a >
            //                <code> inline fixed-width code </ code >
            //                   <pre> pre - formatted fixed-width code block</pre>

            _ = await client.SendTextMessageAsync(e.Message.Chat, "Поиск завершен");
        }
    }
}
