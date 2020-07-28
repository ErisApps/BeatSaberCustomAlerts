namespace CustomAlerts.Configuration
{
    public class AlertValue
    {
        public virtual string Value { get; set; } = string.Empty;
        public virtual float DelayOverrideTime { get; set; } = 2f;
        public virtual bool OverrideDelay { get; set; } = false;
        public virtual int Volume { get; set; } = 100;
        public virtual AlertType AlertType { get; set; }
        public virtual bool Enabled { get; set; }
    }
}