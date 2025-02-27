using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ConsoleBot
{
    public class Program
    {
        //متغیر `botClient` از نوع `TelegramBotClient` تعریف شده
        //TelegramBotClient یک کلاس => یک کلاینت برای ارسال درخواست ها و دریافت پاسخ ها به سرور تلگرام
        static TelegramBotClient botClient;
        public static decimal priBtc=0, priEth=0, priXrp=0;
        //یک شی از کلاس `HttpClient` تعریف شده و در متغیر `client` ذخیره شده است
        //HttpClient یک کلاس =>HttpClient برای ارسال درخواست به وب‌سرویس CoinMarketCap برای دریافت اطلاعات
        static readonly HttpClient client = new HttpClient();
        //apikey قیمت رمز CoinMarketCap
        static readonly string apiKey = "e8bc9488-475e-4b8b-8722-ad251c0856b5";
        //async => یک تابع را به صورت همزمان پیاده‌سازی می کند بدون این که بلافاصله به منبع فراخوانی کننده بازگردد
        static async Task Main() 
        {
            //متغیری که در بالا از نوع TelegramBotClient ایجاد شد تا توکن برنامه داخل باشد
            botClient = new TelegramBotClient("5970260081:AAEWt_0-53NFG2EpqilEAULwhwXyQcEKK-Q");
            //زمانی که ربات یک پیام دریافت می شود botClient.OnMessage متوجه می شود و تابع Bot_OnMessage فراخانی می شود
            botClient.OnMessage += Bot_OnMessage;
            //با فراخنی این تابع زمانی که ربات متصل می شود StartReceiving()ربات شروع به دریافت پیام‌های کاربران می‌کند.
            botClient.StartReceiving();
            //یک ارایه استرینگی درست می کند که داخل ان ارز های که می خواهیم قیمت ان را باز یابی کنیم مشخص شده
            string[] coins = { "BTC", "ETH", "XRP" };
            //حلقه ای برای دریافت قیمت هر ارز از api
            foreach (string coin in coins)
            {
                // با استفاده از URL API و کلید API یک درخواست HTTP ایجاد کنید
                string url = "https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol="+coin+"&convert=USD";
                //HttpRequestMessage` یک شیء برای ارسال درخواست‌های HTTP استفاده می‌شود. با استفاده از این کلاس می‌توانید درخواست‌های GET، POST، PUT، DELETE و ... را ایجاد کرده 
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-CMC_PRO_API_KEY", apiKey);
                //در خواست را Http ارسال می کند و جواب را در فایل response می ریزد
                var response = await client.SendAsync(request);
                //جواب را خوانده و با کمک کتابخانه ی Newtonsoft.Json به فابل جیسون تبدیل می کند
                var json = await response.Content.ReadAsStringAsync();
                // فایل جیسون را با استفاده از JObject.Parse به ابجکت تبدیل می کند و در داخل data داخل اسم رمز ارز(coin) داخلquote داخل USD را داخل data میریزیم
                var data = JObject.Parse(json)["data"][coin]["quote"]["USD"];
                // متغیر price از نوع decimal تعریف شده و مقدار price از فایل جیسون data استخراج می شود
                var price = (decimal)data["price"];
                //قیمت که از api دریافت شد به متغیر مربوط به خودش ریخته می شود
                switch (coin)
                {
                    case "BTC":
                        priBtc = price;
                        break;
                    case "ETH":
                        priEth = price;
                        break;
                    case "XRP":
                        priXrp = price;
                        break;
                }
            }
            
            Console.Write("ربات شروع به کار کرد. برای خروج از برنامه، دکمه‌ی Enter را بفشارید.");

            Console.ReadLine();
            //این تابع ربات تلگرامی را متوقف می کند
            botClient.StopReceiving();
        }
        //تابع دریافت و ارسال پیام
        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            //چک می کند پیام دریافتی خالی هست یا نه
            if (e.Message.Text != null)
            {
                //ای دی کار بر را در chatId زخیره می کند
                long chatId = e.Message.Chat.Id;
                string messageText = "";

                switch (e.Message.Text.ToLower())
                {
                    case "/start":
                        messageText = "سلام، به ربات من خوش آمدید!";
                        break;
                    case "بیت کوین":
                        messageText ="قیمت بیت کوین در حال حاضر "+priBtc.ToString()+"$ می باشد";
                        break;
                    case "اتریوم":
                        messageText ="قیمت اتریوم در حال حاضر "+priEth.ToString()+"$ می باشد";
                        break;
                    case "ریپل":
                        messageText ="قیمت ریپل در حال حاضر "+priXrp.ToString()+"$ می باشد";
                        break;
                    default:
                        messageText = "ببخشید متوجه نمی شوم";
                        break;
                }
                //استرینگ messageText رو به ای دیchatId می فرستد
                await botClient.SendTextMessageAsync(chatId, messageText);
            }
        }
    }
}