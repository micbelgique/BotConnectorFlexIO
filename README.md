# BotConnectorFlexIO

BotConnectorFlexIO is a connector to connect a bot from [Bot Framework](https://dev.botframework.com/) to [FlexIO](https://www.proximus.be/en/id_cl_flexio/companies-and-public-sector/telephony/mobile-services/flexio.html?v1=paidsearch&v3=google&v4=Solution&v6=%2Bflexio&v7=mecor-flexio&gclid=CjwKCAjwo4mIBhBsEiwAKgzXOF1CqEQcsWHHfEHORUaEMSzHIUvNgKntVujeoEAqty1qdXc_tCZbTxoC2UIQAvD_BwE&gclsrc=aw.ds), the platform of Proximus to connect their network to the web via API.

## Setting up the connector

In the [AppSettings.json](https://github.com/micbelgique/BotConnectorFlexIO/blob/master/ProxiCall.FlexIO/appsettings.json), you will find 2 Directline config, you will need to put your Directline Secret from your Bot Framework and your Bot Name too.

If your bot is not hosted, you will need to put in on Azure. Here's [an explenation](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-deploy-az-cli?view=azure-bot-service-4.0) of how to put it online.

## Change settings for FlexIO

In [VoiceController](https://github.com/micbelgique/BotConnectorFlexIO/blob/master/ProxiCall.FlexIO/Controllers/VoiceController.cs), you will find the the function [HandleIncomingBotMessagesAsync](<https://github.com/micbelgique/BotConnectorFlexIO/blob/master/ProxiCall.FlexIO/Controllers/VoiceController.cs#:~:text=private%20XmlDocument-,HandleIncomingBotMessagesAsync,-(IList%3CActivity%3E%20botReplies)>). Inside this function, you will find multiple parameters. The one that you will need to change, it's the GatherAction value because it's the one that come back to your connector after gather speech result.

## How it's working?

![schema of the system](https://github.com/micbelgique/BotConnectorFlexIO/blob/master/schema.png)
