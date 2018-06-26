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

        const string QUANTITY = "0.4";
        const string SYMBOL = "ethbtc";
        const int PRECISION = 6;
        bool priceStable = false;

        Service mainService = Service.Create();

        public Bot()
        {
            LoopCheckPriceStable();
        }

        private void LoopCheckPriceStable()
        {
            new Thread(() =>
            {
                var tempService = Service.Create();
                //连续10次价格变化太大，不做
                var capcity = 10;
                Queue<double> bidPrices = new Queue<double>(capcity);
                Queue<double> askPrices = new Queue<double>(capcity);
                while (true)
                {
                    var ticker = JObject.Parse(tempService.get_ticker(SYMBOL).Result)["ticker"][0];

                    if (bidPrices.Count == capcity)
                    {
                        bidPrices.Dequeue();
                        askPrices.Dequeue();
                    }
                    bidPrices.Enqueue(double.Parse(ticker["bid"].ToString()));
                    askPrices.Enqueue(double.Parse(ticker["ask"].ToString()));

                    if (bidPrices.Count == capcity)
                    {
                        var orderdBidPrice = bidPrices.OrderBy(p => p).ToList();
                        var orderdAslPrice = askPrices.OrderBy(p => p).ToList();
                        var bidStable = (orderdBidPrice[capcity-1] - orderdBidPrice[0]) / orderdBidPrice[capcity-1] <= 0.002;
                        var askStable = (orderdAslPrice[capcity-1] - orderdAslPrice[0]) / orderdAslPrice[capcity-1] <= 0.002;
                        priceStable = bidStable && askStable;
                    }
                    Thread.Sleep(2000);
                }
            }).Start();
        }

        public void Dig()
        {
            try
            {
                if (!priceStable)
                {
                    Console.WriteLine("price not stable, skip");
                    Thread.Sleep(1000);
                    return;
                }
                //Todo:
                //获取深度，取买单最高价，最后一位加1挂单买N USDT的ETH，如1ETH=500USDT，挂单ETHUSD：buy-limit 500.01USDT，数量1
                //同时挂单等价的卖单： sell-limit 500.01USDT, 数量1
                //查询是否成交，如有没有成交的立即调整价格让其成交
                //等成交后再挂下一单
                string buyPrice, sellPrice;
                int waitBuyTime = 0, waitSellTime = 0;
                CalcMyTradePrice(out buyPrice, out sellPrice);

                var buyOrder = ReTryPlaceOrderUntilSucess(SYMBOL, "buy-limit", buyPrice);// ReTry(() => Service.post_order_place("ethusdt", "buy-limit", myPlanPrice, quantity)).Result;
                Log(string.Format("place {0} order:{1} {2}", "buy", buyPrice, buyOrder["status"]));
                if (!PlaceOrderSucess(buyOrder))
                    return;

                var sellOrder = ReTryPlaceOrderUntilSucess(SYMBOL, "sell-limit", sellPrice);// ReTry(() => Service.post_order_place("ethusdt", "sell-limit", myPlanPrice, quantity)).Result;
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
            catch (Exception e)
            {
                Log(e.Message + "\r\n" + e.StackTrace);
                Thread.Sleep(10000);
            }
        }

        public JObject ReTryPlaceOrderUntilSucess(string symbol, string type, string price, string quantity = QUANTITY)
        {
            string description = "";
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var rspns = JObject.Parse(mainService.post_order_place(symbol, type, price, quantity).Result);
                    if (rspns["status"].ToString() == "ok")
                        return rspns;
                    description = rspns["description"].ToString();
                }
                catch (Exception e)
                {
                    Log(i + " Retry place order error" + e);
                }
                if (description == "System Busy.")
                {
                    Thread.Sleep(1000);
                }
                else if (description == "Insufficient balance of assets")
                {
                    throw new Exception(description);
                }
            }
            throw new Exception(description);
        }

        public bool PlaceOrderSucess(JObject order)
        {
            return order["status"].ToString() == "ok";
        }

        public bool WaitOrderDone(string planPrice, ref JObject order, string orderType, int waitTime)
        {
            //Log("wait " + orderType + " done");
            var sellOrderId = order["orderid"].ToString();
            var rspns = JObject.Parse(mainService.post_info(sellOrderId).Result);
            Thread.Sleep(200);
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
                var price = orderRslt["price"].ToString();
                var quantity = orderRslt["orderquantity"].ToString();
                Log(string.Format("{0} {1} {2}, price:{3}, status:{4}", orderType, quantity, SYMBOL, price, orderStatus));
                Logger.LogTradeResult(orderType, quantity, SYMBOL, price, orderStatus);
                return true;
            }
            try
            {
                var filledQuantity = orderStatus == "unfilled" ? "0" : orderRslt["filledquantity"].ToString();
                var unfilledQuantity = double.Parse(orderRslt["orderquantity"].ToString()) - double.Parse(filledQuantity);
                string buyPrice, sellPrice;
                CalcFastTradePrice(out buyPrice, out sellPrice);
                Thread.Sleep(200);
                var nowPrice = orderType == "buy" ? buyPrice : sellPrice;
                Log("now "+ orderType+ " price:" + nowPrice);
                if (nowPrice == planPrice)
                    return false;
                //cancle and reorder
               
                var nowRatio = double.Parse(nowPrice) / double.Parse(planPrice);
                var hasProfitSpace = orderType == "buy" ? (nowRatio <= limitRatio) : (nowRatio >= limitRatio);
                var waitTimeLimited = waitTime > 10;
             
                double lossRatio = 0;
                if (waitTimeLimited)
                    lossRatio = orderType == "buy" ? (nowRatio - limitRatio) : (limitRatio - nowRatio );

                //if wait more than 10 times and loss more than 0.03%, just return and leave it fill auto
                if (waitTimeLimited && lossRatio > 0.0009)
                {
                    Log("wait" + orderType + " more than 10 times and loss ratio more than 0.0003, return");
                    return true;
                }
                
                //if have profit space or wait time more than 10 times and not loss than 0.03%
                if (hasProfitSpace || (waitTimeLimited && lossRatio <= 0.0003))
                {
                    do
                    {
                        var cancleRspns = JObject.Parse(mainService.post_cancel(sellOrderId).Result);//cancel order
                        Log(string.Format("cancle {0} order {1} {2}: {3}", orderType, sellOrderId, cancleRspns["status"], cancleRspns["description"]));
                        if (cancleRspns["status"].ToString() == "error")//response error, retry, may has done
                            return false;

                        Thread.Sleep(500);
                        var orders = JObject.Parse(mainService.post_open_orders("ethusdt").Result.ToString());
                        var content = orders["orders"].ToString();
                        if (string.IsNullOrEmpty(content))//orders is null means has canceled, and could start to reorder;
                            break;

                        var items = orders["orders"]["result"].ToArray();
                        var match = items.FirstOrDefault(i => i["orderid"].ToString() == sellOrderId);
                        if (match == null)//no matched means has canceled too, could start to reorder;
                            break;
                        Thread.Sleep(200);
                    } while (true);

                    order = ReTryPlaceOrderUntilSucess(SYMBOL, orderType + "-limit", nowPrice, unfilledQuantity.ToString());//reorder for unfilled.
                    Thread.Sleep(1100);
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
            var orderbooks = mainService.get_orderbook(SYMBOL, 2).Result;
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
            var orderbooks = mainService.get_orderbook(SYMBOL, 2).Result;
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
                Thread.Sleep(200);
            }
        }

        private void Log(string msg)
        {
            Console.WriteLine("{0:hh-mm-ss:fff}:{1}", DateTime.Now, msg);
            Logger.LogInfo(msg);
        }
    }
}
