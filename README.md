# 2019BotAIHack

Based on the amazing work of Adam Hockemeyer.  

This project starts from the official Microsoft bot [samples](https://github.com/Microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore) and the [Microsoft AI Lab]( https://github.com/microsoft/ailab/tree/master/BuildAnIntelligentBot)

The following is an exploration of the Bot Framework 4.4.3+ - using the official Microsoft samples to create a use case for Bots.

The Bot includes Luis, QnA Maker, exploration of OAuth cards, and 3rd-party API calls.

### WORKING WITH LUIS

From the official Microsoft[samples](https://github.com/Microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore), run the project CoreBot.  

We assume you're working from Visual Studio and have already downloaded the [pre-requistes](https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-sdk-quickstart?view=azure-bot-service-4.0#prerequisites).

Run the CoreBot project; it will show an adaptive card with a large image with buttons which link to relevant websites.

By following the prompts, you can try booking a flight - try telling it you want to go from Paris to New York tomorrow (phrased however you'd like).

After you type something however, you'll notice the bot shows a warning: 
```
NOTE: LUIS is not configured. To enable all capabilities, add ‘LuisAppId’, ‘LuisAPIKey’ and ‘LuisAPIHostName’ to the appsettings.json file.
```

Currently the appsettings.json file looks like this:

```
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "LuisAppId": "",
  "LuisAPIKey": "",
  "LuisAPIHostName": ""
}
```

Open your browser and go to: www.luis.ai.  You'll need to login with your Azure credentials.  Create a project like [this](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/get-started-portal-build-app):

Go back to your www.luis.ai projects > click Build.  Click Create New Intents. Name it: "Book_flight" (copy this exactly as your code looks for this term)
Then it will prompt you to add "utterances" - I added several, I added some like: "Can I book a flight?"

Once you are done, hit the "Train" button so the utterances are tied to the Intent and then after that is done, hit "Publish" so APIs calls will hit this updated model.  (Note, if you don't hit publish, your new changes will *not* reflect from any API calls.)

Now remember, in Visual Studio in our appsettings.json file, we're looking to fill out the following:
You're looking for:
```
  "LuisAppId": "",
  "LuisAPIKey": "",
  "LuisAPIHostName": ""
```

We'll get these values from the Luis.ai portal.  After you've created a Luis project, go to Management tab.  The names in the appsettings.json file are not exactly the same as they are in the portal so look for the equivalent terms in the following chart.  Go ahead and copy these values from the portal.
```
LuisAppId (In appsettings.json) => Application Id (In Luis portal)  
LuisAPIKey (In appsettings.json) => Authoring Key (In Luis portal)  
LuisAPIHostName (In appsettings.json) => Key and Endpoints > Endpoint > only the first part of the listed url (ie. "westus.api.cognitive.microsoft.com") (In Luis portal) 
```

Now re-run your project from Visual Studio.  
Restart the conversation in your Emulator.
Interact with the bot - and try to trigger Luis to indentify the "Book_flight" intent.

- The above code can be found under branch-1-Working-With-Luis

### Working With Luis and QnA Maker

Now, we're going to add QnA Maker.

There is code from a different project in the official Microsoft samples.  We'll take that code and add it to the project we have already been working with.

At a high-level, we'll need to create/configure a QnA Bot.  
Then, we need to add the settings of the QnA bot into our bot in the appsettings.json file. 
Then, we'll need code to call the QnA service and appropriately retrieve the results. 
Then, we'll need to configure the bot to handle the Luis intents (already done) but also to call the QnA service if the intents are not called.

So, let's do the above steps.  To create/configure a QnA knowledge base, follow these two tutorials: 
https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/tutorials/create-publish-query-in-portal
https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/tutorials/create-qna-bot

If you have trouble with any of the above steps, you can look here at an alternative project which helps guide you through the appropriate steps:
https://github.com/microsoft/ailab/tree/master/BuildAnIntelligentBot#adding-knowledge-to-your-bot-with-qna-maker

We're going to need to add the setting of the QnA Bot into our bot in the appsettings.json file.
So to figure out what it is we'll eventually need, let's look through the official Microsoft samples again.  This time, turn your attention to the project QnaBot.
Look at the appsettings.json file, you'll see:
```
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "QnAKnowledgebaseId": "",
  "QnAAuthKey": "",
  "QnAEndpointHostName": ""
}
```

The first two placeholders are pretty generic and we already have them in our current project, let's however take the next three and add them to our current project's app settings file, now it should look like this:

```
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
 
  "LuisAppId": "5555-your-special-string-55555555",
  "LuisAPIKey": "555your555special555string",
  "LuisAPIHostName": "westus.api.cognitive.microsoft.com",
 
  "QnAKnowledgebaseId": "",
  "QnAAuthKey": "",
  "QnAEndpointHostName": "",
}
```

So now's look at our QnA maker project in the qnamaker.ai portal; assuming you've been able set up a QnA service, let's grab the needed values:
So go to QnAMaker.ai > My Knowledge bases > click on the project you've created > Settings (assuming you've published already).

You'll see something like the following (format is correct - some of the strings changed):
```
POST /knowledgebases/555555-knowledge-bases-555-55555555/generateAnswer
Host: https://yourservice55555.azurewebsites.net/qnamaker
Authorization: EndpointKey 5555555-end-point-key-555555555
Content-Type: application/json
{"question":"<Your question>"}
```

This is how it translates:
555555-knowledge-bases-555-55555555 => QnAKnowledgebaseId
5555555-end-point-key-555555555 => QnAAuthKey
https://yourservice55555.azurewebsites.net/qnamaker => QnAEndpointHostName


In the end, you'll have something that looks like this:
```
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
 
  "LuisAppId": "5555-your-special-string-55555555",
  "LuisAPIKey": "555your555special555string",
  "LuisAPIHostName": "westus.api.cognitive.microsoft.com",
 
  "QnAKnowledgebaseId": "555555-knowledge-bases-555-55555555",
  "QnAAuthKey": "5555555-end-point-key-555555555",
  "QnAEndpointHostName": "https://yourservice55555.azurewebsites.net/qnamaker",
}
```

We're going to need to create a QnA Service similar to the Luis service.

Take a look at the Luis service in the CoreBot > LuisHelper.cs

At a high-level, it simply calls a Luis service and then looks for the specific intent "Book_flight".  If it receives that intent, it will specifically search for various entities and then add them to a specific pre-defined object "BookingDetails" and then return that object.

We're going to need to create a similar QnA service.

Copy the LuisHelper.cs file, and create a new file called QnAHelper.cs.  Copy the contents over from LuisHelper over to QnAHelper.

Let's go through the changes we need to make section by section.

First, change the name of the static class to QnAHelper.
Then, change the return type to Task from Task<BookingDetails>.
Change the name of the method to ExecuteQnAQuery from ExecuteLuisQuery.

Then, looking through the method, comment out everything except for the structure. ie. the try/catch and contents of the catch.  But change the contents of the catch statement to say QnA instead of Luis.

So what remains uncommented should look like this:

```
public static class QnAHelper
{
public static async Task ExecuteQnAQuery(IConfiguration configuration, ILogger logger, ITurnContext turnContext, CancellationToken cancellationToken)
{
     try
     {

     }
     catch (Exception e)
     {
          logger.LogWarning($"QnA Exception: {e.Message} Check your QnA configuration.");
     }     
}
```

Now go back to the QnABot project, look at the file QnaBot.cs.  What we want is in the OnMessageActivityAsync. Take the contents there and copy it, into the "try" section of your QnAHelper class.

```
var httpClient = _httpClientFactory.CreateClient();

var qnaMaker = new QnAMaker(new QnAMakerEndpoint
{
     KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
     EndpointKey = _configuration["QnAAuthKey"],
     Host = _configuration["QnAEndpointHostName"]
}, null, httpClient);

_logger.LogInformation("Calling QnA Maker");

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
```

You'll notice the warnings - let's handle them.  
Remove the underscore before "_configuration" and "_logger".  
Remove the line:
```
var httpClient = _httpClientFactory.CreateClient();
```
And then at the end of the new QnAMaker method remove the words "null, httpClient".

Finally, you're going to need to upload a Nuget packages: "Microsoft.Bot.Builder.AI.QnA"
After you're done that, right click on QnAMaker and QnAMakerEndpoint, go to QuickActionsAndRefactorings and add the appropriate using statement.

Finally, configure the bot to handle the Luis intents and to call the QnA service if the intents are not called.

Okay - so we want to create the ability to target multiple Luis intents and then if none of the intents register for it to get picked up by the QnA Maker via the new QnAHelper class we just created.

So if we look through our code, you'll notice in MainDialog the step ActStepAsync is where Luis gets called in the original project.  
Let's make our changes here.

If you look at what is currently there,
```
var bookingDetails = ...
```
it is looking for the results of the LuisHelper execution which *currently* returns a BookingDetails object.  

Because we want to have multiple Luis results, we don't want to always return a BookingDetails object.  We'd rather have it return instead the Luis intent.

Then we're going to need to receive the Luis intent and the appropriate trigger a corresponding dialog.

So comment out the contents of the ActStepAsync method.

Let's use a switch statement that walks through the various Luis results dialogs:

```
var luisResult = await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken);

switch(luisResult.Intent)
{ 
     case "Book_flight":
     case "None":
     case "Cancel":
     default:
          //Default to QnA
          await QnAHelper.ExecuteQnAQuery(Configuration, Logger, stepContext.Context, cancellationToken);
     
     return await stepContext.BeginDialogAsync(nameof(MainDialog), null)
}
```

Now we've got to do two things.  

First, we have to reconfigure the LuisHelper to return an object that returns an Intent and also other details and object that might be important to the Luis results.

Second, we have to make sure that if the intent is Book_flight that it would return an appropriate object - in our case we'll choose BookingDetailsModel.

Then pass the results to the dialog.

So it would look something like this:
```
var luisResult = await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken);

switch(luisResult.Intent)
{ 
     case "Book_flight":
          //We need to return flight details

          // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
          return await stepContext.BeginDialogAsync(nameof(BookingDialog), luisResult, cancellationToken);
```

So let's reconfigure the LuisHelper.  

We always want it to return an Intent but we also need it to return other objects as necessary. So how do we do it?  

Let's have it return a base model; let's call it "BaseModel" that will always includes an Intent property.  
Then if we need to return more detailed objects we'll return those objects and make sure that those detailed objects subclass that base model so anything we return will be acceptable.

In Visual Studio, create a new folder called "Models".  Add three new Items - three new C# classes, one called BaseModel.cs and the other called BookingDetailsModel and the other called NoIntentsModel.

They should look like the following respectively.

```
public abstract class BaseModel
{
     public string Intent { get;set;}
}
```

```
public class BookingDetailsModel : BaseModel
{
     public string Destination { get; set; }
     public string Origin { get; set; }
     public string TravelDate { get; set; }
}
```

```
public class NoIntentModel : BaseModel
{

}	
```

Go back to the LuisHelper.cs class.  Instead of the method returning a "BookingDetails" object, have it instead return a BaseModel.  This sets us up to return any model we'd like so long as it subclasses the BaseModel abstract class.

The method is organized to return BookingDetails.  We need to reconfigure this so that the appropriate specific return object is dicated by Luis.

At the end of the "if" statement, return a model that will return an object that still conforms to "BaseModel" but doesn't trigger an Intent (so that the bot will default to QnAMaker):

```
if (intent == "Book_flight")
{
     ...
     ...
}
else
{
     return new NoIntentModel() { Intent = intent };
}
```

Let's make the return object for the Book_flight intent to be the FlightDetailsModel.

We're going to need to change the BookingDetails object references to BookingDetailsModel.

In the BookingDialog.cs, we're going to need in each step make sure that the object has been appropriately cast to the BookingDetailsModel.

You'll need to change the following:
```
var bookingDetails = (BookingDetails)stepContext.Options;
```

To this:
```
var bookingDetails = (BookingDetailsModel)stepContext.Options;
```
In the MainDialog.cs, you'll also notice there is a type cast to BookingDetails in FinalStepAsync.  

It will look like this:
```
var result = (BookingDetailsModel)stepContext.Result;
```

Go ahead and run the emulator.  Try triggering the Luis "Book_flight" intent and observe the actions.  
Now ask question best answered by the QnA service.

- The above code can be found under branch-2-Working-With-QnA-Maker

Bonus: You'll notice that almost nothing of the details of the Luis intent is picked up by the Bot.  
Currently, the Intent itself is picked up but the Entities are not.  You'll want to add Entities.  Look for the file FlightBooking.json and try creating similar entities.

Hint:  Try adding: 
new entity: Airport > List > Values > New York -> new york, new york city 
new entity: From > Composite > Child > Airport
prebuilt entity: datetimeV2

Another Hint - you'll need to go to your Book_flight intent and scroll over specific Utterances to tag Composite entities.

If you want to see your Luis models as json to compare, click Train > Publish.  Then go to Manage > Versions > Click on a Version > Export as Json

### ADDING OUR OWN DIALOGS

##### Working with AuthDialog

We're going to add two more dialogs - an OAuth dialog and then a third-party service call dialog.  In our sample, we'll call Salesforce - you can modify this to what you'd like.

A couple things need to change first.  If you've been following along, the CoreBot sample was built assuming only one intent and only one dialog.

Everything is triggered from the MainDialog.  If the "Book_flight" intent is detected, then the BookingDialog is triggered.  

At the end of MainDialog look for the following:

```
private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
     // If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
     if (stepContext.Result != null)
     {
          var result = (BookingDetailsModel)stepContext.Result;
          // Now we have all the booking details call the booking service.
          // If the call to the booking service was successful tell the user.

          var timeProperty = new TimexProperty(result.TravelDate);
          var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
          var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
          await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
     }
     else
     {
          await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
     }
     return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
}
```

Copy it - for the next steps.  And then simplify it to:

```
private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
     await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
     return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
}
```

Let's go to BookingDialog.cs - the final step is currently:
```
private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
     if ((bool)stepContext.Result)
     {
          var bookingDetails = (BookingDetailsModel)stepContext.Options;

          return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
     }
     else
     {
          return await stepContext.EndDialogAsync(null, cancellationToken);
     }
}
```

Change it to:  (We've transfered the SendActivityAsync, changed the name of the variable to match what is in this class, and end the dialog without passing on another object:
```
private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
     if ((bool)stepContext.Result)
     {
          var bookingDetails = (BookingDetailsModel)stepContext.Options;

          // Now we have all the booking details call the booking service.
          // If the call to the booking service was successful tell the user.

          var timeProperty = new TimexProperty(bookingDetails.TravelDate);
          var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);               
	  var msg = $"I have you booked to {bookingDetails.Destination} from {bookingDetails.Origin} on {travelDateMsg}";
          await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                
          return await stepContext.EndDialogAsync(null, cancellationToken);
     }
     else
     {
          return await stepContext.EndDialogAsync(null, cancellationToken);
     }
}
```

We've isolated the dialog.  Now we're going to add the OAuth dialog.

Look through the AuthenicationBot - most of the plumbing is similar to what we've been used for the CoreBot.

The main things we want be looking at are MainDialog and the LogoutDialog; however there are some bit and pieces that are live elsewhere in the project.  

You'll also need to add the MicrosoftAppId from the MicrosoftAppPassword from the Azure portal *both* in the AppSettings.json and in the settings in the Azure emulator. 

In case you run into issues, one potential recommendation is to use AuthenticationBot from the official samples.  Make sure it works there, then transfer the rest of the pieces over as necessary.  

Here are the pieces you need from the AuthenticationBot.

First, bringover the LogoutDialog.  Copy it over - the only think you'll need to change is the namespace.

The main operation of the AuthenticationBot lives in the MainDialog.

Let's create a new class AuthDialog in our project where we'll copy over the contents of the MainDialog.
You'll need to make a few changes including of course the namespace name.

Before, the class was defined like this:

```
namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger Logger;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            Logger = logger;

```

We'll need to change it something like this:
```
namespace CoreBot.AuthDialogs
{
    public class AuthDialog : LogoutDialog
    {
        public AuthDialog(IConfiguration configuration)
            : base(nameof(AuthDialog), configuration["ConnectionName"])
        {

```

You'll need to add a couple things - when you add a Dialog, you've got to register it so the Bot can keep track of it.
```
public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))     
{
     Configuration = configuration;
     Logger = logger;

     AddDialog(new AuthDialog(Configuration));
```

There is one more piece that you'll need to make this work.  
In the file, DialogAndWelcomeBot.cs you'll need to this:
```
protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
{
     Logger.LogInformation("Running dialog with Token Response Event Activity.");

     // Run the Dialog with the new Token Response Event Activity.
     await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
}
```

In your Luis.ai project, add another Intent called "AuthDialog_Intent".  Then in the MainDialog add the following:
```
switch (luisResult.Intent)
{
     case "Book_flight":
          //We need to return flight details
          // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.               return await stepContext.BeginDialogAsync(nameof(BookingDialog), luisResult, cancellationToken);
     case "AuthDialog_Intent":
          //Type something like "Oauth card" or "Auth Dialog intent"
          //Run the AuthBot Dialog          
	  return await stepContext.BeginDialogAsync(nameof(AuthDialog), luisResult, cancellationToken);
     case "None":
     case "Cancel":
     default:
```

You'll then need to follow the details here from the document:
https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=aadv1%2Ccsharp%2Cbot-oauth#create-your-bot-resource-on-azure

* You'll need a Bot Channel Registration
* You'll need bot's ID and Password (these will be needed to be added in your project in Visual Studio but also in your emulator).
* You can create an Azure AD application to test your project as described in the project.  
However, for this project and the next, we're going to make an app in Salesforce but we still want the redirect URL described in the document (ie. https://token.botframework.com/.auth/web/redirect)
     - There are many official documents and video to create a connected app in saleforce.  Here is one example: https://blog.mkorman.uk/integrating-net-and-salesforce-part-1-rest-api/
     - https://salesforce.stackexchange.com/questions/40346/where-do-i-find-the-client-id-and-client-secret-of-an-existing-connected-app

Run the application, try signing in with your salesforce account and make sure you can aquire the token.

 - The above code can be found under Branch-3-Working-With-AuthDialog

##### Working with a 3rd-party API 

Let's recreate the same Oauth project but extend it.  Let's use the token and then with an HttpClient call Salesforce.

Here's where the apps are.  Here's how to navigate to your apps > Platform Tools > Apps > App Manager
https://na132.lightning.force.com/lightning/setup/NavigationMenus/home

We need data!  So in salesforce, go through and create an Account and Contact:
https://na132.lightning.force.com/lightning/page/home
https://na132.lightning.force.com/lightning/o/Contact/list?filterName=Recent
https://na132.lightning.force.com/lightning/o/Account/list?filterName=Recent
You can took look an old contact: https://na132.lightning.force.com/lightning/r/Contact/0034P00002VDKB6QAP/view

Now URL do use to get info from Salesforce?

Look for the API developer reference:
https://developer.salesforce.com/docs/api-explorer/sobject/Account/get-account-id/try

It gives you the URL:
GET/services/data/v39.0/sobjects/Account/{Id}

And the base URL - you can find by pressing the gear settings button on the upper right of the live response.	

Press the Gear "Settings" Button on the upper right of the Live Response:  (The "Instance" Url is what you're looking for).
Connection Settings
Username:
myemailaddy@myemail.com
Instance Url:
https://na132.salesforce.com
My Domain:
<none>

To look for an *Account* from your saleforce account, I pulled up a link like this.  I went to recent accounts:
https://na132.lightning.force.com/lightning/o/Account/list?filterName=Recent

Clicked an account and you get a url like this:
https://na132.lightning.force.com/lightning/r/Account/0014P000027LgWKQA0/view

Take the account number after the "Account/" and before the "/view"

Another salesforce link that could be useful:
https://developer.salesforce.com/docs/api-explorer/sobject/Contact/get-contact-id

If you want to test everything, try using [Postman](https://www.getpostman.com/).   Use the above urls and account numbers to form the Get request.  You're going to need to copy a new non-expired token from the previous AuthDialog. Under the Url, click the Authorization tab, pick "Bearer Token" and then use the token you copied under the Token field.

From either the salesforce documentation pages or the JSON that is returned in postman - you'll need to convert that into C# objects:
https://app.quicktype.io/#l=cs&r=json2csharp

We want to add this class to the project in Visual Studio.  We'll need to add this as a helper for when we're parsing through the JSON that is returned from Salesforce in our API.

Next copy the AuthDialog into a new class called APIDialog.  Remember to register this APIDailog in your MainDialog class in the same way you did for the AuthDialog.  Also make sure in the switch/case statement - you create new case and new Luis intent - let's name it "APIDialog_Intent"
```
case "APIDialog_Intent":
//Type something like "Salesforce query" or "I need to check my quota" "I need to check my sales targets"

//Run the AuthBot Dialog
return await stepContext.BeginDialogAsync(nameof(APIDialog), luisResult, cancellationToken);
```

We need a HttpClient to make the call to salesforce - let's look through some various samples to see how HTTP client is used:
https://github.com/microsoft/ailab/blob/master/BuildAnIntelligentBot/src/ChatBot/Services/TranslatorTextService.cs

We'll take a look here and create a method that will call the API and then return the JSON string.

```
public async Task<string> Translate(string sourceLanguage, string targetLanguage, string text)
{
     if (string.Equals(sourceLanguage, targetLanguage, StringComparison.OrdinalIgnoreCase))
     {
          return text; // No translation required
     }

     var body = new System.Object[] { new { Text = text } };
     var requestBody = JsonConvert.SerializeObject(body);

     using (var client = new HttpClient())
     using (var request = new HttpRequestMessage())
     {
          request.Method = HttpMethod.Post;
          request.RequestUri = new Uri($"{TranslateMethodUri}/translate{UriParams}&to={targetLanguage}");
          request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
          request.Headers.Add("Ocp-Apim-Subscription-Key", _translatorTextKey);

          var response = await client.SendAsync(request);
          response.EnsureSuccessStatusCode();
          var responseBody = await response.Content.ReadAsStringAsync();
          var result = JsonConvert.DeserializeObject<List<TextTranslatorResponse>>(responseBody);
          return result.First().Translations.First().Text;
     }
}
```

Create a similar method at the end of your class definition, but let's change the name of the method SalesforceAPIGetAccountInfo - you're going to need to pass the token in that we've retrieved in the previous project.

Create two new steps in the waterfall dailog.  One called UnformattedJSONSalesforce and the other called FormattedJSONSalesforce.  Remember to both define the steps but also to put those steps in the waterfall steps array near the top of the class.

Exercise 1:
For UnformattedJSONSalesforce, recieve the token from the previous step.  Call the new SalesforceAPIGetAccountInfo("your-token-here") and of course pass the token.  Receive a unformatted json string as the result. Display that in the chat bot.

Exercise 2:
For FormattedJSONSalesforce, receive the unformatted json from the previous step.  If you've use the https://app.quicktype.io/#l=cs&r=json2csharp link you'll notice at the very top in the comments, it will show how to deserialize the json into the object it detected.
Use that method and then you should get a C# object.  Then use that to create a better formatted return string for your results.  

 - The above code can be found under Branch-4-Working-With-A-3rd-Party-API
