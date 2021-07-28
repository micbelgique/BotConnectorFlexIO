using System;

namespace ProxiCall.FlexIO.Models.AppSettings
{
    public class DirectlineConfig
    {
        public string DirectlineSecret { get; set; }
        public Uri Host { get; set; }
        public Uri ProxiCallCrmHostname { get; set; }
        public string AdminPhoneNumber { get; set; }
        public string BotName { get; set; }
    }
}
