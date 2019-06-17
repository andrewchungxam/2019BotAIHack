//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace CoreBot.APIDialog
//{
//    public class APIDialog
//    {
//    }
//}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace CoreBot.AuthDialogs
//{
//    public class AuthDialog
//    {
//    }
//}

//Copyright(c) Microsoft Corporation.All rights reserved.

//Licensed under the MIT License.

//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Schema;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace CoreBot.AuthDialogs
//{
//    public class AuthDialog : LogoutDialog
//    {
//        protected readonly ILogger Logger;

//        public AuthDialog(IConfiguration configuration, ILogger<AuthDialog> logger)
//            : base(nameof(AuthDialog), configuration["ConnectionName"])

//        public AuthDialog(IConfiguration configuration)
//            : base(nameof(AuthDialog), configuration["ConnectionName"])
//        {
//            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
//            {
//                TestPromptStepAsync,
//            }));

//            The initial child Dialog to run.
//            InitialDialogId = nameof(WaterfallDialog);
//        }

//        private async Task<DialogTurnResult> TestPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Just the first step test."), cancellationToken);
//            return await stepContext.NextAsync(null, cancellationToken);
//        }
//    }
//}



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace CoreBot.AuthDialogs
//{
//    public class AuthDialog
//    {
//    }
//}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.APIHelperModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CoreBot.AuthDialogs
{
    public class APIDialog : LogoutDialog
    {
        //protected readonly ILogger Logger;

        //public AuthDialog(IConfiguration configuration, ILogger<AuthDialog> logger)
        //    : base(nameof(AuthDialog), configuration["ConnectionName"])

        public APIDialog(IConfiguration configuration)
            : base(nameof(APIDialog), configuration["ConnectionName"])
        {
            //Logger = logger;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                //TestPromptStepAsync,
                PromptStepAsync,
                LoginStepAsync,
                DisplayTokenPhase1Async,
                DisplayTokenPhase2Async,
                UnformattedJSONSalesforce,
                FormattedJSONSalesforce,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        //private async Task<DialogTurnResult> TestPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Just the first step test."), cancellationToken);
        //    return await stepContext.NextAsync(null, cancellationToken);
        //}

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You are now logged in."), cancellationToken);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token?") }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

            var result = (bool)stepContext.Result;
            if (result)
            {
                // Call the prompt again because we need the token. The reasons for this are:
                // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
                // about refreshing it. We can always just call the prompt again to get the token.
                // 2. We never know how long it will take a user to respond. By the time the
                // user responds the token may have expired. The user would then be prompted to login again.
                //
                // There is no reason to store the token locally in the bot because we can always just call
                // the OAuth prompt to get the token or get a new token if needed.
                return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token {tokenResponse.Token}"), cancellationToken);
            }



            //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            return await stepContext.NextAsync(tokenResponse, cancellationToken: cancellationToken);

        }

        private async Task<DialogTurnResult> UnformattedJSONSalesforce(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string returnedAPIString = "";    

            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                //                var returnedAPIString = await this.SalesforceAPIGetAccountInfo("test");
                returnedAPIString = await this.SalesforceAPIGetAccountInfo(tokenResponse.Token);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{returnedAPIString}"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Issue returning JSON from salesforce"), cancellationToken);
            }

            return await stepContext.NextAsync(returnedAPIString, cancellationToken: cancellationToken);

            //var tokenResponse = (TokenResponse)stepContext.Result;
            //if (tokenResponse != null)
            //{
            //    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token {tokenResponse.Token}"), cancellationToken);
            //}

            //var returnedAPIString = await this.SalesforceAPIGetAccountInfo("test");

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{returnedAPIString}"), cancellationToken);

            //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FormattedJSONSalesforce(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var jsonString = (string)stepContext.Result;

            var apiHelperModel = ApiHelperModel.FromJson(jsonString);
            
            await stepContext.Context.SendActivityAsync(MessageFactory.Text
            (
                $"Account name: {apiHelperModel.Name} \n\n " +
                $"Address: {apiHelperModel.BillingStreet} \n\n" +
                $" {apiHelperModel.BillingCity},  {apiHelperModel.BillingState},  {apiHelperModel.BillingPostalCode},  {apiHelperModel.BillingCountry}"
            ), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        public async Task<string> SalesforceAPIGetAccountInfo(string tokenForCall)
        {
            //if (string.Equals(sourceLanguage, targetLanguage, StringComparison.OrdinalIgnoreCase))
            //{
            //    return text; // No translation required
            //}

            //var body = new System.Object[] { new { Text = text } };
            //var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                //var tokenForCall = "00D4P000000yP4r!ARsAQK8QGHVThqmwsvReWu3HzEMliGhjamqat28EYjUfmEg_OhvuUel9hI2_Mtp8hZc86crOD03aAcICSagr9a89cErgU.eU";

                if (client.DefaultRequestHeaders != null)
                { 
                    client.DefaultRequestHeaders.Clear();
                }
                
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenForCall);

                request.Method = HttpMethod.Get;

                var baseURL = "https://na132.salesforce.com";
                var GetAccountURL = "/services/data/v39.0/sobjects/Account/";
                var AccountId = "0014P000027LgWKQA0";

                //request.RequestUri = new Uri($"{TranslateMethodUri}/translate{UriParams}&to={targetLanguage}");
                request.RequestUri = new Uri($"{baseURL}{GetAccountURL}{AccountId}");

                //request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                //request.Headers.Add("Ocp-Apim-Subscription-Key", _translatorTextKey);
                //request.Headers.Add("Bearer Token", tokenForCall);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
                
                //var result = JsonConvert.DeserializeObject<List<TextTranslatorResponse>>(responseBody);
                //return result.First().Translations.First().Text;

                //var apiHelperModel = ApiHelperModel.FromJson(responseBody);

            }
        }
    }
}



//public async Task<string> Translate(string sourceLanguage, string targetLanguage, string text)
//{
//    if (string.Equals(sourceLanguage, targetLanguage, StringComparison.OrdinalIgnoreCase))
//    {
//        return text; // No translation required
//    }

//    var body = new System.Object[] { new { Text = text } };
//    var requestBody = JsonConvert.SerializeObject(body);

//    using (var client = new HttpClient())
//    using (var request = new HttpRequestMessage())
//    {
//        request.Method = HttpMethod.Post;
//        request.RequestUri = new Uri($"{TranslateMethodUri}/translate{UriParams}&to={targetLanguage}");
//        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
//        request.Headers.Add("Ocp-Apim-Subscription-Key", _translatorTextKey);

//        var response = await client.SendAsync(request);
//        response.EnsureSuccessStatusCode();
//        var responseBody = await response.Content.ReadAsStringAsync();
//        var result = JsonConvert.DeserializeObject<List<TextTranslatorResponse>>(responseBody);
//        return result.First().Translations.First().Text;
//    }
//}

