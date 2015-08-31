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
    class Rakuten
    {
        private CookieContainer cc;
        private SpWebClient spwc;
        private string id;
        private string listUrl;
        private List<string> itemUrls;
        public int pageNum;
        public double itemNum;
        
        public Rakuten(string id)
        {
            this.cc = new CookieContainer();
            this.spwc = new SpWebClient(cc);
            this.spwc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.72 Safari/537.36");
            this.id = id;
            this.listUrl = String.Format("http://www.rakuten.com.tw/shop/{0}/products/", id);
            this.itemUrls = new List<string>();
        }

        public bool Init()
        {
            Regex regPageNum = new Regex(@"共 ([0-9]+) 筆結果");
            CQ dom = spwc.DownloadString(listUrl);
            try
            {
                this.itemNum = Double.Parse(regPageNum.Match(dom.Select(".b-tabs-utility").Text()).Groups[1].Value);
                this.pageNum = Int32.Parse(Math.Ceiling(itemNum / 60).ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<string> getItemUrlsList()
        {
            for (int i = 1; i <= this.pageNum; ++i)
            {
                CQ dom = spwc.DownloadString(this.listUrl + "?h=3&v=l&p=" + i.ToString());
                CQ proHrefs = dom.Select(".b-item .b-text b>a");
                proHrefs.Each((idx, a) => {
                    this.itemUrls.Add(a["href"]);
                });
                Console.WriteLine("page " + i + "; items: " + proHrefs.Length);
            }
            return itemUrls;
        }

        public string getSinglePage(string url)
        {
            CQ dom = spwc.DownloadString(url);
            string title = dom.Select("h1").Text().Trim();
            string description = dom.Select(".sinfo").Text().Trim();
            int preferPrice = Int32.Parse(dom.Select("td>b+span").First().Text());
            string orgPriceStr = dom.Select(".t13t").First().Text().Replace(Convert.ToChar(36), Convert.ToChar(32)).Trim();
            int orgPrice = orgPriceStr == "" ? preferPrice : Int32.Parse(orgPriceStr);
            string content = dom.Select("tr[style^='FONT']").First().Text().Trim();
            if (content.Length > 500) content = content.Substring(0, 500);
            string[] contentPic = new string[3];
            dom.Select("tr[style^='FONT'] img").Each((i, e) => {
                if (i < 3)
                    contentPic[i] = e["src"];
            });
            string pic = dom.Select("img[name='b_img_t']")[0]["src"];
            string path = Regex.Replace(dom.Select(".topbar_bg+tr>td[height]").Text(), @"\s+", String.Empty);
            return String.Format("\"{0}\",\"{1}\",{2},{3},\"{4}\",{5} ,{6} ,,,,,case1,case2,{7}", title, description, preferPrice, orgPrice,
                content, string.Join(" ,", contentPic), pic, path) + Environment.NewLine;
        }

    }
}
