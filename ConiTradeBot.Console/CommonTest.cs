using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConiTradeBot.Run
{
    class CommonTest
    {
        public void TestMathRound()
        {
            Print(Math.Round(Amend(1.110), 2));
            Print(Math.Round(Amend(1.111), 2));
            Print(Math.Round(Amend(1.112), 2));
            Print(Math.Round(Amend(1.113), 2));
            Print(Math.Round(Amend(1.114), 2));
            Print(Math.Round(Amend(1.115), 2));
            Print(Math.Round(Amend(1.116), 2));
            Print(Math.Round(Amend(1.117), 2));
            Print(Math.Round(Amend(1.118), 2));
            Print(Math.Round(Amend(1.119), 2));
            Print(Math.Round(Amend(1.120), 2));
        }

        public double Amend(double value)
        {
            return value + 0.004;
        }

        void Print(double value)
        {
            Console.WriteLine(value.ToString());
        }
    }
}
