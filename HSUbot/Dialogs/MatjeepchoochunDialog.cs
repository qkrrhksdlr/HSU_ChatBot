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
    public class MatjeepchoochunDialog : CancelAndHelpDialog
    {

        MatjeepchoochunDetail matjeepchoochunDetail = new MatjeepchoochunDetail();

        public MatjeepchoochunDialog()
            : base(nameof(MatjeepchoochunDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                SecondStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            matjeepchoochunDetail = (MatjeepchoochunDetail)stepContext.Options;
            if (matjeepchoochunDetail.Category == null)
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("맛집추천!!"),
                        RetryPrompt = MessageFactory.Text("아래 항목 중에서 고르세요"),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "한식", "중식", "일식", "분식", "치킨", "피자", "순대국" })
                    }, cancellationToken);
            else
                return await stepContext.NextAsync(matjeepchoochunDetail.Category,cancellationToken);
        }

        private async Task<DialogTurnResult> SecondStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            matjeepchoochunDetail.Category = matjeepchoochunDetail.Category == null ? stepContext.Context.Activity.Text:matjeepchoochunDetail.Category;
            var msg = String.Empty;

            var attachments = new List<Attachment>();

            var reply = MessageFactory.Attachment(attachments);

            switch (matjeepchoochunDetail.Category)
            {
                case "한식":
                    reply.Attachments.Add(CardGenerator.GetYoonGaNeCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetRiceBurgerCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetWoonBongCard().ToAttachment());
                    break;
                case "중식":
                    reply.Attachments.Add(CardGenerator.GetSeunglijangCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetJoongHwaMyungGaCard().ToAttachment());
                    break;
                case "일식":
                    reply.Attachments.Add(CardGenerator.GetSushiHarooCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetSushiHyeonCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetMrDonkkasCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetStarDongCard().ToAttachment());
                    break;
                case "분식":
                    reply.Attachments.Add(CardGenerator.GetMecaDDuckCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetSinJeonCard().ToAttachment());
                    break;
                case "치킨":
                    reply.Attachments.Add(CardGenerator.GetHoChickenCard().ToAttachment());
                    break;
                case "피자":
                    reply.Attachments.Add(CardGenerator.GetPizzaBellCard().ToAttachment());
                    break;
                case "순대국":
                    reply.Attachments.Add(CardGenerator.GetGrandMamaCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetAuneCard().ToAttachment());
                    reply.Attachments.Add(CardGenerator.GetDonamgolCard().ToAttachment());
                    break;
            }

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);

        }

    }
}