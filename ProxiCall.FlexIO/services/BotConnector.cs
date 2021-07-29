using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using ProxiCall.FlexIO.Models.AppSettings;


namespace ProxiCall.FlexIO.services
{
    public class BotConnector
    {
        private DirectLineClient _directLineClient;
        public readonly string _conversationId;
        private readonly string _streamUrl;
        private readonly DirectlineConfig _directlineConfig;

        public delegate Task OnReplyHandler(IList<Activity> botReplies, string callSid);

        public BotConnector(DirectlineConfig directlineConfig)
        {
            _directlineConfig = directlineConfig;
            _directLineClient = new DirectLineClient(_directlineConfig.DirectlineSecret);
            var conversation = _directLineClient.Conversations.StartConversation();
            _conversationId = conversation.ConversationId;
            _streamUrl = conversation.StreamUrl;
        }
        public BotConnector(DirectlineConfig directlineConfig, string conversationId)
        {
            _directlineConfig = directlineConfig;
            _directLineClient = new DirectLineClient(_directlineConfig.DirectlineSecret);
            var conversation = _directLineClient.Conversations.ReconnectToConversation(conversationId);
            _conversationId = conversation.ConversationId;
            _streamUrl = conversation.StreamUrl;
        }

        public async Task<List<Activity>> ReceiveMessagesFromBotAsync()
        {
            var webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(_streamUrl), CancellationToken.None);

            string botReply = String.Empty;
            var replyBuffer = WebSocket.CreateClientBuffer(1024 * 8, 1024 * 8);

            var activities = new List<Activity>();

            WebSocketReceiveResult websocketReceivedResult = new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
            while (!websocketReceivedResult.CloseStatus.HasValue)
            {
                do
                {
                    websocketReceivedResult = await webSocket.ReceiveAsync(replyBuffer, CancellationToken.None);
                } while (!websocketReceivedResult.EndOfMessage);

                if (websocketReceivedResult.Count != 0)
                {
                    botReply = Encoding.UTF8.GetString(replyBuffer.ToArray(), 0, websocketReceivedResult.Count);
                    var activitySet = JsonConvert.DeserializeObject<ActivitySet>(botReply);
                    foreach (Activity activity in activitySet.Activities)
                    {
                        //TODO Replace this name with the name of your bot
                        var isFromBot = activity.From.Name == _directlineConfig.BotName;
                        var isIgnoringInput = activity.InputHint == InputHints.IgnoringInput;
                        if (isFromBot)
                        {
                            activities.Add(activity);
                            var isForwardingMessage = false;

                            if (activity.Entities != null && activity.Entities.Count != 0)
                            {
                                isForwardingMessage = activity.Entities[0].Properties.GetValue("forward") != null;
                            }
                            if (!isIgnoringInput || isForwardingMessage)
                            {
                                return activities;
                            }
                        }
                    }
                }

            }
            await webSocket.CloseAsync(websocketReceivedResult.CloseStatus.Value, websocketReceivedResult.CloseStatusDescription, CancellationToken.None);
            return null;
        }

        public async Task SendMessageToBotAsync(Activity userMessage)
        {
            await _directLineClient.Conversations.PostActivityAsync(_conversationId, userMessage, CancellationToken.None);
        }
    }
}
