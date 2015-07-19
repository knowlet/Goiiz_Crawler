using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Goiiz_Crawler
{
    class PChome
    {
        private CookieContainer cc;
        private SpWebClient spwc;

        public PChome()
        {
            this.cc = new CookieContainer();
            this.spwc = new SpWebClient(cc);
            this.spwc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.72 Safari/537.36");
             CQ dom = spwc.DownloadString("http://www.pcstore.com.tw/linewon/M20463857.htm");
            //var dom = CQ.CreateFromUrl("http://www.pcstore.com.tw/linewon/M20463857.htm");
            string title = dom.Select("h1").Text();
            Console.WriteLine(title);
        }
    }
}
