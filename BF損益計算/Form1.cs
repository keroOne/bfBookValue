using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace BF損益計算
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e) {
            var u = new Utils();
            var l = await u.GetOlhcAsync("bitflyer/ethbtc");
            Console.WriteLine(l.ToString());
        }

        private async void btnSaveTo_ClickAsync(object sender, EventArgs e) {
            if (SD1.ShowDialog() == DialogResult.OK) {
                btnSaveTo.Enabled = false;
                var u = new Utils();
                await u.SavePriceHistoriesToFileAsync(SD1.FileName).ContinueWith(_ => {
                    MessageBox.Show("Done");
                    btnSaveTo.Enabled = true;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        List<TradeItem> Trades = null;
        private void button2_Click(object sender, EventArgs e) {
            if (OD1.ShowDialog() == DialogResult.OK) {//取引ファイル
                var u = new Utils();
                Trades = u.LoadTradeHistoriesFromFile(OD1.FileName).OrderBy(t => t.Time).ToList();
            }
        }
        List<BfPrices> PriceHistory;
        private void button3_Click(object sender, EventArgs e) {
            if (OD2.ShowDialog()== DialogResult.OK){//日足ファイル
                var u = new Utils();
                PriceHistory = u.LoadPriceHistoriesFromFile(OD2.FileName);
            }
        }

        private void btnCalc_Click(object sender, EventArgs e) {
            var u = new Utils();
            //Trades = u.LoadTradeHistoriesFromFile(@"C:\Users\xxxx\Desktop\BF利益額計算\TradeHistory_20180113.csv").OrderBy(t => t.Time).ToList();
            //PriceHistory = u.LoadPriceHistoriesFromFile(@"C:\Users\xxxx\Desktop\BF利益額計算\PriceHistory_20180113.csv");
            if(Trades==null || PriceHistory == null) {
                MessageBox.Show("価格と取引履歴の両方をロードしてから実行してください");
                return;
            }
            CurrencyConverter.History = PriceHistory;
            if (SD2.ShowDialog() == DialogResult.OK) {//損益履歴を保存する
                var c = new Calcurator();
                var result = c.Calc(Trades, PriceHistory);
                u.SaveTradeResultToFile(result, SD2.FileName);
                MessageBox.Show("Done");
            }
        }
    }
}
