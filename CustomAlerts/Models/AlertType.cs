namespace CustomAlerts
{
    /// <remark>
    /// Enum member values aren't linear because the Donation AlertType was later on removed after the migration from StreamLabs to CatCore
    /// </remark>
    public enum AlertType
    {
        Follow = 0,
        Subscription = 1,
        Host = 3,
        Bits = 4,
        Raids = 5,
        ChannelPoints = 6,
        Other = 7
    }
}