using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;

namespace AWS.Patterns.SQS
{
    public class SQSConsumer<TRecordType> : IQueueConsumer
    {
        private readonly IAmazonSQS _sqs;

        public SQSConsumer(IAmazonSQS sqs)
        {
            _sqs = sqs ?? throw new ArgumentNullException(nameof(sqs));
        }
        
        public Task<int> ConsumeAsync(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}