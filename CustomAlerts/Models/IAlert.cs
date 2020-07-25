namespace CustomAlerts.Models
{
    public interface IAlert
    {
        AlertType AlertType { get; }
        float Lifeline { get; }
        float Flatline { get; }
        void Spawn();
        void Destroy();
    }
}