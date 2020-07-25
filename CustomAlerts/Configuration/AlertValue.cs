namespace CustomAlerts.Configuration
{
    public class AlertValue
    {
        public virtual AlertType AlertType { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual bool OverrideDelay { get; set; } = false;
        public virtual float DelayOverrideTime { get; set; } = 2f;
        public virtual string Value { get; set; } = string.Empty;
    }
}