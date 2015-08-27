using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Goiiz_Crawler
{
    class PChome
    {
        private CookieContainer cc;
        private SpWebClient spwc;
        private string id;
        private string itemList;
        public int pageNum;
        public int itemNum;

        public PChome(string id)
        {
            this.cc = new CookieContainer();
            this.spwc = new SpWebClient(cc);
            this.spwc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.72 Safari/537.36");
            this.id = id;
            this.itemList = String.Format("http://www.pcstore.com.tw/{0}/HM/search.htm", id);
            Console.WriteLine(itemList);
            this.cc.Add(new Cookie("pagecountBycookie", "40") { Domain = ".www.pcstore.com.tw" });
        }

        public bool Init()
        {
            Regex regPageNum = new Regex(@"共 ([0-9]+) 頁");
            CQ dom = spwc.DownloadString(itemList);
            try
            {
                string pages = regPageNum.Match(dom.Select(".t11:first").Text()).Groups[1].Value;
                this.pageNum = Int32.Parse(pages);
                this.itemNum = Int32.Parse(dom.Select("#inside_content td>font>strong").Text().Trim());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool getSinglePage(string url)
        {
            CQ dom = spwc.DownloadString(url);
            string title = dom.Select("h1").Text().Trim();
            string description = dom.Select(".sinfo").Text().Trim().Trim();
            int preferPrice = Int32.Parse(dom.Select("td>b+span").First().Text());
            string orgPriceStr = dom.Select(".t13t").First().Text().Replace(Convert.ToChar(36), Convert.ToChar(32)).Trim();
            int orgPrice = orgPriceStr == "" ? preferPrice : Int32.Parse(orgPriceStr);
            string content = dom.Select("tr[style^='FONT']").First().Text().Trim();
            if (content.Length > 500) content = content.Substring(0, 500);
            string[] contentPic = new string[3];
            dom.Select("tr[style^='FONT'] img").Each((i,e) => {
                if (i < 3)
                    contentPic[i] = e["src"];
            });
            string pic = dom.Select("img[name='b_img_t']")[0]["src"];
            string path = dom.Select(".topbar_bg+tr>td[height]").Text().Trim().Replace("!\\s+!", String.Empty);
            Console.WriteLine("{0},\"{1}\",{2},{3},\"{4}\",{5} ,{6} ,{7} ,{8} ,,,,,case1,case2,{9}", title, description, preferPrice, orgPrice,
                content, contentPic[0], contentPic[1], contentPic[2], pic, path);
            return true;
        }
    }
}
