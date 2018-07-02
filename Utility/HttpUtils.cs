using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConiTradeBot.API
{
    public class HttpUtils
    {
        private static int requestCount;

        public static int RequestCount
        {
            get { return requestCount; }
            private set
            {
                requestCount = value;
                if (requestCount % 100 == 0)
                    Console.WriteLine("Request Count:" + requestCount);
            }
        }

        private readonly HttpClient client = new HttpClient();

        public HttpUtils()
        {
            //client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");
            client.Timeout = TimeSpan.FromSeconds(120);
        }
        
        public async Task<string> Post(string url, Dictionary<string, string> body)
        {
            RequestCount++;
            Console.Write("<p");
            var json = CovertToJson(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.Write(">");
            return responseString;
        }

        private string CovertToJson(Dictionary<string, string> body)
        {
            var lines = new List<string>();
            foreach (var item in body)
            {
                lines.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));
            }
            return "{" + string.Join(",", lines) + "}";
        }

        public Task<string> http_post_sign(string url, Dictionary<string, string> dic)
        {
            var mysign = signMd5(dic);
            dic.Remove("secret");
            dic.Add("sign", mysign);
            return Post(url, dic);
        }

        public string signMd5(Dictionary<string, string> kwargs)
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

        public string SHA256(string input)
        {
            var mysecret = Encoding.ASCII.GetBytes(input.ToUpper());
            var m = SHA256Managed.Create().ComputeHash(mysecret);
            return BitConverter.ToString(m).Replace("-", "").ToLower();
        }

        public async Task<string> http_get_nosign(string url)
        {
            RequestCount++;
            Console.Write("<g");
            var result = await client.GetStringAsync(url);
            Console.Write(">");
            return result;
        }

        public static string GenUrl(string urlRoot, Dictionary<string, string> param)
        {
            var lines = new List<string>();
            foreach (var item in param)
            {
                lines.Add(string.Format("{0}={1}", item.Key, item.Value));
            }
            var rawUrl = urlRoot + "&" + string.Join(",", lines);
            return System.Web.HttpUtility.UrlEncode(rawUrl);
        }

        public static string GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }
    }
}
