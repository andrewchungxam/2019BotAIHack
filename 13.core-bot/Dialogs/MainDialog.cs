//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License.

//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Schema;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace CoreBot.AuthDialogs
//{
//    public class MainDialog : LogoutDialog
//    {
//        protected readonly ILogger Logger;

//        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
//            : base(nameof(MainDialog), configuration["ConnectionName"])
//        {
//            Logger = logger;

//            AddDialog(new OAuthPrompt(
//                nameof(OAuthPrompt),
//                new OAuthPromptSettings
//                {
//                    ConnectionName = ConnectionName,
//                    Text = "Please Sign In",
//                    Title = "Sign In",
//                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
//                }));

//            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

//            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
//            {
//                PromptStepAsync,
//                LoginStepAsync,
//                DisplayTokenPhase1Async,
//                DisplayTokenPhase2Async,
//            }));

//            // The initial child Dialog to run.
//            InitialDialogId = nameof(WaterfallDialog);
//        }

//        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
//        }

//        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            // Get the token from the previous step. Note that we could also have gotten the
//            // token directly from the prompt itself. There is an example of this in the next method.
//            var tokenResponse = (TokenResponse)stepContext.Result;
//            if (tokenResponse != null)
//            {
//                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You are now logged in."), cancellationToken);
//                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token?") }, cancellationToken);
//            }

//            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
//            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
//        }

//        private async Task<DialogTurnResult> DisplayTokenPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

//            var result = (bool)stepContext.Result;
//            if (result)
//            {
//                // Call the prompt again because we need the token. The reasons for this are:
//                // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
//                // about refreshing it. We can always just call the prompt again to get the token.
//                // 2. We never know how long it will take a user to respond. By the time the
//                // user responds the token may have expired. The user would then be prompted to login again.
//                //
//                // There is no reason to store the token locally in the bot because we can always just call
//                // the OAuth prompt to get the token or get a new token if needed.
//                return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), cancellationToken: cancellationToken);
//            }

//            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
//        }

//        private async Task<DialogTurnResult> DisplayTokenPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            var tokenResponse = (TokenResponse)stepContext.Result;
//            if (tokenResponse != null)
//            {
//                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token {tokenResponse.Token}"), cancellationToken);
//            }

//            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
//        }
//    }
//}






// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.AuthDialogs;
using CoreBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;



            //DELETE
            //AddDialog(new NewerDialog());

            AddDialog(new AuthDialog(Configuration));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new BookingDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Configuration["LuisAppId"]) || string.IsNullOrEmpty(Configuration["LuisAPIKey"]) || string.IsNullOrEmpty(Configuration["LuisAPIHostName"]))
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file."), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What can I help you with today?\nSay something like \"Book a flight from Paris to Berlin on March 22, 2020\"") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //// Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            //var bookingDetails = stepContext.Result != null
            //        ?
            //    await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken)
            //        :
            //    new BookingDetails();

            //// In this sample we only have a single Intent we are concerned with. However, typically a scenario
            //// will have multiple different Intents each corresponding to starting a different child Dialog.

            //// Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            //return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

            var luisResult = await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken);

            switch (luisResult.Intent)
            {
                case "Book_flight":
                    //We need to return flight details
                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(BookingDialog), luisResult, cancellationToken);
               
                    //return await stepContext.BeginDialogAsync(nameof(NewerDialog), luisResult, cancellationToken);
                case "AuthDialog_Intent":
                    //Type something like "Oauth card" or "Auth Dialog intent"

                    //Run the AuthBot Dialog
                    return await stepContext.BeginDialogAsync(nameof(AuthDialog), luisResult, cancellationToken);
                case "None":
                case "Cancel":
                default:
                    //Default to QnA
                    await QnAHelper.ExecuteQnAQuery(Configuration, Logger, stepContext.Context, cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(MainDialog), null);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        //private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    If the child dialog("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
        //    if (stepContext.Result != null)
        //    {
        //        var result = (BookingDetailsModel)stepContext.Result;

        //        Now we have all the booking details call the booking service.

        //         If the call to the booking service was successful tell the user.

        //        var timeProperty = new TimexProperty(result.TravelDate);
        //        var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
        //        var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
        //        await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
        //    }
        //    else
        //    {
        //        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
        //    }
        //    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        //}
    }
}
