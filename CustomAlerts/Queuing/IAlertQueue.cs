namespace CustomAlerts.Queuing
{
    public interface IAlertQueue
    {
        float Delay { get; }
        void Enqueue(IAlert alert);
        void Dequeue(IAlert alert);
    }
}