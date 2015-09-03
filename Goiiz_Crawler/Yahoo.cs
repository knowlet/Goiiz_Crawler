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
    class Yahoo
    {
        private CookieContainer cc;
        private SpWebClient spwc;
        private string id;
        private string listUrl;
        private List<string> itemUrls;
        public int pageNum;
        public double itemNum;

        public Yahoo(string id)
        {
            this.cc = new CookieContainer();
            this.spwc = new SpWebClient(cc);
            this.spwc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.72 Safari/537.36");
            this.id = id;
            this.listUrl = String.Format("https://tw.mall.yahoo.com/search?m=list&sid={0}&s=-createtime&view=both", id);
            this.itemUrls = new List<string>();
        }

        public bool Init()
        {
            CQ dom = spwc.DownloadString(listUrl, Encoding.UTF8);
            try
            {
                string item = dom.Select("p>strong").Text();
                this.itemNum = Double.Parse(item);
                this.pageNum = Convert.ToInt32(Math.Ceiling(itemNum / 48));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<string> getItemUrlsList()
        {
            for (int i = 0; i <= this.pageNum; i += 48)
            {
                CQ dom = spwc.DownloadString(this.listUrl + "&b=30000" + i.ToString(), Encoding.UTF8);
                // 超過三萬件商品可能會有空白資料的情況
                if (i > 29952 && dom.Select("p>strong").Text().Trim() == "0") break;
                CQ proHrefs = dom.Select("td.title>a");
                proHrefs.Each((idx, a) => {
                    this.itemUrls.Add(a["href"]);
                });
                Console.WriteLine("page " + i + "; items: " + proHrefs.Length);
            }
            return itemUrls;
        }

        public string getSinglePage(string url)
        {
            CQ dom = spwc.DownloadString(url, Encoding.UTF8);
            string title = dom.Select("h1>span:first").Text().Trim();
            string description = dom.Select("h1+p").Text().Trim();
            int preferPrice = Int32.Parse(dom.Select("span.price:first").Text().Replace("元", String.Empty));
            string orgPriceStr = "";
            CQ tmp = dom.Select("del");
            if (tmp != null) orgPriceStr = tmp.Text().Replace("元", String.Empty);
            else
            {
                tmp = dom.Select(".has_promo_price");
                if (tmp != null) orgPriceStr = tmp.Text().Replace("元", String.Empty);
            }
            int orgPrice = orgPriceStr != "" ? Int32.Parse(orgPriceStr) : preferPrice;
            string content = dom.Select("#ypsudes div.ins").Text().Trim().Replace("\"", "\"\"");
            if (content.Length > 500) content = content.Substring(0, 500);
            string[] contentPic = new string[3];
            dom.Select("#ypsid img").Each((i, e) => {
                if (i < 3)
                    contentPic[i] = e["src"];
            });
            string pic = contentPic[0];
            string path = dom.Select(".on>a").Text();
            dom.Select(".showsub").Each((i, e) => {
                if (i > 0)
                    path += ">" + System.Net.WebUtility.HtmlDecode(e.PreviousElementSibling.ChildNodes[0].InnerText);
            });
            path += ">" + dom.Select(".slectit").Text().Trim();
            return String.Format("\"{0}\",\"{1}\",{2},{3},\"{4}\",{5} ,{6} ,,,,,case1,case2,{7}", title, description, preferPrice, orgPrice,
                content, string.Join(" ,", contentPic), pic, path) + Environment.NewLine;
        }

    }
}
