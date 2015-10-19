using CsQuery;
using Excel;
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
        public int itemNum;

        public Yahoo(string id)
        {
            this.cc = new CookieContainer();
            this.spwc = new SpWebClient(cc);
            this.spwc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.72 Safari/537.36");
            this.id = id;
            this.listUrl = String.Format("https://tw.mall.yahoo.com/search?m=list&sid={0}&s=-createtime&view=both", id);
        }

        public bool Init()
        {
            CQ dom = spwc.DownloadString(listUrl, Encoding.UTF8);
            try
            {
                string item = dom.Select("p>strong").Text();
                this.itemNum = Int32.Parse(item);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool getItemUrlsList(int i, List<string> itemUrls)
        {
            CQ dom = spwc.DownloadString(this.listUrl + "&b=" + i.ToString(), Encoding.UTF8);
            // 超過三萬件商品可能會有空白資料的情況
            if (i > 29952 && dom.Select("p>strong").Text().Trim() == "0") return false;
            CQ proHrefs = dom.Select("td.title>a");
            proHrefs.Each((idx, a) => {
                itemUrls.Add(a["href"]);
            });
            Console.WriteLine("page " + i + "; items: " + proHrefs.Length);
            return true;
        }

        private string findClassId(string[] classes)
        {
            foreach (var worksheet in Workbook.Worksheets(Environment.CurrentDirectory + "\\data.xlsx"))
            {
                foreach (var row in worksheet.Rows)
                {
                    if (row.Cells[0].Text == classes[0])
                    {
                        foreach (var cell in row.Cells)
                        {
                            if (cell.Text == row.Cells[5].Text) return ",";
                            if (cell.Text == null) continue;
                            foreach (string class2 in classes)
                            {
                                if (cell.Text == class2)
                                {
                                    return row.Cells[6].Text + "," + row.Cells[8].Text;
                                }
                            }
                        }
                    }
                }
            }
            return ",";
        }

        public string getSinglePage(string url)
        {
            CQ dom = spwc.DownloadString(url, Encoding.UTF8);
            string title = dom.Select("h1>span:first").Text().Trim();
            string description = dom.Select("h1+p").Text().Trim();
            int preferPrice = Int32.Parse(dom.Select("span.price:first").Text().Replace("元", String.Empty));
            string orgPriceStr = "";
            CQ tmp = dom.Select("del");
            if (tmp != null) orgPriceStr = Regex.Match(tmp.Text(), "^\\d+").Value;
            else
            {
                tmp = dom.Select(".has_promo_price");
                if (tmp != null) orgPriceStr = Regex.Match(tmp.Text(), "^\\d+").Value;
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
            return string.Format("{0},\"{1}\",\"{2}\",{3},{4},{5},\"{6}\",{7} ,{8} ,,,,", path, title, description, preferPrice, orgPrice, findClassId(path.Split('>')),
                content, string.Join(" ,", contentPic), pic) + Environment.NewLine;
        }

    }
}
