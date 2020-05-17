using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HSUbot.Dialogs
{
    public class PersonalTimeTableDialog : CancelAndHelpDialog
    {
        public PersonalTimeTableDialog() : base(nameof(PersonalTimeTableDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] {
                TimeTableViewAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> TimeTableViewAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (LoginRequest.LoginInfoFlag)
            {
                var dd = (HttpWebRequest)WebRequest.Create("https://info.hansung.ac.kr/fuz/sugang/dae_sigan_main_data.jsp");
                dd.Method = "post";
                string h = $"as_hakbun={LoginRequest.UserId}";
                dd.ContentLength = h.Length;
                dd.ContentType = "application/x-www-urlencoded";
                TextWriter wd = (TextWriter)new StreamWriter(dd.GetRequestStream());
                wd.Write(h);
                wd.Close();
                dd.CookieContainer = LoginRequest.InfoCookie;

                var ddf = (HttpWebResponse)dd.GetResponse();
                using (var r = new StreamReader(ddf.GetResponseStream(), Encoding.GetEncoding("euc-kr"), true))
                {
                    var ee = r.ReadToEnd();
                    var siganpyo =  TimeTableRemake(ee);
                    var cardGenerator = new CardGenerator();
                    await cardGenerator.AttachTimeTableCardAsync(stepContext.Context, siganpyo, cancellationToken);
                }
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(LoginDialog), "timeTable", cancellationToken);
            }
        }


        private List<TimeTable> TimeTableRemake(string oldTimeTable)
        {
            var jArray = JArray.Parse(oldTimeTable);

            var result = new List<TimeTable>();
                        
            foreach(var sigan in jArray)
            {
                var timeResult = getTime(sigan["start"].ToString(), sigan["end"].ToString());
                timeResult.StartDateTime = DateTime.Parse(sigan["start"].ToString());
                timeResult.EndDateTime = DateTime.Parse(sigan["end"].ToString());
                var title = sigan["title"].ToString().Split("\n");
                timeResult.Subject = title[0];
                timeResult.Professor = title[1];
                timeResult.Room = title[2];

                result.Add(timeResult);
            }
            result = SortTimeTable(result);

            return result;
        }

        private TimeTable getTime(string start, string end)
        {
            var timeTable = new TimeTable();
            var startDayAndTime = start.Split(' ');
            var startDay = startDayAndTime[0].Split('-');
            var startTime = startDayAndTime[2].Substring(0,5);
            var endT = end.Split(' ');
            DateTime date = new DateTime(int.Parse(startDay[0]), int.Parse(startDay[1]), int.Parse(startDay[2]));
            
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    timeTable.Day = "월";
                    break;
                case DayOfWeek.Tuesday:
                    timeTable.Day = "화";
                    break;
                case DayOfWeek.Wednesday:
                    timeTable.Day = "수";
                    break;
                case DayOfWeek.Thursday:
                    timeTable.Day = "목";
                    break;
                case DayOfWeek.Friday:
                    timeTable.Day = "금";
                    break;
                case DayOfWeek.Saturday:
                    timeTable.Day = "토";
                    break;
                case DayOfWeek.Sunday:
                    timeTable.Day = "일";
                    break;
            }

            return timeTable;
        }

        private List<TimeTable> SortTimeTable(List<TimeTable> oldTimeTable)
        {
            var newTimeTable = new List<TimeTable>();
            foreach(var oldTime in oldTimeTable)
            {
                if(newTimeTable.Count == 0)
                {
                    newTimeTable.Add(oldTime);
                    continue;
                }
                int i = 0;
                foreach(var newTime in newTimeTable)
                {
                    var result = DateTime.Compare(oldTime.StartDateTime, newTime.StartDateTime);
                    if (result > 0)
                        i++;
                }
                newTimeTable.Insert(i, oldTime);
            }

            return newTimeTable;
        }

    }
    
    public class TimeTable
    {
        public string Day;
        public string Subject;
        public string Professor;
        public string Room;
        public DateTime StartDateTime;
        public DateTime EndDateTime;
    }
}
