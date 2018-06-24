using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConiTradeBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace ConiTradeBot.Run
{
    public class Bot
    {

        const string QUANTITY = "0.01";
        const string TRADETYPE = "ethbtc";
        const int PRECISION = 6;

        public void Dig()
        {
            //Todo:
            //获取深度，取买单最高价，最后一位加1挂单买N USDT的ETH，如1ETH=500USDT，挂单ETHUSD：buy-limit 500.01USDT，数量1
            //同时挂单等价的卖单： sell-limit 500.01USDT, 数量1
            //查询是否成交，如有没有成交的立即调整价格让其成交
            //等成交后再挂下一单
            string buyPrice, sellPrice;
            int waitBuyTime = 0, waitSellTime = 0;
            CalcMyTradePrice(out buyPrice, out sellPrice);

            var buyOrder = ReTryPlaceOrderUntilSucess(TRADETYPE, "buy-limit", buyPrice);// ReTry(() => Service.post_order_place("ethusdt", "buy-limit", myPlanPrice, quantity)).Result;
            Log(string.Format("place {0} order:{1} {2}", "buy", buyPrice, buyOrder["buyOrder"]));
            if (!PlaceOrderSucess(buyOrder))
                return;
            var sellOrder = ReTryPlaceOrderUntilSucess(TRADETYPE, "sell-limit", sellPrice);// ReTry(() => Service.post_order_place("ethusdt", "sell-limit", myPlanPrice, quantity)).Result;
            Log(string.Format("place {0} order:{1} {2}", "sell", sellPrice, sellOrder["status"]));
            if (!PlaceOrderSucess(sellOrder))
                throw new Exception("Re placed 10 times sell order failed, please check what happen.");
            Thread.Sleep(2200);

            bool isBuyDone = false;
            bool isSellDone = false;
            while (true)
            {
                if (!isBuyDone)
                {
                    isBuyDone = WaitOrderDone(buyPrice, ref buyOrder, "buy", waitBuyTime);
                    waitBuyTime++;
                }
                if (!isSellDone)
                {
                    isSellDone = WaitOrderDone(sellPrice, ref sellOrder, "sell", waitSellTime);
                    waitSellTime++;
                }
                if (isBuyDone && isSellDone)
                    break;
            }
        }

        public JObject ReTryPlaceOrderUntilSucess(string symbol, string type, string price, string quantity = QUANTITY)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var rspns = JObject.Parse(Service.post_order_place(symbol, type, price, quantity).Result);
                    if (rspns["status"].ToString() == "ok")
                        return rspns;

                    var descrip = rspns["description"].ToString();
                    if (descrip == "System Busy.")
                    {
                        Thread.Sleep(1000);
                    }
                    else
                        throw new Exception(descrip);
                }
                catch (Exception e)
                {
                    Log(i + " Retry place order error" + e);
                }
            }
            throw new Exception("System Busy.");
        }

        public bool PlaceOrderSucess(JObject order)
        {
            return order["status"].ToString() == "ok";
        }

        public bool WaitOrderDone(string planPrice, ref JObject order, string orderType, int waitTime)
        {
            //Log("wait " + orderType + " done");
            var sellOrderId = order["orderid"].ToString();
            var rspns = JObject.Parse(Service.post_info(sellOrderId).Result);
            Thread.Sleep(100);
            if (rspns["status"].ToString() == "error")
            {
                Console.WriteLine(orderType+ "order error: " + rspns["description"]);
                return false;
            }

            var orderRslt = rspns["order"];
            var orderStatus = orderRslt["orderstatus"].ToString();
            var isOrderFilled = orderStatus == "filled";
            var limitRatio = orderType == "buy" ? 1.0003 : 0.9997;//if ratio is between limitRatio, that menas has profit space.

            if (isOrderFilled)
            {
                var averageprice = orderRslt["averagePrice"];
                var fees = orderRslt["fees"];
                Log(string.Format("{0} {1} {2}, price:{3},fees:{4}, status:{5}", orderType, QUANTITY, TRADETYPE, averageprice, fees, orderStatus));
                return true;
            }
            try
            {

                var filledQuantity = orderStatus == "unfilled" ? "0" : orderRslt["filledquantity"].ToString();
                var unfilledQuantity = double.Parse(orderRslt["orderquantity"].ToString()) - double.Parse(filledQuantity);
                string buyPrice, sellPrice;
                CalcFastTradePrice(out buyPrice, out sellPrice);
                Thread.Sleep(100);
                var nowPrice = orderType == "buy" ? buyPrice : sellPrice;
                Log("now trade price:" + nowPrice);
                if (nowPrice == planPrice)
                    return false;
                //cancle and reorder
                var nowRatio = double.Parse(nowPrice) / double.Parse(planPrice);
                var hasProfitSpace = orderType == "buy" ? (nowRatio <= limitRatio) : (nowRatio >= limitRatio);
                var waitTimeLimited = waitTime > 10;
                //or wait time more than 10 times

                if (hasProfitSpace || waitTimeLimited)
                {
                    do
                    {
                        var cancleRspns = JObject.Parse(Service.post_cancel(sellOrderId).Result);//cancel order
                        Log(string.Format("cancle {0} order {1} {2}: {3}", orderType, sellOrderId, cancleRspns["status"], cancleRspns["description"]));
                        if (cancleRspns["status"].ToString() == "error")//response error, retry, may has done
                            return false;

                        Thread.Sleep(500);
                        var orders = JObject.Parse(Service.post_open_orders("ethusdt").Result.ToString());
                        var content = orders["orders"].ToString();
                        if (string.IsNullOrEmpty(content))//orders is null means has canceled, and could start to reorder;
                            break;

                        var items = orders["orders"]["result"].ToArray();
                        var match = items.FirstOrDefault(i => i["orderid"].ToString() == sellOrderId);
                        if (match == null)//no matched means has canceled too, could start to reorder;
                            break;
                        Thread.Sleep(100);
                    } while (true);

                    order = ReTryPlaceOrderUntilSucess(TRADETYPE, orderType + "-limit", nowPrice, unfilledQuantity.ToString());//reorder for unfilled.
                    Thread.Sleep(1000);
                    Log(string.Format("reoder {0}:{1} {2}", orderType, nowPrice, order["status"]));
                }
            }
            catch (Exception e)
            {
                Log("check order error, retry:" + e.Message);
            }
            return isOrderFilled;
        }

        public void CalcFastTradePrice(out string buyPrice, out string sellPrice)
        {
            var orderbooks = Service.get_orderbook(TRADETYPE, 2).Result;
            var jObj = JObject.Parse(orderbooks);
            var bidPrice = double.Parse(jObj["orderbook"]["bids"][0]["price"].ToString());
            var bidQuantity = double.Parse(jObj["orderbook"]["bids"][0]["quantity"].ToString());

            var askPrice = double.Parse(jObj["orderbook"]["asks"][0]["price"].ToString());
            var askQuantity = double.Parse(jObj["orderbook"]["asks"][0]["quantity"].ToString());
            buyPrice = askPrice.ToString();
            sellPrice = bidPrice.ToString();
        }

        private void CalcMyTradePrice(out string buyPrice, out string sellPrice)
        {
            var orderbooks = Service.get_orderbook(TRADETYPE, 2).Result;
            var jObj = JObject.Parse(orderbooks);
            var bidPrice = double.Parse(jObj["orderbook"]["bids"][0]["price"].ToString());
            var bidQuantity = double.Parse(jObj["orderbook"]["bids"][0]["quantity"].ToString());

            var askPrice = double.Parse(jObj["orderbook"]["asks"][0]["price"].ToString());
            var askQuantity = double.Parse(jObj["orderbook"]["asks"][0]["quantity"].ToString());

            var tradePrice = (bidPrice + askPrice) / 2 + 0.4 * Math.Pow(10, (0 - PRECISION));
            tradePrice = Math.Round(tradePrice, PRECISION);

            if (tradePrice > bidPrice && tradePrice < askPrice)
            {
                buyPrice = tradePrice.ToString();
                sellPrice = tradePrice.ToString();
            }
            else
            {
                Log(string.Format("askprice:{0}, bidprice:{1}", askPrice, bidPrice));
                buyPrice = askPrice.ToString();
                sellPrice = bidPrice.ToString();
            }
        }

        private RT ReTry<RT>(Func<RT> act)
        {
            int failCnt = 0;
            while (true)
            {
                try
                {
                    return act();
                }
                catch (Exception e)
                {
                    Log("Retry");
                    failCnt++;
                    if (failCnt == 5)
                        throw e;
                }
                Thread.Sleep(100);
            }
        }

        private void Log(string msg)
        {
            Console.WriteLine("{0:hh-mm-ss:fff}:{1}", DateTime.Now, msg);
        }
    }
}
