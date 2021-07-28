using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.FlexIO.services
{
    public class ConversationIdService
    {
        private Dictionary<string, string> FromToConversId;
        public ConversationIdService()
        {
            FromToConversId = new Dictionary<string, string>();
        }

        public string GetConversationIdByFrom(string from)
        {
            return FromToConversId.GetValueOrDefault(from);
        }

        public void AddConversationIdByFrom(string conversationId, string from) 
        {
            FromToConversId.Add(from, conversationId);
        }

    }
}
