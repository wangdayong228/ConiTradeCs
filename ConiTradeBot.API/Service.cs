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

        static Service()
        {
            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json;charset=utf-8");
            //client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        public async static Task<string> Post(string url, Dictionary<string,string> body)
        {
            var json = CovertToJson(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        private static string CovertToJson(Dictionary<string, string> body)
        {
            var lines = new List<string>();
            foreach (var item in body)
            {
                lines.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));
            }
            return "{"+ string.Join(",", lines)+ "}";
        }

        public static Task<string> http_post_sign(string url, Dictionary<string, string> dic)
        {
            var mysign = sign(dic);
            dic.Remove("secret");
            dic.Add("sign", mysign);
            return Post(url, dic);
        }

        public static string sign(Dictionary<string, string> kwargs)
        {
            //将传入的参数生成列表形式，排序后用＆拼接成字符串，用hashbli加密成生sign
            var sign_list = new List<string>();
            foreach (var k in kwargs)
            {
                sign_list.Add(string.Format("{0}={1}", k.Key, k.Value));
            }
            sign_list.Sort();
            var sign_str = string.Join("&", sign_list);
            var mysecret = Encoding.ASCII.GetBytes(sign_str.ToUpper());
            var m = MD5.Create().ComputeHash(mysecret);
            return BitConverter.ToString(m).Replace("-", "").ToLower();
        }

        public async static Task<string> http_get_nosign(string url)
        {
            return await client.GetStringAsync(url);
        }

        public static Task<string> GetTicker(string symbol)
        {
            var url = market_url + "ticker?symbol=" + symbol;
            return http_get_nosign(url);
        }

        public static Task<string> get_orderbook(string symbol, int depth = 200)
        {
            var url = market_url + "orderbook?symbol=" + symbol + "&depth=" + depth;
            return http_get_nosign(url);
        }

        //获取成交记录
        public static Task<string> get_trade(string symbol, int size = 300)
        {
            /*
            size:获取记录数量，按照时间倒序传输。默认300
            */
            var url = market_url + "trades?symbol=" + symbol + "&size=" + size;
            return http_get_nosign(url);
        }

        //查询账户余额
        public static Task<string> post_balance(Dictionary<string,string> dic)
        {
            /*
            以字典形式传参
            apiid:可在coinbene申请,
            secret: 个人密钥(请勿透露给他人),
            timestamp: 时间戳,
            account: 默认为exchange，
            */
            var url = trade_url + "balance";
            return http_post_sign(url, dic);
        }



    }
}
