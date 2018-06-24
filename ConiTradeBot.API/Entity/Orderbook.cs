using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConiTradeBot.API.Entity
{
    public class Orderbook
    {
        public string status { get; set; }
        public string symbol { get; set; }
        public string timestamp { get; set; }
        public OrderbookDetail orderbook { get; set; }
    }

    public class OrderbookDetail {
        public List<OrderbookDetailItem> asks { get; set; }
        public List<OrderbookDetailItem> bids { get; set; }
    }

    public class OrderbookDetailItem {
        public string price { get; set; }
        public string quantity { get; set; }
    }
}
