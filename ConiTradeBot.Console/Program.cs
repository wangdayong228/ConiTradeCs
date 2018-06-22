using ConiTradeBot.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConiTradeBot.Run
{
    class Program
    {
        static void Main(string[] args)
        {           

            var body = new Dictionary<string, string>
            {
                {"wPath", "STE/MUTS/HP34401A" },
                {"wModule", "LEO34401A,haha,gaga" },
                {"wAction", "Add" }
            };
            //var response = Service.Post("http://localhost:3001/app/module", body);
            //Console.WriteLine(response.Result);

            //var ticker = Service.GetTicker("btcusdt");
            //Console.WriteLine(ticker.Result);

            var balance = Service.post_balance(new Dictionary<string, string>()
            {
                { "apiid","ec7cec8400370acca39fbe902b481471" },
                { "secret","fb571b65836047e2aef9360b785e6bfd"},
                { "timestamp",utils.GetTimestamp()},
                { "account","exchange"}
            });

            //var balance = Service.post_balance(new Dictionary<string, string>()
            //{
            //    { "apiid","xxx" },
            //    { "secret","xxx"},
            //    { "timestamp","11223112231"},
            //    { "account","exchange"}
            //});
            Console.WriteLine(balance.Result);

            Console.Read();
        }
    }
}
