using CustomAlerts.Models;

namespace CustomAlerts.Queuing
{
    public interface IAlertQueue
    {
        void Enqueue(IAlert alert);
    }
}