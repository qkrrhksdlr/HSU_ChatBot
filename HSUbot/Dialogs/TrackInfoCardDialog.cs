using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Net.Http;
using HtmlAgilityPack;
using System.Xml;
using System.IO;
using HSUbot.Details;
using Microsoft.Bot.Schema;

namespace HSUbot.Dialogs
{
    public class TrackInfoCardDialog : CancelAndHelpDialog
    {
        public TrackInfoCardDialog()
            : base(nameof(TrackInfoCardDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                MajorStepAsync,
                CardStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> MajorStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("어떤 단과대학이 궁금하신가요?"),
                    RetryPrompt = MessageFactory.Text("아래 단과대 중에서 고르세요"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "공과대학", "인문예술대학", "디자인대학", "사회과학대학", })
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> CardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            TrackInfoCardDetail trackInfoCardDetail = new TrackInfoCardDetail();
            trackInfoCardDetail.Major = stepContext.Context.Activity.Text;

            string cardPath = "";
            CardGenerator cardGenerator = new CardGenerator();

            switch (trackInfoCardDetail.Major)
            {
                case "공과대학":
                    cardPath = "HSUbot.Cards.TrackInfoCards.ITGongdeaCard.json";
                    break;

                case "인문예술대학":
                    cardPath = "HSUbot.Cards.TrackInfoCards.InMoonYeSoolDaeCard.json";
                    break;

                case "디자인대학":
                    cardPath = "HSUbot.Cards.TrackInfoCards.DesignDaeCard.json";
                    break;

                case "사회과학대학":
                    cardPath = "HSUbot.Cards.TrackInfoCards.SocialScienceDaeCard.json";
                    break;
            }

            await cardGenerator.AttachAdaptiveCardAsync(stepContext.Context, cardPath, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);

        }

    }
}