using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ProxiCall.FlexIO.Models.AppSettings;
using ProxiCall.FlexIO.services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Activity = Microsoft.Bot.Connector.DirectLine.Activity;

namespace ProxiCall.FlexIO.Controllers
{
    [Produces("application/xml")]
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private readonly DirectlineConfig _directlineConfig;
        private static BotConnector _botConnector;
        private ConversationIdService _conversationIdService;

        public VoiceController(
            IOptions<DirectlineConfig> directlineOptions,
            ConversationIdService conversationIdService)
        {
            _directlineConfig = directlineOptions.Value;
            _conversationIdService = conversationIdService;
        }

        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveCall([FromForm] string CallSid, [FromForm] string CallStatus, [FromForm] string From, [FromForm] string SpeechResult)
        {
            var conversationId = _conversationIdService.GetConversationIdByFrom(From);
            if (conversationId == null)
            {
                _botConnector = new BotConnector(_directlineConfig, CallSid);
                _conversationIdService.AddConversationIdByFrom(_botConnector._conversationId, From);
            }
            else
            {
                _botConnector = new BotConnector(_directlineConfig, CallSid, conversationId);
            }

            var activity = new Activity
            {
                From = new ChannelAccount(From, "Proximus"),
                Type = ActivityTypes.Message,
                Text = string.Empty,
                Entities = new List<Entity>()
            };

            var phoneNumber = From.Split("+")[1];

            if (CallStatus == "ringing")
            {
                var entity = new Entity
                {
                    Properties = new JObject
                {
                    {
                        "firstmessage", JToken.Parse(phoneNumber)
                    }
                }
                };
                activity.Entities.Add(entity);
            }
            else
                activity.Text = SpeechResult;

            await _botConnector.SendMessageToBotAsync(activity);
            var activities = await _botConnector.ReceiveMessagesFromBotAsync();

            return Ok(HandleIncomingBotMessagesAsync(activities));
        }

        private XmlDocument HandleIncomingBotMessagesAsync(IList<Activity> botReplies)
        {
            var says = new StringBuilder();

            if (botReplies == null)
            {
                says.Append("une erreur de communication avec le Bot est survenue. Veuillez réessayer plus tard");
            }
            else
            {
                foreach (var activity in botReplies)
                {
                    says.Append(activity.Text);
                }
            }
            var doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);
            XmlElement response = doc.CreateElement(string.Empty, "Response", string.Empty);
            doc.AppendChild(response);

            XmlElement say = doc.CreateElement(string.Empty, "Say", string.Empty);
            XmlAttribute sayAttributeVoice = doc.CreateAttribute("voice");
            sayAttributeVoice.Value = "woman";
            say.Attributes.Append(sayAttributeVoice);
            XmlAttribute sayAttributeLanguage = doc.CreateAttribute("language");
            sayAttributeLanguage.Value = "voicerss.fr";
            say.Attributes.Append(sayAttributeLanguage);
            response.AppendChild(say);

            XmlElement gather = doc.CreateElement(string.Empty, "Gather", string.Empty);
            XmlAttribute gatherMethod = doc.CreateAttribute("method");
            gatherMethod.Value = "POST";
            gather.Attributes.Append(gatherMethod);
            XmlAttribute gatherInput = doc.CreateAttribute("input");
            gatherInput.Value = "speech";
            gather.Attributes.Append(gatherInput);
            XmlAttribute gatherLanguage = doc.CreateAttribute("language");
            gatherLanguage.Value = "fr-FR";
            gather.Attributes.Append(gatherLanguage);
            XmlAttribute gatherTimeOut = doc.CreateAttribute("timeout");
            gatherTimeOut.Value = "2";
            gather.Attributes.Append(gatherTimeOut);
            XmlAttribute gatherAction = doc.CreateAttribute("action");
            gatherAction.Value = "https://proxicallconnector.azurewebsites.net/api/voice/receive";
            gather.Attributes.Append(gatherAction);
            response.AppendChild(gather);

            XmlText sayText = doc.CreateTextNode(says.ToString());
            say.AppendChild(sayText);

            return doc;
        }

    }
}
