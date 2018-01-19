using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BF損益計算 {
    class Calcurator {
        internal List<TradeResult> Calc(List<TradeItem> tradeItems, List<BfPrices> priceHistories) {
            var tradeResults = new List<TradeResult>();
            //取引データを読んで売りなら引き落とすべき簿価を計算して簿価と数量を減算する、ついでに損益を計算する
            //買いなら簿価と数量を加算する
            //他通貨間なら円ベースの取得価額を計算して簿価に追加する
            var lastResult = new TradeResult { TradeItem = new TradeItem(), BokaBtc = 0, BokaEth = 0, BokaBch = 0, GrossZanBtc = 0 };//ひとつ前のものを参照しながら計算するので初回用を用意する
            tradeItems.ForEach(ti => {
                var result = new TradeResult {
                    TradeItem = ti,
                    BfPrices = priceHistories.Where(ph => ph.Time.Date == ti.Time.Date).First(),
                    BokaBtc = lastResult.BokaBtc,//動きがないときは前の値を引き継ぐのでとりあえずコピーしておく
                    BokaBch = lastResult.BokaBch,
                    BokaEth = lastResult.BokaEth,
                    GrossZanBtc = lastResult.GrossZanBtc,
                    GrossZanEth = lastResult.GrossZanEth,
                    GrossZanBch = lastResult.GrossZanBch,
                    GrossZanJpy = lastResult.GrossZanJpy,
                };
                if ((ti.CurrPair == "BTC/JPY" && ti.Kind == "買い") || (ti.CurrPair == "BTC" && ti.Kind == "受取")) {//1-1.BtcをJPYで買う場合
                    //購入後の簿価：JPYに-1をかけてBTCの簿価に足す
                    result.BokaBtc = result.BokaBtc + (-ti.Jpy);
                    //購入後の残数量：
                    result.GrossZanBtc = result.GrossZanBtc + ti.Btc + ti.BtcCom;
                    //購入後のJPYの数量
                    result.GrossZanJpy = result.GrossZanJpy + ti.Jpy;
                }
                else if ((ti.CurrPair == "BTC/JPY" && ti.Kind == "売り") || (ti.CurrPair == "BTC" && ti.Kind == "外部送付")) { //1-2.BTCをJPYに売る場合
                    //売却分の簿価：簿価(BTC JPY)×(-BTC-手数料(BTC))/(売却後の残高数量+(-売却量(BTC)-手数料(BTC))) 分母は売却前数量のこと
                    var sellBoka = result.BtcSellRatio() * result.BokaBtc;
                    //売却にかかる利益:JPY-売却分の簿価
                    result.PlBtc2Jpy = ti.Jpy - sellBoka;
                    //売却後の簿価：簿価(BTC JPY)
                    result.BokaBtc = result.BokaBtc - sellBoka;
                    //売却後の残数量：(数量と手数料は負数なので足せばいい)
                    result.GrossZanBtc = result.GrossZanBtc + ti.Btc + ti.BtcCom;
                    //売却後のJPYの数量
                    result.GrossZanJpy = result.GrossZanJpy + ti.Jpy;
                }
                else if (ti.CurrPair == "ETH/BTC" && (ti.Kind == "買い")) { //2-1.ETHをBTCで買う場合 
                    //ETHの簿価(円貨)：BTCに-1をかけたものにJPYBTCをかけてETHの簿価に足す(手数料も簿価に含める)
                    result.BokaEth = result.BokaEth + (-ti.Btc.Btc2Jpy(ti.Time));
                    //(BfのExcelのEthZanは動きがないときに0になっていて使いにくいのでここで常に計算してその値を使う)
                    result.GrossZanEth = result.GrossZanEth + ti.Eth + ti.EthCom;
                    //BTCの簿価(Ethを購入するためにBTCをJPYに売却したとみなす)
                    var sellBoka = result.BokaBtc * result.BtcSellRatio();
                    result.BokaBtc = result.BokaBtc - sellBoka;
                    //BTCの売却益(減少数量分の時価-簿価)
                    //result.PlBtc2Jpy = -ti.Btc * result.BfPrices.JPY_BTC - sellBoka;
                    result.PlBtc2Jpy = -ti.Btc.Btc2Jpy(ti.Time) - sellBoka;
                    //BTCの売却後残数量
                    result.GrossZanBtc = result.GrossZanBtc + ti.Btc;
                }
                else if ((ti.CurrPair == "ETH/BTC" && ti.Kind == "売り") || (ti.CurrPair == "ETH" && ti.Kind == "外部送付")) {//2-2.ETHをBTCに売る場合
                    //ETHの売却分の簿価：簿価(ETH JPY)×(-EHT-手数料)/(残高数量ETH+(-ETH-手数料(ETH)))×JPYBTC
                    var sellBoka = result.BokaEth * result.EthSellRatio();
                    //ETHの売却にかかる利益:BTC×JPYBTC-ETH売却分の簿価
                    var incomeInJpy = -(ti.Eth - ti.EthCom).Eth2Jpy(ti.Time);// ti.Btc.Btc2Jpy(ti.Time);
                    result.PlEth2Btc = incomeInJpy - sellBoka;
                    //ETH売却後のETH簿価：簿価(ETH JPY)
                    result.BokaEth = result.BokaEth - sellBoka;
                    //ETH売却後のETH残数量はEthZanを参照すればいいよ
                    //やっぱり計算するよ(EthZanは動きがないときに0になっていて使いにくいので)
                    result.GrossZanEth = result.GrossZanEth + ti.Eth + ti.EthCom;
                    if (ti.Kind == "売り") {//売りならBTC簿価も更新する
                        //ETH売却後のBTCの簿価の増分:BTC×JPYBTC
                        result.BokaBtc = result.BokaBtc + incomeInJpy;
                        //ETH売却後のBTCの残数量
                        result.GrossZanBtc = result.GrossZanBtc + ti.Btc;
                    }
                }
                else if ((ti.CurrPair == "BCH/BTC" && ti.Kind == "買い") || (ti.CurrPair == "BCH" && ti.Kind == "預入")) { //3-1.BCHをBTCで買う場合 
                    //BCHの簿価(円貨)：BTCに-1をかけたものにJPYBTCをかけてBCHの簿価に足す
                    result.BokaBch = result.BokaBch + (-ti.Btc.Btc2Jpy(ti.Time));
                    //BCHの残数量はBchZanを参照すればいいから無いよ
                    //やっぱり計算するよ(BchZanは動きがないときに0になっていて使いにくいので)
                    result.GrossZanBch = result.GrossZanBch + ti.Bch + ti.BchCom;
                    //BTCの簿価
                    if (ti.Kind == "買い") {//買いならBTC簿価も更新する
                        var sellBoka = result.BokaBtc * result.BtcSellRatio();
                        result.BokaBtc = result.BokaBtc - sellBoka;
                        //BTCの売却益
                        //result.PlBtc2Jpy = -ti.Btc * result.BfPrices.JPY_BTC - sellBoka;
                        result.PlBtc2Jpy = -ti.Btc.Btc2Jpy(ti.Time) - sellBoka;
                        //BTCの売却後残数量
                        result.GrossZanBtc = result.GrossZanBtc + ti.Btc;
                    }
                }
                else if (ti.CurrPair == "BCH/BTC" && (ti.Kind == "売り" || ti.Kind == "外部送付")) {//2-2.BCHをBTCに売る場合
                    //BCHの売却分の簿価：簿価(BCH JPY)×(-EHT-手数料)/(残高数量BCH+(-BCH-手数料(BCH)))×JPYBTC
                    var sellBoka = result.BokaBch * result.BchSellRatio();
                    //BCHの売却にかかる利益:BTC×JPYBTC-BCH売却分の簿価
                    var incomeInJpy = -(ti.Bch - ti.BchCom).Bch2Jpy(ti.Time); //.Btc.Btc2Jpy(ti.Time);
                    result.PlBch2Btc = incomeInJpy - sellBoka;
                    //BCH売却後のBCH簿価：簿価(BCH JPY)
                    result.BokaBch = result.BokaBch - sellBoka;
                    //BCH売却後のBCH残数量はBchZanを参照すればいいよ
                    //やっぱり計算するよ(BchZanは動きがないときに0になっていて使いにくいので)
                    result.GrossZanBch = result.GrossZanBch + ti.Bch + ti.BchCom;
                    if (ti.Kind == "売り") {//売りならBTC簿価も更新する
                        //BCH売却後のBTCの簿価の増分:BTC×JPYBTC
                        result.BokaBtc = result.BokaBtc + incomeInJpy;
                        //BCH売却後のBTCの残数量
                        result.GrossZanBtc = result.GrossZanBtc + ti.Btc;
                    }
                }
                else if (ti.CurrPair == "JPY" && ti.Kind == "入金") {
                    result.GrossZanJpy = result.GrossZanJpy + ti.Jpy;
                }
                else if (ti.CurrPair == "JPY" && ti.Kind == "出金") {
                    result.GrossZanJpy = result.GrossZanJpy + ti.Jpy;
                }
                else {
                    Console.WriteLine("{0} {1}", ti.Kind, ti.Time);
                }
                tradeResults.Add(result);
                lastResult = result;
            });
            return tradeResults;//リストを返すか保存するかどっちかにする
        }
    }
}

