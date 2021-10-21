namespace CustomAlerts.Streamlabs
{
    public class TwitchEvent
    {
        public AlertType AlertType { get; set; }
        public Message[] Message { get; set; }

        // TODO: Remove commented code
        /*{
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
                    case "resub":
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
        }*/
    }
}