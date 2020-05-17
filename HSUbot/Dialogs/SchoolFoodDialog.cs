using HtmlAgilityPack;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HSUbot.Dialogs
{
    public class SchoolFoodDialog : CancelAndHelpDialog
    {
        public SchoolFoodDialog() : base(nameof(SchoolFoodDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{
                FoodTableViewAsync
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FoodTableViewAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var client = new HttpClient();
            var resp = await client.GetAsync("https://www.hansung.ac.kr/web/www/life_03_01_t2");

            var result = await resp.Content.ReadAsStringAsync();
            var html = new HtmlDocument();
            html.LoadHtml(result);

            var htmlNode = html.DocumentNode.Descendants("table").Where(x => x.GetAttributeValue("class", "").Equals("table-b table-b-menu")).First().ChildNodes[7];

            var htmlLunch = htmlNode.ChildNodes[1];
            var htmlDinner = htmlNode.ChildNodes[2];
            var cardGenerator = new CardGenerator();

            var lunch = TodayFood(htmlLunch);
            var dinner = TodayFood(htmlDinner);

            await cardGenerator.AttachFoodCardAsync(stepContext.Context, lunch, dinner, cancellationToken);       

            return await stepContext.EndDialogAsync(cancellationToken);
        }

        public string TodayFood(HtmlNode htmlNode)
        {
            var index = 3 + (int)DateTime.Now.DayOfWeek;

            var result = htmlNode.ChildNodes[index].InnerHtml.Split("<br>");

            string str = "";
            foreach(var a in result)
            {
                str += (a + " ");
            }

            return str;
        }
    }
}
