
using System;
using System.Threading.Tasks;
namespace ServiceBus.DeadletterBehavior.Core
{
    public interface IQueueAgent
    {
        string EntityName { get; set; }

        Task SendMessageAsync(TimeSpan timeToLive);
        Task<string> PeekDeadLetterAsync();
        Task<string> PeekQueueAsync();
        Task<string> ReceiveFromQueueAsync();

        Task CreateQueueAsync();
        Task DeleteQueueAsync();
    }
}
