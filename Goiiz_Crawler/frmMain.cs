using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Goiiz_Crawler
{
    public partial class frmMain : Form
    {
        string PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\Goiiz\\";
        Regex regPCStore = new Regex(@"^https?://www\.pcstore\.com\.tw/(\w+)");
        Regex regRakuten = new Regex(@"^https?://www\.rakuten\.com\.tw/shop/([^\/]+)");
        Regex regYahooMall = new Regex(@"^https?://tw\.mall\.yahoo\.com/store/(\w+)");
        Random rand = new Random();

        public frmMain()
        {
            InitializeComponent();
            Directory.CreateDirectory(PATH);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (bgwk.IsBusy)
            {
                MessageBox.Show("執行中請稍後");
            }
            else
            {
                try
                {
                    bgwk.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnStart_Click(this, new EventArgs());
            }
        }

        private delegate void UIAppendText(string str);
        private void log(string str)
        {
            if (txtShow.InvokeRequired)
            {
                UIAppendText u = new UIAppendText(log);
                this.Invoke(u, new object[] { str });
            }
            else
            {
                txtShow.AppendText(str + Environment.NewLine);
            }
        }

        private delegate void UISetMax(int max);
        private void SetMaximum(int max)
        {
            if (pgbShow.InvokeRequired)
            {
                UISetMax u = new UISetMax(SetMaximum);
                this.Invoke(u, new object[] { max });
            }
            else
            {
                pgbShow.Maximum = max;
            }
        }

        private void bgwk_DoWork(object sender, DoWorkEventArgs e)
        {
            string url = txtUrl.Text;
            List<string> urls = new List<string>();
            pgbShow.Value = 0;
            if (regPCStore.IsMatch(url))
            {
                string id = regPCStore.Match(url).Groups[1].Value;
                PChome bot = new PChome(id);
                log("初始化 PCstore 爬蟲機器人.. ");
                if (bot.Init())
                {
                    log("初始化完成!");
                    log(String.Format("找到 {0} 個產品，共 {1} 頁", bot.itemNum, bot.pageNum));
                    log("下載物品清單中，請耐心等候");
                    for (int i = 1; i <= bot.pageNum; ++i)
                    {
                        bot.getItemUrlsList(i, urls);
                        Thread.Sleep(900 + rand.Next() % 300);
                    }
                    log("物品清單下載完成，開始取得物品資料");
                    string path = PATH + id + ".csv";
                    File.Delete(path);
                    SetMaximum(bot.itemNum);
                    foreach (string u in urls)
                    {
                        File.AppendAllText(path, bot.getSinglePage(regPCStore.Match(url).Value + u), Encoding.UTF8);
                        Thread.Sleep(300 + rand.Next() % 100);
                        if (pgbShow.Value < pgbShow.Maximum) bgwk.ReportProgress(pgbShow.Value + 1);
                        if (pgbShow.Value > 1000 && pgbShow.Value % 1000 == 0)
                        {
                            log("休息一下，還有 " + (bot.itemNum - pgbShow.Value).ToString() + "筆資料.. ");
                            Thread.Sleep(5000 + rand.Next() % 5000);
                            log("繼續下載");
                        }
                    }
                    log("店家 " + id + " 資料下載完成!" + Environment.NewLine);
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
                log("初始化 Rakuten 爬蟲機器人.. ");
                if (bot.Init())
                {
                    log("初始化完成!");
                    log(String.Format("找到 {0} 個產品，共 {1} 頁", bot.itemNum, bot.pageNum));
                    log("下載物品清單中，請耐心等候");
                    for (int i = 1; i <= bot.pageNum; ++i)
                    {
                        bot.getItemUrlsList(i, urls);
                        Thread.Sleep(2900 + rand.Next() % 300);
                    }
                    log("物品清單下載完成，開始取得物品資料");
                    string path = PATH + id + ".csv";
                    File.Delete(path);
                    SetMaximum(Convert.ToInt32(bot.itemNum));
                    foreach (string u in urls)
                    {
                        File.AppendAllText(path, bot.getSinglePage("http://www.rakuten.com.tw" + u), Encoding.UTF8);
                        Thread.Sleep(300 + rand.Next() % 100);
                        if (pgbShow.Value < pgbShow.Maximum) bgwk.ReportProgress(pgbShow.Value + 1);
                        if (pgbShow.Value > 1000 && pgbShow.Value % 1000 == 0)
                        {
                            log("休息一下，還有 " + (bot.itemNum - pgbShow.Value).ToString() + "筆資料.. ");
                            Thread.Sleep(5000 + rand.Next() % 5000);
                            log("繼續下載");
                        }
                    }
                    log("店家 " + id + " 資料下載完成!" + Environment.NewLine);
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
                log("初始化 YahooMall 爬蟲機器人.. ");
                if (bot.Init())
                {
                    log("初始化完成!");
                    log(String.Format("找到 {0} 個產品", bot.itemNum));
                    log("下載物品清單中，請耐心等候");
                    for (int i = 0; i <= bot.itemNum; i += 48)
                    {
                        if (!bot.getItemUrlsList(i, urls)) break;
                        Thread.Sleep(900 + rand.Next() % 300);
                    }
                    log("物品清單下載完成，開始取得物品資料");
                    string path = PATH + id + ".csv";
                    File.Delete(path);
                    SetMaximum(Convert.ToInt32(bot.itemNum));
                    foreach (string u in urls)
                    {
                        File.AppendAllText(path, bot.getSinglePage(u), Encoding.UTF8);
                        Thread.Sleep(300 + rand.Next() % 100);
                        if (pgbShow.Value < pgbShow.Maximum) bgwk.ReportProgress(pgbShow.Value + 1);
                        if (pgbShow.Value > 1000 && pgbShow.Value % 1000 == 0)
                        {
                            log("休息一下，還有 " + (bot.itemNum - pgbShow.Value).ToString() + "筆資料.. ");
                            Thread.Sleep(5000 + rand.Next() % 5000);
                            log("繼續下載");
                        }
                    }
                    log("店家 " + id + " 資料下載完成!" + Environment.NewLine);
                }
                else
                {
                    MessageBox.Show("資料獲取異常");
                }
            }
            else
            {
                MessageBox.Show("請確認輸入的網址是否位於商家首頁！");
            }
        }

        private void bgwk_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pgbShow.Value = e.ProgressPercentage;
        }

        private void bgwk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}
