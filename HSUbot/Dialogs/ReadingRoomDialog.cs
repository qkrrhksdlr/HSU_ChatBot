using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace HSUbot.Dialogs
{
    public class ReadingRoomDialog : CancelAndHelpDialog
    {
        public ReadingRoomDialog() : base(nameof(ReadingRoomDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{
                ReadingRoomSeatAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
            
        }

        private async Task<DialogTurnResult> ReadingRoomSeatAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            if (LoginRequest.LoginHselFlag) {
                var req = (HttpWebRequest)WebRequest.Create("http://hsel.hansung.ac.kr/reading_reading_list.mir");
                req.CookieContainer = LoginRequest.HselCookie;

                var resp = (HttpWebResponse)req.GetResponse();

                using(var r = new StreamReader(resp.GetResponseStream()))
                {
                    var str = r.ReadToEnd();
                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(str);

                    var htmlNode = html.DocumentNode.Descendants("div").Where(x => x.GetAttributeValue("class", "").Equals("facility_box"));
                    var seatNumbers = new List<SeatNumber>();
                    foreach (var node in htmlNode)
                    {
                        seatNumbers.Add(SeatNumberClassfication(node));
                    }
                    var cardGenerator = new CardGenerator();
                    await cardGenerator.AttachSeatCardAsync(stepContext.Context, seatNumbers, cancellationToken);
                                                          
                }

                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(LoginDialog), "readingRoom", cancellationToken);
            }
        }

        private SeatNumber SeatNumberClassfication(HtmlNode node)
        {
            var seat = new SeatNumber();
            seat.name = node.ChildNodes[1].ChildNodes[1].InnerText;
            seat.possiblenum = node.ChildNodes[3].ChildNodes[1].ChildNodes[3].InnerText;
            seat.wholenum = node.ChildNodes[3].ChildNodes[1].ChildNodes[7].InnerText;
            
            return seat;
        }

        
    }
    public class SeatNumber
    {
        public string name;
        public string possiblenum;
        public string wholenum;
    }
}
