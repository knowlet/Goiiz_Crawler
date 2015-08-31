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
                    txtShow.AppendText("物品清單下載完成，開始下在物品資料");
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + id + ".csv";
                    File.Delete(path);
                    foreach (string u in urls)
                    {
                        // txtShow.AppendText(u + Environment.NewLine);
                        File.AppendAllText(path, bot.getSinglePage(regPCStore.Match(url).Value + u), Encoding.UTF8);
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
                Rakuten bot = new Rakuten(regRakuten.Match(url).Groups[1].Value);
                txtShow.AppendText("初始化R天爬蟲機器人.. ");
                if (bot.Init())
                {
                    txtShow.AppendText("初始化完成!\n");
                    txtShow.AppendText(String.Format("找到 {0} 個產品，共 {1} 頁\n", bot.itemNum, bot.pageNum));
                    // txtShow.AppendText(bot.getSinglePage("http://www.rakuten.com.tw/shop/psmall/product/100000009124190/?l-id=tw_search_product_1"));
                    List<string> urls = bot.getItemUrlsList();
                    txtShow.AppendText("物品清單下載完成，開始下在物品資料");
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + id + ".csv";
                    File.Delete(path);
                    foreach (string u in urls)
                    {
                        // txtShow.AppendText(u + Environment.NewLine);
                        File.AppendAllText(path, bot.getSinglePage(regPCStore.Match(url).Value + u), Encoding.UTF8);
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
