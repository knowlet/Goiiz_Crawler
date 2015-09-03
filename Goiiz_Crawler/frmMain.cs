using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Goiiz_Crawler
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            Regex regPCStore = new Regex(@"^https?://www\.pcstore\.com\.tw/(\w+)");
            Regex regRakuten = new Regex(@"^https?://www\.rakuten\.com\.tw/shop/(\w+)");
            Regex regYahooMall = new Regex(@"^https?://tw\.mall\.yahoo\.com/store/(\w+)");
            pgbShow.Value = 0;
            if (regPCStore.IsMatch(url))
            {
                string id = regPCStore.Match(url).Groups[1].Value;
                PChome bot = new PChome(id);
                txtShow.AppendText("初始化 PCstore 爬蟲機器人.. ");
                if (bot.Init())
                {
                    txtShow.AppendText("初始化完成!\n");
                    txtShow.AppendText(String.Format("找到 {0} 個產品，共 {1} 頁\n", bot.itemNum, bot.pageNum));
                    List<string> urls = bot.getItemUrlsList();
                    txtShow.AppendText("物品清單下載完成，開始下載物品資料");
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + id + ".csv";
                    File.Delete(path);
                    pgbShow.Maximum = bot.itemNum;
                    pgbShow.Minimum = 0;
                    foreach (string u in urls)
                    {
                        // txtShow.AppendText(u + Environment.NewLine);
                        File.AppendAllText(path, bot.getSinglePage(regPCStore.Match(url).Value + u), Encoding.UTF8);
                        ++pgbShow.Value;
                    }
                    txtShow.AppendText("完成!");
                }
                else
                {
                    MessageBox.Show("資料獲取異常");
                }
            }
            else if (regRakuten.IsMatch(url))
            {
                string id = regRakuten.Match(url).Groups[1].Value;
                Rakuten bot = new Rakuten(id);
                txtShow.AppendText("初始化R天爬蟲機器人.. ");
                if (bot.Init())
                {
                    txtShow.AppendText("初始化完成!\n");
                    txtShow.AppendText(String.Format("找到 {0} 個產品，共 {1} 頁\n", bot.itemNum, bot.pageNum));
                    // txtShow.AppendText(bot.getSinglePage("http://www.rakuten.com.tw/shop/pcgoex/product/100000010481842/?l-id=tw_search_product_1"));
                    List<string> urls = bot.getItemUrlsList();
                    txtShow.AppendText("物品清單下載完成，開始載在物品資料");
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + id + ".csv";
                    File.Delete(path);
                    pgbShow.Maximum = Convert.ToInt32(bot.itemNum);
                    pgbShow.Minimum = 0;
                    foreach (string u in urls)
                    {
                        // txtShow.AppendText(u + Environment.NewLine);
                        File.AppendAllText(path, bot.getSinglePage("http://www.rakuten.com.tw" + u), Encoding.UTF8);
                        ++pgbShow.Value;
                    }
                    txtShow.AppendText("完成!");
                }
                else
                {
                    MessageBox.Show("資料獲取異常");
                }
            }
            else if (regYahooMall.IsMatch(url))
            {
                string id = regYahooMall.Match(url).Groups[1].Value;
                Yahoo bot = new Yahoo(id);
                txtShow.AppendText("初始化YahooMall爬蟲機器人.. ");
                if (bot.Init())
                {
                    txtShow.AppendText("初始化完成!\n");
                    txtShow.AppendText(String.Format("找到 {0} 個產品，共 {1} 頁\n", bot.itemNum, bot.pageNum));
                    txtShow.AppendText(bot.getSinglePage("https://tw.mall.yahoo.com/item/%E7%91%B0%E7%8F%80%E7%BF%A0-2015-%E5%8E%9F%E5%BB%A0%E8%AD%B7%E6%89%8B%E9%9C%9C%E4%B8%89%E5%85%A5%E7%A6%AE%E7%9B%92-p034075665212"));
                    /*List<string> urls = bot.getItemUrlsList();
                    txtShow.AppendText("物品清單下載完成，開始載在物品資料");
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + id + ".csv";
                    File.Delete(path);
                    pgbShow.Maximum = Convert.ToInt32(bot.itemNum);
                    pgbShow.Minimum = 0;
                    foreach (string u in urls)
                    {
                        // txtShow.AppendText(u + Environment.NewLine);
                        File.AppendAllText(path, bot.getSinglePage("http://www.rakuten.com.tw" + u), Encoding.UTF8);
                        ++pgbShow.Value;
                    }
                    txtShow.AppendText("完成!");*/
                }
                else
                {
                    MessageBox.Show("資料獲取異常");
                }
            }
            else
            {
                MessageBox.Show("請確認輸入的網址是否有效！");
            }
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnStart_Click(this, new EventArgs());
            }
        }
    }
}
