using System;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Patterns.SQS
{
    public interface IQueueConsumer
    {   
        Task<int> ConsumeAsync(CancellationToken token);
        event EventHandler<string> Log;
        event EventHandler<MessagePollEvent> OnMessagePoll;
    }
}