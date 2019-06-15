using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public static class QnAHelper
    {
        public static async Task ExecuteQnAQuery(IConfiguration configuration, ILogger logger, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var qnaMaker = new QnAMaker(new QnAMakerEndpoint
                {
                    KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                    EndpointKey = configuration["QnAAuthKey"],
                    Host = configuration["QnAEndpointHostName"]
                });

                logger.LogInformation("Calling QnA Maker");

                // The actual call to the QnA Maker service.
                var response = await qnaMaker.GetAnswersAsync(turnContext);
                if (response != null && response.Length > 0)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"QnA Exception: {e.Message} Check your QnA configuration.");
            }
        }
    }
}

