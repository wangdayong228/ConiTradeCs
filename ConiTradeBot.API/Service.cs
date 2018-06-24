using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;

namespace ConiTradeBot.API
{
    public class Service
    {
        private static readonly HttpClient client = new HttpClient();
        private static string market_url = "http://api.coinbene.com/v1/market/";
        private static string trade_url = "http://api.coinbene.com/v1/trade/";

        //no ip
        const string appid = "799926456b0add7e4b2bf29091fd17f1";
        const string secret = "4b6152883ed045658499cd1ae5073609";

        //111.194.46.100
        //const string appid = "b9795ff6245f36b47f8e69fc2c0b04e1";
        //const string secret = "317fba58e5914d38a946149220221030";

        //1.234.31.68
        //const string appid = "45800d071257e47e3193b58b426ee71b";
        //const string secret = "f6466a19c7664a9ca81aa59010ff09c0";

        static Service()
        {
            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json;charset=utf-8");
            //client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        //获取最新价
        public static Task<string> get_ticker(string symbol)
        {
            var url = market_url + "ticker?symbol=" + symbol;
            return utils.http_get_nosign(url);
        }

        //获取挂单
        public static Task<string> get_orderbook(string symbol, int depth = 200)
        {
            var url = market_url + "orderbook?symbol=" + symbol + "&depth=" + depth;
            return utils.http_get_nosign(url);
        }

        //获取成交记录
        public static Task<string> get_trade(string symbol, int size = 300)
        {
            /*
            size:获取记录数量，按照时间倒序传输。默认300
            */
            var url = market_url + "trades?symbol=" + symbol + "&size=" + size;
            return utils.http_get_nosign(url);
        }



        //查询账户余额
        public static Task<string> post_balance(Dictionary<string, string> dic)
        {
            /*
            以字典形式传参
            apiid:可在coinbene申请,
            secret: 个人密钥(请勿透露给他人),
            timestamp: 时间戳,
            account: 默认为exchange，
            */
            var url = trade_url + "balance";
            return utils.http_post_sign(url, dic);
        }

        //查询账户余额
        public static Task<string> post_balance()
        {
            /*
            以字典形式传参
            apiid:可在coinbene申请,
            secret: 个人密钥(请勿透露给他人),
            timestamp: 时间戳,
            account: 默认为exchange，
            */
            var url = trade_url + "balance";
            var dic = new Dictionary<string, string> {
                { "apiid",appid },
                { "secret",secret },
                { "timestamp",utils.GetTimestamp() },
                { "account","exchange"} };
            return utils.http_post_sign(url, dic);
        }

        //下单
        public static Task<string> post_order_place(Dictionary<string, string> dic)
        {
            /*
            以字典形式传参
            apiid, symbol, timestamp
            type: 可选 buy-limit / sell - limit
            price: 购买单价
             quantity:购买数量
            */
            var url = trade_url + "order/place";
            return utils.http_post_sign(url, dic);
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="type">buy-limit or sell-limit</param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="symbol">such as "btcusdt"</param>
        /// <returns></returns>
        public static Task<string> post_order_place(string symbol, string type,string price,string quantity)
        {
            /*
            以字典形式传参
            apiid, symbol, timestamp
            type: 可选 buy-limit / sell - limit
            price: 购买单价
             quantity:购买数量
            */
            var url = trade_url + "order/place";
            var dic = new Dictionary<string, string> {
            { "apiid",appid },
            { "secret",secret },
            { "timestamp",utils.GetTimestamp() },
            { "type",type },
            { "price",price },
            { "quantity",quantity },
            { "symbol",symbol } };
            return utils.http_post_sign(url, dic);
        }

        // 查询委托
        public static Task<string> post_info(Dictionary<string, string> dic)
        {
            /*
            以字典形式传参
            apiid, timestamp, secret, orderid
            */
            var url = trade_url + "order/info";
            return utils.http_post_sign(url, dic);
        }


        /// <summary>
        /// 查询委托
        /// </summary>
        /// <param name="orderid">201806201043458241111111</param>
        /// <returns></returns>
        public static Task<string> post_info(string orderid)
        {
            /*
            以字典形式传参
            apiid, timestamp, secret, orderid
            */
            var url = trade_url + "order/info";
            var dic = new Dictionary<string, string> {
             { "apiid",appid },
             { "secret",secret },
             { "timestamp",utils.GetTimestamp()},
             { "orderid",orderid}
            };
            return utils.http_post_sign(url, dic);
        }

        //查询当前委托
        public static Task<string> post_open_orders(Dictionary<string, string> dic) {
            /*
            以字典形式传参
            apiid, timestamp, secret, symbol
            */
            var url = trade_url + "order/open-orders";
            return utils.http_post_sign(url, dic);
        }

        //查询当前委托
        public static Task<string> post_open_orders(string symbol)
        {
            /*
            以字典形式传参
            apiid, timestamp, secret, symbol
            */
            var url = trade_url + "order/open-orders";
            var dic = new Dictionary<string, string> {
                { "apiid",appid},
                { "secret",secret},
                { "timestamp",utils.GetTimestamp() },
                { "symbol",symbol}};
            return utils.http_post_sign(url, dic);
        }

        //撤单
        public static Task<string> post_cancel(Dictionary<string, string> dic) {
            /*
            以字典形式传参
            apiid, timestamp, secret, orderid
            */
            var url = trade_url + "order/cancel";
            return utils.http_post_sign(url, dic);
        }

        //撤单
        public static Task<string> post_cancel(string orderid)
        {
            /*
            以字典形式传参
            apiid, timestamp, secret, orderid
            */
            var url = trade_url + "order/cancel";
            var dic = new Dictionary<string, string>{
                { "apiid",appid },
                { "secret",secret },
                { "timestamp",utils.GetTimestamp()  },
                { "orderid",orderid } };
            return utils.http_post_sign(url, dic);
        }


    }
}
