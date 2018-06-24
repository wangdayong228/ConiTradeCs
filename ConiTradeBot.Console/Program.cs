using ConiTradeBot.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConiTradeBot.Run
{
    class Program
    {
        static void Main(string[] args)
        {
            //new ServiceTest().Start();
            //new CommonTest().TestMathRound();

            int count = 0;
            var bot = new Bot();
            while (true)
            {
                Console.WriteLine("Dig number:" + count++ + "------------------------------------------------");
                bot.Dig();
            }
            //Console.Read();
        }
    }
}
