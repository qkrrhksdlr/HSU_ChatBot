using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Net.Http;
using HtmlAgilityPack;
using System.Linq;

namespace HSUbot.Dialogs
{
    public class NoticeDialog : CancelAndHelpDialog
    {
        private readonly List<string> NOTICENAME = new List<string>
        {
            "한성공지",
            "학사공지",
            "비교과공지",
            "장학공지"
        };
        private readonly List<string> URIS = new List<string>
        {
            "https://www.hansung.ac.kr/web/www/cmty_01_01",
            "https://www.hansung.ac.kr/web/www/cmty_01_03",
            "https://www.hansung.ac.kr/web/www/1323",
            "https://www.hansung.ac.kr/web/www/552"
        };

        public NoticeDialog()
            : base(nameof(NoticeDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SelectNoticeAsync,
                NoticeViewAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectNoticeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("공지사항이 궁금하신가요?"),
                    RetryPrompt = MessageFactory.Text("아래 항목 중에서 고르세요"),
                    Choices = ChoiceFactory.ToChoices(NOTICENAME)
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> NoticeViewAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var notice = stepContext.Context.Activity.Text;
            var index = NOTICENAME.IndexOf(notice);

            var client = new HttpClient();
            var response = await client.GetAsync(URIS[index]);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var html = new HtmlDocument();
                html.LoadHtml(result);

                var htmlNode = html.DocumentNode.Descendants("table").Where(x => x.GetAttributeValue("class", "").Equals("bbs-list")).First().ChildNodes[7];

                var notices = NoticeDivider(htmlNode.ChildNodes);
                var cardGenerator = new CardGenerator();
                await cardGenerator.AttachNoticeCard(stepContext.Context, notices, notice, URIS[index], cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("공지사항을 불러오는데 실패했습니다");
                  
            }
            return await stepContext.EndDialogAsync(cancellationToken);

        }
        public List<Notice> NoticeDivider(HtmlNodeCollection tbody)
        {
            var notices = new List<Notice>();

            for(int i=1; i<14; i+=3)
            {
                var tr = tbody[i];
                var notice = new Notice();
                notice.Title = tr.ChildNodes[1].ChildNodes[1].InnerText;
                notice.Uri = tr.ChildNodes[1].ChildNodes[1].Attributes["href"].Value;
                notice.Writer = tr.ChildNodes[3].InnerText;
                notice.Date = tr.ChildNodes[4].InnerText;

                notices.Add(notice);
            }

            return notices;
        }

    }
    public class Notice
    {
        public string Title;
        public string Uri;
        public string Writer;
        public string Date;
    }
}