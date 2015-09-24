using CsQuery;
using Excel;
using System;
using System.Collections.Generic;
using System.IO;
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
        private string listUrl;
        private List<string> itemUrls;
        public int pageNum;
        public int itemNum;

        public PChome(string id)
        {
            this.cc = new CookieContainer();
            this.spwc = new SpWebClient(cc);
            this.spwc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.72 Safari/537.36");
            this.id = id;
            this.listUrl = String.Format("http://www.pcstore.com.tw/{0}/HM/search.htm", id);
            this.cc.Add(new Cookie("pagecountBycookie", "40") { Domain = ".www.pcstore.com.tw" });
            this.cc.Add(new Cookie("brwsckidsn", "KNOWLET_MAGIC") { Domain = ".pcstore.com.tw" });
            this.itemUrls = new List<string>();
        }

        public bool Init()
        {
            Regex regPageNum = new Regex(@"共 ([0-9]+) 頁");
            CQ dom = spwc.DownloadString(listUrl);
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

        public List<string> getItemUrlsList()
        {
            for (int i = 1; i <= this.pageNum; ++i)
            {
                CQ dom = spwc.DownloadString(this.listUrl + "?st_sort=2&s_page=" + i.ToString());
                CQ proHrefs = dom.Select(".list_proName>a");
                proHrefs.Each((idx, a) => {
                    this.itemUrls.Add(a["href"]);
                });
                Console.WriteLine("page " + i + "; items: " + proHrefs.Length);
            }
            return itemUrls;
        }
        
        private string findClassId(string class1, string class2)
        {
            foreach (var worksheet in Workbook.Worksheets(Environment.CurrentDirectory + "\\data.xlsx"))
            {
                foreach (var row in worksheet.Rows)
                {
                    if (row.Cells[0].Text == class1)
                    {
                        foreach (var cell in row.Cells)
                        {
                            if (cell.Text == row.Cells[5].Text) return ",";
                            if (cell.Text == null) continue;
                            if (cell.Text== class2)
                            {
                                return row.Cells[6].Text + "," + row.Cells[8].Text;
                            }
                        }
                    }
                }
            }
            return ",";
        }

        public string getSinglePage(string url)
        {
            CQ dom = spwc.DownloadString(url);
            string title = dom.Select("h1").Text().Trim();
            string description = dom.Select(".sinfo").Text().Trim().Replace("\"", "\"\""); ;
            int preferPrice = Int32.Parse(dom.Select("td>b+span").First().Text());
            string orgPriceStr = dom.Select(".t13t").First().Text().Replace(Convert.ToChar(36), Convert.ToChar(32)).Trim();
            int orgPrice = orgPriceStr == "" ? preferPrice : Int32.Parse(orgPriceStr);
            string content = dom.Select("tr[style^='FONT']").First().Text().Trim().Replace("\"", "\"\""); ;
            if (content.Length > 500) content = content.Substring(0, 500);
            string[] contentPic = new string[3];
            dom.Select("tr[style^='FONT'] img").Each((i,e) => {
                if (i < 3)
                    contentPic[i] = e["src"];
            });
            string pic = dom.Select("img[name='b_img_t']")[0]["src"];
            string path = Regex.Replace(dom.Select(".topbar_bg+tr>td[height]").Text(), @"\s+", String.Empty);
            string[] classes = path.Split('>');
            return String.Format("{0},\"{1}\",\"{2}\",{3},{4},\"{5}\",{6} ,{7} ,,,,,{8}", path, title, description, preferPrice, orgPrice,
                content, string.Join(" ,", contentPic), pic, findClassId(classes[2].Trim(), classes[4].Trim())) + Environment.NewLine;
        }


    }
}
