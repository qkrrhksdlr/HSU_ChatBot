using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HSUbot.Dialogs
{
    public class LoginDialog : CancelAndHelpDialog
    {
        public static bool LoginState = false;
        public LoginDialog() : base(nameof(LoginDialog))
        {
            AddDialog(new PersonalTimeTableDialog());
            AddDialog(new ReadingRoomDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
               loginCardAttachAsync,
               loginCheckAsync
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        public async Task<DialogTurnResult> loginCardAttachAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            CardGenerator cardGenerator = new CardGenerator();
            await cardGenerator.AttachLoginCardAsync(stepContext.Context, cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        public async Task<DialogTurnResult> loginCheckAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var loginResult = (JObject)stepContext.Context.Activity?.Value;
            var id = loginResult["userId"].ToString();
            var pass = loginResult["userPass"].ToString();

            var success = LoginRequest.Login(id, pass);

            if (success)
            {
                await stepContext.Context.SendActivityAsync($"로그인 성공! {LoginRequest.UserName}님 안녕하세요!");
                if ((string)stepContext.Options == "timeTable")
                    await stepContext.BeginDialogAsync(nameof(PersonalTimeTableDialog), cancellationToken);
                else if ((string)stepContext.Options == "readingRoom")
                    await stepContext.BeginDialogAsync(nameof(ReadingRoomDialog), cancellationToken);

                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                LoginRequest.Logout();
                await stepContext.Context.SendActivityAsync("로그인 실패! 다시 입력해 주세요");
                return await stepContext.BeginDialogAsync(nameof(LoginDialog), cancellationToken);
            }
        }
            
    }
}
