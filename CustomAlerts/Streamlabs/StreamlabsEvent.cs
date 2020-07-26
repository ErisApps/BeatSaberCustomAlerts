namespace CustomAlerts.Streamlabs
{
    public class StreamlabsEvent
    {
        public string Type { get; set; }
        public string EventID { get; set; }
        public string EventFor { get; set; }
        public Message[] Message { get; set; }

        public AlertType AlertType
        {
            get
            {
                switch (Type)
                {
                    case "channelpoints":
                        return AlertType.ChannelPoints;
                    case "follow":
                        return AlertType.Follow;
                    case "subscription":
                        return AlertType.Subscription;
                    case "donation":
                        return AlertType.Donation;
                    case "bits":
                        return AlertType.Bits;
                    case "raid":
                        return AlertType.Raids;
                    case "host":
                        return AlertType.Host;
                    default:
                        return AlertType.Other;
                }
            }
        }
    }
}