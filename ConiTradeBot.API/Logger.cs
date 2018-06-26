using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConiTradeBot.API
{
    public class Logger
    {
        public static string tradeLogPath = Path.Combine(Environment.CurrentDirectory, string.Format(@"log\trade_{0:MM_dd_hh_mm_ss}.csv",DateTime.Now));
        public static string infoLogPath = Path.Combine(Environment.CurrentDirectory, string.Format(@"log\info_{0:MM_dd_hh_mm_ss}.txt", DateTime.Now));

        public static void LogTradeResult(string orderType, string quantity, string symbol, string price, string orderStatus)
        {
            var tradeMsg = string.Format("{0},{1},{2},{3},{4}\r\n",symbol,orderType,price,quantity,orderStatus);
            FileHelper.AppendStrToFile(tradeMsg,tradeLogPath, false);
        }

        public static void LogInfo(string msg)
        {
            var entireMsg = string.Format("[{0:hh-mm-ss-fff}] {1}\r\n",DateTime.Now, msg);
            FileHelper.AppendStrToFile(entireMsg, infoLogPath);
        }
    }
}
