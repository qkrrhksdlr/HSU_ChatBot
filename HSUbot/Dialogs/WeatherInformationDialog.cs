using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using HSUbot.Details;
using System;
using System.Net.Mail;
using Microsoft.Bot.Schema;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace HSUbot.Dialogs
{
    public class WeatherInformationDialog : CancelAndHelpDialog


    {
        DustDetails dustDetail = new DustDetails();


        public WeatherInformationDialog() : base(nameof(WeatherInformationDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
             
               //serviceStepAsync1,
          
              FirstStepAsync,
             DustInfoStepAsync
            }));

            
            InitialDialogId = nameof(WaterfallDialog);
        }



        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            dustDetail = (DustDetails)stepContext.Options;
            if (dustDetail.Dosistationname == null)
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions
                    {

                        RetryPrompt = MessageFactory.Text("아래 항목 중에서 고르세요"),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "강동구", "강북구", "강서구", "관악구", "광진구",
                        "구로구", "금천구", "노원구", "도봉구", "동대문구", "동작구", "마포구", "서대문구", "서초구",
                        "성동구", "성북구", "송파구", "양천구", "영등포구", "용산구", "은평구", "종로구", "중구", "중랑구"})
                    }, cancellationToken);
            else
                return await stepContext.NextAsync(dustDetail.Dosistationname, cancellationToken);
        }

        private async Task<DialogTurnResult> DustInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Attachment(CardGenerator.AttachDustCard().ToAttachment());

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            dustDetail.Dosistationname = dustDetail.Dosistationname == null ? stepContext.Context.Activity.Text : stepContext.Result.ToString();

            var msg = await DustInformation.GetDustInformationAsync(dustDetail.Dosistationname);


            await stepContext.Context.SendActivitiesAsync(msg.ToArray(), cancellationToken);


            return await stepContext.EndDialogAsync(null, cancellationToken);

        }



       

    }




    /*   private async Task<DialogTurnResult> serviceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
       {
           var dustDetails = (DustDetails)stepContext.Options;

           dustDetails.Area = (string)stepContext.Result;

           if (dustDetails.Service == null)
           {
               return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("1.미세먼지 2. 날씨 \n 원하시는 서비스는 무엇인가요?") }, cancellationToken);
           }
           else
           {
               return await stepContext.NextAsync(dustDetails.Service, cancellationToken);
           }
       }

       private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
       {
           var dustDetails = (DustDetails)stepContext.Options;

           dustDetails.Service = (string)stepContext.Result;

           var msg = $"원하시는 서비스는 {dustDetails.Area}의  {dustDetails.Service} 가 맞습니까?";

           return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
       }

       private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
       {
           if ((bool)stepContext.Result)
           {
               var dustDetails = (DustDetails)stepContext.Options;

               return await stepContext.EndDialogAsync(dustDetails, cancellationToken);
           }
           else
           {
               return await stepContext.EndDialogAsync(null, cancellationToken);
           }
       }
       */
    /* private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
     {
         dustDetails.Dustservice = stepContext.Context.Activity.Text;

         if (dustDetails.Dustservice == "초미세먼지")
         {
             string msg = "http://cleanair.seoul.go.kr/air_city.htm?method=measure&grp1=pm25";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
             return await stepContext.EndDialogAsync(null, cancellationToken);
         }

         else if (dustDetails.Dustservice == "미세먼지")
         {
             string msg = "http://cleanair.seoul.go.kr/air_city.htm?method=measure&grp1=pm10";
             await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
           return await stepContext.EndDialogAsync(null, cancellationToken);
         }

         else if (dustDetails.Dustservice == "날씨")
         {
             // string msg = "http://www.weather.go.kr/weather/observation/currentweather.jsp";
             string msg = "http://www.weather.go.kr/weather/main.jsp";
             await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
           return await stepContext.EndDialogAsync(null, cancellationToken);
         }

         return await stepContext.EndDialogAsync(null, cancellationToken);
     }
     */
}
