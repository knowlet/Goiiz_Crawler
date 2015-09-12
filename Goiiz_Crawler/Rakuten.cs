using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
            CQ dom = spwc.DownloadString(listUrl, Encoding.UTF8);
            try
            {
            string item = dom.Select(".b-tabs-utility").Text();
            this.itemNum = Double.Parse(regPageNum.Match(item).Groups[1].Value);
            this.pageNum = Convert.ToInt32(Math.Ceiling(itemNum / 60));
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
                CQ dom = spwc.DownloadString(this.listUrl + "?h=3&v=l&p=" + i.ToString(), Encoding.UTF8);
                CQ proHrefs = dom.Select(".b-item .b-text b>a");
                proHrefs.Each((idx, a) => {
                    this.itemUrls.Add(a["href"]);
                });
                Console.WriteLine("page " + i + "; items: " + proHrefs.Length);
                Thread.Sleep(1000);
                Console.WriteLine("Wait 1 sec.");
            }
            return itemUrls;
        }

        public string getSinglePage(string url)
        {
            CQ dom = spwc.DownloadString(url, Encoding.UTF8);
            string title = dom.Select("h1").Text().Trim();
            string description = "";    // dom.Select(".prd-description").Text().Trim();
            int preferPrice = Int32.Parse(dom.Select("span.qa-product-actualPrice").Text().Trim().Replace(",", String.Empty));
            Regex regListPrice = new Regex(@"prod_list_price': (\w+)");
            string dataLayer = dom.Select("script[type]:not([src]):first").Text();
            string orgPriceStr = regListPrice.Match(dataLayer).Groups[1].Value.Trim().Replace(",", String.Empty);
            int orgPrice = orgPriceStr == "null" ? preferPrice : Int32.Parse(orgPriceStr);
            string content = dom.Select(".prd-description").Text().Trim().Replace("\"", "\"\"");
            if (content.Length > 500) content = content.Substring(0, 500);
            string[] contentPic = new string[3];
            dom.Select(".prd-description img").Each((i, e) => {
                if (i < 3)
                    contentPic[i] = e["src"];
            });
            string[] pic = new String[5];
            CQ itemThumnails = dom.Select(".item-thumnails img");
            if (itemThumnails.Length > 0)
            {
                itemThumnails.Each((i, e) => {
                    if (i < 5)
                        pic[i] = e["data-frz-src"];
                });
            }
            else
            {
                pic[0] = dom.Select("img[itemprop]")[0]["data-frz-src"].Split('?')[0];
            }
            string path = dom.Select(".related-categories>ul").Text().Trim();
            return String.Format("\"{0}\",\"{1}\",{2},{3},\"{4}\",{5} ,{6} ,case1,case2,{7}", title, description, preferPrice, orgPrice,
                content, string.Join(" ,", contentPic), string.Join(" ,", pic), path) + Environment.NewLine;
        }

    }
}
