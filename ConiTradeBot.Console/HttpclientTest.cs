using ConiTradeBot.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConiTradeBot.Run
{
    class HttpclientTest
    {
        private static readonly HttpClient client1 = new HttpClient();
        private static readonly HttpClient client2 = new HttpClient();

        public string HttpGet(HttpClient client, string url)
        {
            return client.GetStringAsync(url).Result;
        }

        public void Test()
        {
            new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var rspns = HttpGet(client1, "https://www.cnblogs.com/skynet/archive/2010/05/18/1738301.html");
                    Console.Write(i + ".");
                }
            }).Start();

            //new Thread(() =>
            //{
            for (int i = 0; i < 100; i++)
                {
                    var rspns = HttpGet(client2, "https://www.cnblogs.com/skynet/archive/2010/05/18/1738301.html");
                    Console.Write(i + ",");
                }
            //}).Start();



        }
    }
}
