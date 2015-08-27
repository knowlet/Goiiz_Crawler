using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public static void wait(int sec)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            Regex regPCStore = new Regex(@"^https?://www\.pcstore\.com\.tw/(\w+)");
            Regex regRakuten = new Regex(@"^https?://www\.rakuten\.com\.tw/shop/(\w+)");
            Regex regYahooMall = new Regex(@"^https?://tw\.mall\.yahoo\.com/store/(\w+)");
            if (regPCStore.IsMatch(url))
            {
                PChome bot = new PChome(regPCStore.Match(url).Groups[1].Value);
                txtShow.AppendText("初始化 PCstore 爬蟲機器人.. ");
                if (bot.Init())
                {
                    txtShow.AppendText("初始化完成!\n");
                    txtShow.AppendText(String.Format("找到 {0} 個產品，共 {1} 頁\n", bot.itemNum, bot.pageNum));
                    List<string> urls = bot.getItemUrlsList();
                    foreach (string u in urls)
                    {
                        txtShow.AppendText(u + Environment.NewLine);
                    }
                }
                else
                {
                    MessageBox.Show("資料獲取異常");
                }
            }
            else if (regRakuten.IsMatch(url))
            {

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
