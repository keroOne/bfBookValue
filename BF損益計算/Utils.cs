using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace BF損益計算 {
    class TradeResult    {//取引結果(損益額と簿価を保持)
        //取引データ
        public TradeItem TradeItem { get; set; }
        //価格情報(他通貨間の取引の場合に円換算するために使用)
        public BfPrices BfPrices { get; set; }
        //取引後の残(数量と円換算簿価)
        public double BokaBtc { get; set; }
        public double BokaEth { get; set; }
        public double BokaBch { get; set; }
        //証拠金に差し入れた分を考慮しない残高数量(BTCは証拠金に差し入れることができるので正味の残数量の管理が必要)
        public double GrossZanBtc { get; set; }
        //ETHとBCHも用意する
        public double GrossZanEth { get; set; }
        public double GrossZanBch { get; set; }
        //JPYも
        public double GrossZanJpy { get; set; }
        //損益(円換算なので円を買ったとき以外の取引について計算した値を保持する)
        public double PlBtc2Eth { get; set; }
        public double PlBtc2Bch { get; set; }
        public double PlBch2Btc { get; set; }
        public double PlEth2Btc { get; set; }
        public double PlBtc2Jpy { get; set; }
    }

    class TradeItem {//BitflyerのTradeHistoryのデータをそのまま格納する
        public DateTime Time { get; set; }
        public string CurrPair { get; set; }//通貨ペア
        public string Kind { get; set; }//取引種類 ex:売り、買い...
        public double Price { get; set; }//約定単価
        public double Btc { get; set; }//BTC取引量(買いがプラス)
        public double BtcCom { get; set; }//BTC手数料(手数料無料期間以外は常にマイナス)
        public double BtcZan { get; set; }//BTC残高 前回残高+取引量+手数料=今回残高 
                                          //ただしその通貨が取引に関係していないときは0がセットされている
        public double Eth { get; set; }//BTCと同じ考え方
        public double EthCom { get; set; }
        public double EthZan { get; set; }
        public double Bch { get; set; }
        public double BchCom { get; set; }
        public double BchZan { get; set; }
        public double Jpy { get; set; }
        public double JpyZan { get; set; }
    }
    class OHLCV {//四本値と出来高を格納する(Closeだけでいいんだけど...)
        //cryptowatchから取得したデータを格納するけど日足のCloseが何時の値段なのかよくわからない...
        public DateTime Time { get; set; }
        public double O { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double C { get; set; }
        public double V { get; set; }
    }
    class BfPrices {
        public DateTime Time { get; set; }
        public double JPY_BTC { get; set; }
        public double BTC_BCH { get; set; }
        public double BTC_ETH { get; set; }
    }

    static class CurrencyConverter {//その日の終値で円換算する
        public static List<BfPrices> History = null;
        public static double Btc2Jpy(this double btc, DateTime time) {
            var rate = History.Where(b => b.Time.Date == time.Date).First().JPY_BTC;
            return btc * rate;
        }
        public static double Jpy2Btc(this double jpy, DateTime time) {
            var rate = History.Where(b => b.Time.Date == time.Date).First().JPY_BTC;
            return jpy / rate;
        }
        public static double Bch2Jpy(this double bch, DateTime time) {
            var bfp = History.Where(b => b.Time.Date == time.Date).First();
            var btcBch = bfp.BTC_BCH;
            var jpyBtc = bfp.JPY_BTC;
            return bch * btcBch * jpyBtc;
        }
        public static double Jpy2Bch(this double jpy, DateTime time) {
            var bfp = History.Where(b => b.Time.Date == time.Date).First();
            var btcBch = bfp.BTC_BCH;
            var jpyBtc = bfp.JPY_BTC;
            return jpy / jpyBtc / btcBch;
        }
        public static double Eth2Jpy(this double eth, DateTime time) {
            var bfp = History.Where(b => b.Time.Date == time.Date).First();
            var btcEth = bfp.BTC_ETH;
            var jpyBtc = bfp.JPY_BTC;
            return eth * btcEth * jpyBtc;
        }
        public static double Jpy2Eth(this double jpy, DateTime time) {
            var bfp = History.Where(b => b.Time.Date == time.Date).First();
            var btcEth = bfp.BTC_ETH;
            var jpyBtc = bfp.JPY_BTC;
            return jpy / jpyBtc / btcEth;
        }

        public static double BtcSellRatio(this TradeResult tr) {//売却量の売却前全残数量に対する割合(BTC)
            var r = (-tr.TradeItem.Btc - tr.TradeItem.BtcCom) / (tr.GrossZanBtc);//TradeItem.BtcZanじゃないよ
            return r;
        }
        public static double EthSellRatio(this TradeResult tr) {//売却量の売却前全残数量に対する割合(ETH)
            var r= (-tr.TradeItem.Eth - tr.TradeItem.EthCom) / (tr.GrossZanEth);
            return r;
        }
        public static double BchSellRatio(this TradeResult tr) {//売却量の売却前全残数量に対する割合(BCH)
            var r= (-tr.TradeItem.Bch - tr.TradeItem.BchCom) / (tr.GrossZanBch);
            return r;
        }
    }

    static class Sutils {
        static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0);
        public static DateTime ToDateTimeFromUnix(this double utime) {
            return UNIX_EPOCH.AddSeconds(utime);
        }
        //from neue.cc
        public static Task WhenAll(this IEnumerable<Task> tasks) {
            return Task.WhenAll(tasks);
        }

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) {
            return Task.WhenAll(tasks);
        }
    }

    class Utils {
        //cryptowatchから日足データを取得してエンティティのリストにして返すタスクを返す
        public async Task<List<OHLCV>> GetOlhcAsync(string market_currPair) { //ex market_currPair bitflyer/btcjpy
            var path = "/markets/" + market_currPair + "/ohlc?periods=86400";//60s * 60m * 24h 秒数で指定するので日足なら86400を渡す
            var data = await CallCwApiAsync(path);
            var s = data.Replace("{\"86400\":[[", "{\"X6\":[[");//キーの先頭文字が数字だとうまくないのでキーを置き換える
            dynamic j = DynamicJson.Parse(s);
            dynamic[] ohlvs = j.result.X6;
            var list = new List<OHLCV>();
            try {
                foreach (var o in ohlvs) {
                    double dd = o[0];
                    DateTime d = dd.ToDateTimeFromUnix();
                    var ohlcv = new OHLCV() {
                        Time = d, O = o[1], H = o[2], L = o[3], C = o[4], V = o[5], };
                    list.Add(ohlcv);
                }
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e) {
                Console.WriteLine(e.Message);
            }
            return list;
        }

        public async Task SavePriceHistoriesToFileAsync(string path) {
            var pairs = new string[] { "bitflyer/btcjpy", "bitflyer/ethbtc", "bitflyer/bchbtc" };//パス  市場/通貨
            var t = pairs.Select(pair => { return GetOlhcAsync(pair); }).ToList();
            await Task.WhenAll(t);
            var btcs = t[0].Result.OrderBy(h => h.Time); var eths = t[1].Result.OrderBy(h => h.Time); var bchs = t[2].Result.OrderBy(h => h.Time);
            var sb = new StringBuilder();
            sb.Append("日付,O,H,L,BTC_JPY,ETH_BTC,BCH_BTC\r\n");
            DateTime iDate = DateTime.Parse("2016/08/01 00:00:00");
            var btcc = 0d; var ethc = 0d; var bchc = 0d;
            var btco = 0d; var btch = 0d; var btcl = 0d;
            while (iDate < DateTime.Now) {
                try {
                    var btc = btcs.Where(h => h.Time == iDate).FirstOrDefault();
                    btcc = btc != null && btc.C != 0 ? btc.C : btcc;
                    btco = btc != null && btc.O != 0 ? btc.O : btco;
                    btch = btc != null && btc.H != 0 ? btc.H : btch;
                    btcl = btc != null && btc.L != 0 ? btc.L : btcl;
                    var eth = eths.Where(h => h.Time == iDate).FirstOrDefault();
                    ethc = eth != null && eth.C != 0 ? eth.C : ethc;
                    var bch = bchs.Where(h => h.Time == iDate).FirstOrDefault();
                    bchc = bch != null && bch.C != 0 ? bch.C : bchc;
                    sb.Append(String.Format("{0},{1},{2},{3},{4},{5},{6} \r\n", iDate.AddDays(-1), btco, btch, btcl, btcc, ethc, bchc));//チャートと比べると一日ずれているので
                    iDate = iDate.AddDays(1);
                }
                catch (ApplicationException ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
            using (var sw = new StreamWriter(path, false, Encoding.UTF8)) {
                sw.Write(sb.ToString());
            }
        }

        public void SaveTradeResultToFile(List<TradeResult> tradeResults, string path) {
            var sb = new StringBuilder();
            sb.Append("日付,通貨,取引,BTC,ETH,BCH,BTC損益,ETH損益,BCH損益,BTC簿価,ETH簿価,BCH簿価,JPY増減,BTC増減,ETH増減,BCH増減,JPY残,BTC残,ETH残,BCH残\r\n");
            tradeResults.ForEach(t => {
                sb.Append(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}\r\n",
                    t.TradeItem.Time, t.TradeItem.CurrPair, t.TradeItem.Kind,
                    t.BfPrices.JPY_BTC, t.BfPrices.BTC_ETH, t.BfPrices.BTC_BCH,
                    t.PlBtc2Jpy, t.PlEth2Btc, t.PlBch2Btc,
                    t.BokaBtc, t.BokaEth, t.BokaBch,
                    t.TradeItem.Jpy,
                    t.TradeItem.Btc + t.TradeItem.BtcCom,
                    t.TradeItem.Eth + t.TradeItem.EthCom,
                    t.TradeItem.Bch + t.TradeItem.BchCom,
                    t.GrossZanJpy, t.GrossZanBtc, t.GrossZanEth, t.GrossZanBch));
            });
            using (var sw = new StreamWriter(path, false, Encoding.GetEncoding("Shift_JIS"))){//.UTF8)) {
                sw.Write(sb.ToString());
            }
        }

        //BitFlyerのCSVファイルから取引データを取得する
        public List<TradeItem> LoadTradeHistoriesFromFile(string path) {
            const int Time = 0, CurrPair = 1, Kind = 2, Price = 3, Btc = 4, BtcCom = 5, BtcZan = 6, Jpy = 7, JpyZan = 8, Eth = 9, EthCom = 10, EthZan = 11, Bch = 18, BchCom = 19, BchZan = 20;
            var result = new List<TradeItem>();
            using (var parser = new TextFieldParser(path, Encoding.GetEncoding("Shift_JIS"))) {// "utf-8"))) {//Shift_JIS"))) {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                var headers = parser.ReadFields();//ヘッダ行を読む(ヘッダから始まると決めつけている)
                while (!parser.EndOfData) {
                    var values = parser.ReadFields();
                    var item = new TradeItem() {
                        Time = DateTime.Parse(values[Time]),
                        CurrPair = values[CurrPair],
                        Kind = values[Kind],
                        Price = double.Parse(values[Price]),
                        Btc = double.Parse(values[Btc]),
                        BtcCom = double.Parse(values[BtcCom]),
                        BtcZan = double.Parse(values[BtcZan]),
                        Jpy = double.Parse(values[Jpy]),
                        JpyZan = double.Parse(values[JpyZan]),
                        Eth = double.Parse(values[Eth]),
                        EthCom = double.Parse(values[EthCom]),
                        EthZan = double.Parse(values[EthZan]),
                        Bch = double.Parse(values[Bch]),
                        BchCom = double.Parse(values[BchCom]),
                        BchZan = double.Parse(values[BchZan])
                    };
                    result.Add(item);
                }
            }
            return result;
        }

        internal List<BfPrices> LoadPriceHistoriesFromFile(string path) {
            const int Time = 0, Btc = 4, Eth = 5, Bch = 6;
            var result = new List<BfPrices>();
            using (var parser = new TextFieldParser(path, Encoding.UTF8)) {// "utf-8"))) {//Shift_JIS"))) {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                var headers = parser.ReadFields();//ヘッダ行を読む(ヘッダから始まると決めつけている)
                while (!parser.EndOfData) {
                    var values = parser.ReadFields();
                    var item = new BfPrices() {
                        Time = DateTime.Parse(values[Time]),
                        JPY_BTC = double.Parse(values[Btc]),
                        BTC_ETH = double.Parse(values[Eth]),
                        BTC_BCH = double.Parse(values[Bch])
                    };
                    result.Add(item);
                }
            }
            return result;
        }
        async Task<string> CallCwApiAsync(string path) {
            var method = "GET";
            var query = "";//
            var endPoint = new Uri("https://api.cryptowat.ch/markets");
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(new HttpMethod(method), path + query)) {
                client.BaseAddress = endPoint;
                var message = await client.SendAsync(request);
                var response = await message.Content.ReadAsStringAsync();
                return response;
            }
        }
    }

}
