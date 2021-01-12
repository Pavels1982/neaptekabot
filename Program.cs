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
        static HtmlAgilityPack.HtmlDocument html;
        static JObject json;
        static List<Offer> nodes = new List<Offer>();
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
            html = web.Load(url3.AbsoluteUri);

            nodes.Clear();

            HtmlNodeCollection links = html.DocumentNode.SelectNodes(".//div[@class='offer ']");
              foreach (HtmlNode node in links)
            {
                var analoglink = node.ChildNodes[1].SelectSingleNode(".//a[@class='offer__analogues-link']")?.GetAttributeValue("href", ""); ;
                if (analoglink == null) break;

                var offer = new Offer();
                try
                {
                    offer.Analog_Link = analoglink;
                    offer.Image = node.ChildNodes[1].ChildNodes[1].ChildNodes[1].SelectSingleNode(".//img").GetAttributeValue("src", "");
                    offer.Title = node.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[1].InnerText.Trim();
                    offer.Description = node.ChildNodes[1].ChildNodes[3].ChildNodes[11].InnerText.Trim();
                    offer.Manufacturer = node.ChildNodes[1].ChildNodes[3].ChildNodes[13].ChildNodes[3].InnerHtml.Trim();
                }
                catch 
                {
                    offer.Image = "";
                }
                    try
                {
                    offer.Price = "Цена от " + node.ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[6].InnerText.Trim() + " руб";
                }
                catch
                {
                    offer.Price = "Цена неизвестна.";
                }
                
                
               nodes.Add(offer);
            }
            foreach (Offer offer in nodes)
            {
                if (offer.Image != "")
                {
                    _ = await client.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        disableWebPagePreview: false,
                        text: $"{offer.Title}\n{offer.Description}\n Производитель:{offer.Manufacturer}\n{offer.Price}[.]({ offer.Image})",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown

                       );
                    // await client.SendTextMessageAsync(e.Message.Chat.Id, offer.Image);
                }


            }
           // text: $"<b>{offer.Title}</b>\n@bold []({offer.Image})\n<b>{offer.Description}</b>\n<b>Производитель:{offer.Manufacturer}</b>\n<b>Цена от:{offer.Price}</b>",
                //        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html


      //      <b> bold </b>, <strong  bold </strong>
      //<i> italic </i >, <em> italic </em >
      //      < a href = "http://www.example.com/" > inline URL </ a >
      //           < a href = "tg://user?id=123456789" > inline mention of a user</ a >
      //                <code> inline fixed-width code </ code >
      //                   <pre> pre - formatted fixed-width code block</pre>

            //await client.SendTextMessageAsync(<img src=\"{offer.Image}\"/>
            //    chatId: e.Message.Chat,
            //    text: $"{e.Message.Chat.FirstName}, привет!:  '{text}'"
            //    ).ConfigureAwait(false);

        }
    }
}
