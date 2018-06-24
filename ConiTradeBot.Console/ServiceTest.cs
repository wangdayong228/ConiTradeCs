using ConiTradeBot.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConiTradeBot.Run
{
    public class ServiceTest
    {
        //const string appid = "请填写你自己的APPID";
        //const string secret = "请填写你自己的SECRET";

        //no ip
        const string appid = "799926456b0add7e4b2bf29091fd17f1";
        const string secret = "4b6152883ed045658499cd1ae5073609";


        public void Start()
        {
            Test_get_ticker();
            Test_get_orderbook();
            Test_get_trade();
            Test_post_balance();
            Test_post_order_place();
            Test_post_info();
            Test_post_open_orders();
            Test_post_cancel();
        }

        //查询最新价
        public void Test_get_ticker()
        {
            print(Service.get_ticker("btcusdt"));
        }
        //查询结果:{'timestamp': 1529461186102, 'status': 'ok', 'ticker': 
        //[{'symbol': 'BTCUSDT', '24hrHigh': '6832.65', 'ask': '6603.77', '24hrVol': '5329.3321', '24hrLow': '6547.0', 'bid': '6598.89', 'last': '6598.90', '24hrAmt': '35754346.714095'}]}



        // 查询挂单,需要传入参数symbol,depth默认为200
        public void Test_get_orderbook()
        {
            print(Service.get_orderbook("btcusdt", 5));
        }
        // 查询结果:{'status': 'ok', 
        // 'orderbook': {
        //         'asks': [{'quantity': 0.0678, 'price': 6612.78}, 
        // {'quantity': 0.16, 'price': 6629.01}, 
        // {'quantity': 0.0559, 'price': 6644.22}, 
        // {'quantity': 0.3679, 'price': 6660.9}, 
        // {'quantity': 0.01, 'price': 6679.6}], 
        //         'bids': [{'quantity': 0.5475, 'price': 6580}, 
        // {'quantity': 0.0146, 'price': 6579.16},
        // {'quantity': 0.4093, 'price': 6579.11}, 
        // {'quantity': 0.0071, 'price': 6563.46}, 
        // {'quantity': 0.01, 'price': 6563.41}]}, 
        //         'timestamp': 1529461342333, 
        // 'symbol': 'BTCUSDT'}


        // 查询成交记录,传参:symbol和size默认为300
        public void Test_get_trade()
        {
            print(Service.get_trade("ethusdt", 2));
        }
        // 查询结果:{'timestamp': 1529461627036, 
        //         'trades': [{'tradeId': '201806201026349790017791201806201026316910017482', 
        //                     'quantity': '0.24', 
        //                     'time': 1529461595000, 
        //                     'price': '520.18', 
        //                     'take': 'buy'}, 
        //                    {'tradeId': '201806201024559520019121201806201026316910017482',
        //                     'quantity': '0.11', 
        //                     'time': 1529461591000, 
        //                     'price': '520.18', 
        //                     'take': 'sell'}],
        //         'status': 'ok', 
        //         'symbol': 'ETHUSDT'}


        // 查询余额,此处以字典形式传参
        public void Test_post_balance()
        {
            var dic = new Dictionary<string, string> {
                { "apiid",appid },
                { "secret",secret },
                { "timestamp",utils.GetTimestamp() },
                { "account","exchange"} };
            print(Service.post_balance(dic));
        }
        // 查询结果:{
        //         "account":"exchange",
        //         "balance":[
        //         {
        //             "asset":"ACT",
        //             "available":"999999.0000000000000000",
        //             "reserved":"0.0000000000000000",
        //             "total":"999999.0000000000000000"
        //         },
        //         {
        //             "asset":"AE",
        //             "available":"999999.0000000000000000",
        //             "reserved":"0.0000000000000000",
        //             "total":"999999.0000000000000000"
        //         }
        //     ],
        //         "status":"ok",
        //         "timestamp":1517536673213
        // }


        // 下订单,以字典形式传参
        public void Test_post_order_place()
        {
            var dic = new Dictionary<string, string> {
            { "apiid",appid },
            { "secret",secret },
            { "timestamp",utils.GetTimestamp() },
            { "type","buy-limit" },
            { "price","0.003401" },
            { "quantity","10" },
            { "symbol","swtcusdt" } };
            print(Service.post_order_place(dic));
        }
        // 查询结果:{'status': 'ok', 'timestamp': 1529462625853, 'orderid': '201806201043458241111111'}


        // 按订单号查询委托,以字典形式传参
        public void Test_post_info()
        {
            var dic = new Dictionary<string, string> {
             { "apiid",appid },
             { "secret",secret },
             { "timestamp",utils.GetTimestamp()  },
             { "orderid","201806201043458241111111"}
            };
            print(Service.post_info(dic));
        }
        // 查询结果:{
        //         'order': {'filledquantity': '0', 
        //               'symbol': 'SWTCUSDT', 
        //               'type': 'buy-limit', 
        //               'filledamount': '0', 
        //               'price': '0.003401', 
        //               'orderquantity': '1', 
        //               'orderstatus': 'unfilled', 
        //               'orderid': '201806201043458241111111', 
        //               'createtime': 1529491426000}, 
        //         'status': 'ok', 
        //         'timestamp': 1529463050492}


        // 查询当前委托,以字典形式传参
        public void Test_post_open_orders()
        {
            var dic = new Dictionary<string, string> {
                { "apiid",appid},
                { "secret",secret},
                { "timestamp",utils.GetTimestamp() },
                { "symbol","swtcusdt"}};
            print(Service.post_open_orders(dic));
        }
        // 查询结果:{
        //     "orders":{
        //         "page":1,
        //         "pagesize":10000,
        //         "result":[
        //             {
        //                 "createtime":"2018-03-14 00:00:00",
        //                 "filledamount":"1.0010000000000000",
        //                 "filledquantity":"1.00000000",
        //                 "lastmodified":"2018-03-14 00:00:00",
        //                 "orderid":"B180124163642823625172",
        //                 "orderquantity":"1.00000000",
        //                 "orderstatus":"partialfilled",
        //                 "price":"1.00000000",
        //                 "symbol":"btcusdt",
        //                 "type":"buy-limit"
        //             },
        //             {
        //                 "createtime":"2018-03-14 00:00:00",
        //                 "filledamount":"0.0000000000000000",
        //                 "filledquantity":"0.10000000",
        //                 "lastmodified":"2018-03-14 00:00:00",
        //                 "orderid":"S180123183111553329941",
        //                 "orderquantity":"0.10000000",
        //                 "orderstatus":"partialfilled",
        //                 "price":"11000.00000000",
        //                 "symbol":"btcusdt",
        //                 "type":"sell-limit"
        //             },
        //         ],
        //         "totalcount":3
        //     },
        //     "status":"ok",
        //     "timestamp":1517536673213
        // }

        // 撤销订单,以字典形式传参
        public void Test_post_cancel()
        {
            var dic = new Dictionary<string, string>{
                { "apiid",appid },
                { "secret",secret },
                { "timestamp",utils.GetTimestamp()  },
                { "orderid","201806222116212090013231" } };
            print(Service.post_cancel(dic));
        }
        // 查询结果:{'orderid': '201806201043458241111111', 
        //           'timestamp': 1529463751589, 
        //           'status': 'ok'}

        private void print(Task<string> task)
        {
            Console.WriteLine(task.Result);
        }
    }
}
