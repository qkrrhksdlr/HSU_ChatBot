using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace HSUbot
{
    public class DustInformation
    {

        private const String APIKEY = "O0iOIuNyfXzOdW%2ByvKxdCuPR1PshWXlN4X3v3W4fy0KXyJ2hJqvJl0sLuCHT6ZRfgm67jp5X2BlkWCcs8zpqFg%3D%3D";

        //API
        private const String DUSTRUTEURL = "http://openapi.airkorea.or.kr/";
        private const String WEATHERURL = "http://newsky2.kma.go.kr/";

        // 서비스 요청 URL
        private const byte DUSTSTATIONID = 0;

        public DustInformation() { }



        public static async Task<List<IMessageActivity>> GetDustInformationAsync(string dosistationname) // dustDetail.Dustservice, dustDetail.StationName  서울(종로구,광진구) 부산(광복동 초량동) -> 서울(서울미세먼지) 부산(부산미세먼지)
                                                                                                         // 2차 수정 - 여기에는 강북구 = dosistation  
        {

            string dusturl = $"{DUSTRUTEURL}openapi/services/rest/ArpltnInforInqireSvc/getMsrstnAcctoRltmMesureDnsty?stationName={dosistationname}"
                               + $"&dataTerm=daily&pageNo=1&numOfRows=10&ServiceKey={ APIKEY }&ver=1.3";

            XmlDocument xml = await DustApiReponseToXMLAsync(dusturl); //api 데이터 요청

            if (xml != null)
            {
                XmlNode root = xml.SelectNodes("response")[0].SelectNodes("body")[0].SelectNodes("items")[0].SelectNodes("item")[0];
                var pm10Value = int.Parse(root.SelectNodes("pm10Value")[0].InnerText);
                var pm25Value = int.Parse(root.SelectNodes("pm25Value")[0].InnerText);
                string pm10 = "";
                string pm25 = "";
                if (pm10Value <= 30)
                    pm10 = "좋음";
                else if (pm10Value <= 80)
                    pm10 = "보통";
                else if (pm10Value <= 150)
                    pm10 = "나쁨";
                else
                    pm10 = "심각";

                if (pm25Value <= 15)
                    pm25 = "좋음";
                else if (pm25Value <= 50)
                    pm25 = "보통";
                else if (pm25Value <= 100)
                    pm25 = "나쁨";
                else
                    pm25 = "심각";

                return new List<IMessageActivity> {
                    MessageFactory.Text($"현재 {dosistationname} 의 미세먼지 현황입니다"),
                    MessageFactory.Text($"미세먼지 : 농도-{pm10}({pm10Value})"),
                    MessageFactory.Text($"초미세먼지 : 농도-{pm25}({pm25Value})"),
                    MessageFactory.Text("정보제공 : 에어코리아")
                };
            }
        
            else
            {
                return new List<IMessageActivity> { MessageFactory.Text("정보를 불러오는데 실패하였습니다.") };
            }


            
        }


        private static async Task<XmlDocument> DustApiReponseToXMLAsync(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage reponse = await client.GetAsync(url);

            if (reponse.IsSuccessStatusCode)
            {
                XmlDocument xml = new XmlDocument();

                Stream stream = await reponse.Content.ReadAsStreamAsync();
                xml.Load(stream);

                return xml;
            }
            else
            {
                return null;
            }

        }
    }
}
