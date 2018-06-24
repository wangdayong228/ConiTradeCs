using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConiTradeBot.API;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConiTradeBot.API.Test
{
    [TestClass]
    public class UnitTest1
    {
        //# 查询最新价
        [TestMethod]
        public void Test_get_ticker()
        {
            print(Service.get_ticker("btcusdt"));
        }

        [TestMethod]
        public void Test_get_orderbook()
        {
            print(Service.get_orderbook("btcusdt", 5));
        }

        [TestMethod]
        public void Test_get_trade()
        {
            print(Service.get_trade("ethusdt", 2));
        }

        [TestMethod]
        public void Test_post_balance()
        {
            var dic = new Dictionary<string, string> {
                { "apiid","xxx" },
                { "secret","xxx" },
                { "timestamp","11223112231" },
                { "account","exchange"} };
            print(Service.post_balance(dic));
        }

        [TestMethod]
        public void Test_post_order_place()
        {
            var dic = new Dictionary<string, string> {
            { "apiid","xxx" },
            { "secret","xxx" },
            { "timestamp","1122311221131" },
            { "type","buy-limit" },
            { "price","0.003401" },
            { "quantity","1" },
            { "symbol","swtcusdt" } };
            print(Service.post_order_place(dic));
        }

        [TestMethod]
        public void Test_post_info()
        {
            var dic = new Dictionary<string, string> {
             { "apiid","xxx" },
             { "secret","xxx" },
             { "timestamp","1122311211231" },
             { "orderid","201806201043458241111111"}
            };
            print(Service.post_info(dic));
        }

        [TestMethod]
        public void Test_post_open_orders()
        {
            var dic = new Dictionary<string, string> {
                { "apiid","xxx"},
                { "secret","xxx"},
                { "timestamp","1122311211231"},
                { "symbol","swtcusdt"}};
            print(Service.post_open_orders(dic));
        }

        [TestMethod]
        public void Test_post_cancel()
        {
            var dic = new Dictionary<string, string>{
                { "apiid","xxx" },
                { "secret","xxx" },
                { "timestamp","112231121111231" },
                { "orderid","201806201043458241111111" } };
            print(Service.post_cancel(dic));
        }

        private void print(Task<string> task)
        {
            Console.WriteLine(task.Result);
        }

        [TestMethod]
        public void TestMathRound()
        {
            Math.Round(1.111, 2);
            Math.Round(1.112, 2);
            Math.Round(1.113, 2);
            Math.Round(1.114, 2);
            Math.Round(1.115, 2);
            Math.Round(1.116, 2);
            Math.Round(1.117, 2);
            Math.Round(1.118, 2);
            Math.Round(1.119, 2);
        }
    }
}
