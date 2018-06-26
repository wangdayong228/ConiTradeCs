using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConiTradeBot.API;
using Newtonsoft.Json.Linq;

namespace ConiTradeBot.Run
{
    public class TestCancelOrder
    {
        const string QUANTITY = "1400";
        const string TRADETYPE = "conieth";
        const int PRECISION = 6;
        const string PRICE = "";

        public void Test()
        {
            List<JObject> placeOrderRspns = new List<JObject>();
            for (int i = 0; i < 5; i++)
            {
                var rspns = JObject.Parse(Service.post_order_place(TRADETYPE, "buy-limit", "0.000635", QUANTITY).Result);                
                placeOrderRspns.Add(rspns);
                if (rspns["status"].ToString() == "error")
                {
                    Console.WriteLine("sell swtc error:"+ rspns["description"]);
                }
                Thread.Sleep(100);
                //break;
            }

            for (int i = 0; i < 5; i++)
            {
                var sellOrderId = placeOrderRspns[i]["orderid"].ToString();
                do
                {
                    var cancleRspns = JObject.Parse(Service.post_cancel(sellOrderId).Result);//cancel order
                    Console.WriteLine(string.Format("cancle order {0} {1}: {2}", sellOrderId, cancleRspns["status"], cancleRspns["description"]));
                    if (cancleRspns["status"].ToString() == "error")//response error, retry, may has done
                        break;

                    Thread.Sleep(500);
                    var orders = JObject.Parse(Service.post_open_orders(TRADETYPE).Result.ToString());
                    var content = orders["orders"].ToString();
                    if (string.IsNullOrEmpty(content))//orders is null means has canceled, and could start to reorder;
                    {
                        Console.WriteLine("no ");
                        break;
                    }

                    var items = orders["orders"]["result"].ToArray();
                    var match = items.FirstOrDefault(item => item["orderid"].ToString() == sellOrderId);
                    if (match == null)//no matched means has canceled too, could start to reorder;
                        break;
                    Thread.Sleep(100);
                } while (true);

            }


        }
    }
}
