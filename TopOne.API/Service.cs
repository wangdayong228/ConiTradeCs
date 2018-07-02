using ConiTradeBot.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TopOne.API
{
    public class Service
    {
        HttpUtils utils=new HttpUtils();
        string appid = "xxxxxxxxxxxxxxxxxxxxxxxxxx";
        string appkey = "xxxxxxxxxxxxxxxxxxxxxxxxxx";

        private string getToken()
        {
            var num = new Random().Next(100000, 999999);
            var curtime = HttpUtils.GetTimestamp();
            var data = "appkey=" + appkey + "&random=" + num + "&time=" + curtime;
            var signature = utils.SHA256(data);
            var param = new Dictionary<string, string>() {
                { "appid",appid},
                { "time",curtime},
                { "random",num.ToString()},
                { "sig",signature},
            };
            var url = HttpUtils.GenUrl("https://server.top.one/api/apiToken", param);
            var rsps = utils.http_get_nosign(url).Result;
            var token = JObject.Parse(rsps)["data"]["apitoken"].ToString();
            return token;
        }

        public string GetBalance()
        {

        }
    }
}
