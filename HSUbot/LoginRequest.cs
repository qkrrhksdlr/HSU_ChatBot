using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HSUbot
{
    public class LoginRequest
    {
        private const string INFOURI = "https://info.hansung.ac.kr/servlet/s_gong.gong_login_ssl";
        private const string HSELURI = "https://hsel.hansung.ac.kr/home_security_login_write_prss.mir";
        public static CookieContainer InfoCookie { get; private set; }
        public static bool LoginInfoFlag { get; private set; } = false;
        public static CookieContainer HselCookie { get; private set; }
        public static bool LoginHselFlag { get; private set; } = false;
        public static string UserId { get; private set; }
        public static string UserName { get; private set; }
        public static string UserPass { get; private set; }
        
        public static bool Login(string id, string pass)
        {
            var info = LoginInfoHansung(id, pass);
            var hsel = LoginHselHansung(id, pass);

            return info && hsel;
        }
        public static bool LoginInfoHansung(string id, string pass)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var req = (HttpWebRequest)WebRequest.Create(INFOURI);
            req.Method = "POST"; ;
            string s = $"id={id}&passwd={pass}";
            req.CookieContainer = new CookieContainer();
            req.ContentLength = s.Length;
            req.AllowAutoRedirect = true;
            req.MaximumAutomaticRedirections = 1;
            req.KeepAlive = true;
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            TextWriter w = (TextWriter)new StreamWriter(req.GetRequestStream());
            w.Write(s);
            w.Close();

            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.Found)
                {
                    var nameReq = (HttpWebRequest)WebRequest.Create("https://info.hansung.ac.kr/fuz/common/include/default/top.jsp");
                    nameReq.CookieContainer = req.CookieContainer;

                    var nameResp = (HttpWebResponse)nameReq.GetResponse();
                    using(var r = new StreamReader(nameResp.GetResponseStream(), Encoding.GetEncoding("euc-kr"), true))
                    {
                        var re = r.ReadToEnd();
                        var html = new HtmlDocument();
                        html.LoadHtml(re);
                        var htmlNode = html.DocumentNode.Descendants("ul").Where(x => x.GetAttributeValue("class", "").Equals("info"))?.First();
                        UserName = htmlNode.ChildNodes[1].InnerText.Split(" : ")[1];
                    }

                    var cookies = req.CookieContainer;
                    cookies.Add(response.Cookies);
                    UserId = id;
                    InfoCookie = cookies;
                    LoginInfoFlag = true;
                    return true;
                }
                else
                {
                    LoginInfoFlag = false;
                    return false;
                }
            }
            LoginInfoFlag = false;
            return false;
        }

        public static bool LoginHselHansung(string id, string pass)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var req = (HttpWebRequest)WebRequest.Create(HSELURI);
            req.Method = "POST"; 
            string s = $"home_login_id={id}&home_login_password={pass}";
            req.CookieContainer = new CookieContainer();
            req.ContentLength = s.Length;
            req.AllowAutoRedirect = true;
            req.MaximumAutomaticRedirections = 1;
            req.KeepAlive = true;
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            TextWriter w = (TextWriter)new StreamWriter(req.GetRequestStream());
            w.Write(s);
            w.Close();

            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.ResponseUri.OriginalString == "https://hsel.hansung.ac.kr/home_login_write_prss.mir")
                {
                    var cookies = req.CookieContainer;
                    UserId = id;
                    HselCookie = cookies;
                    LoginHselFlag = true;
                }

                else
                {
                    LoginHselFlag = false;
                }
            }
            return LoginHselFlag;
        }

        public static bool Logout()
        {
            InfoCookie = null;
            HselCookie = null;
            LoginHselFlag = false;
            LoginInfoFlag = false;
            UserId = null;
            UserPass = null;

            return false;
        }
    }

}
