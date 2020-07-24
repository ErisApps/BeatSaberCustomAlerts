namespace CustomAlerts
{
    public interface IAlert
    {
        AlertType AlertType { get; }
        float Lifeline { get; }
        void Spawn();
        void Destroy();
    }
}