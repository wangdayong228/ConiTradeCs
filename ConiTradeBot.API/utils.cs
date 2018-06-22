using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConiTradeBot.API
{
    public class utils
    {
        private static readonly HttpClient client = new HttpClient();

        public async static Task<string> Post(string url, Dictionary<string, string> body)
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
            return "{" + string.Join(",", lines) + "}";
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
        public static string GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}
