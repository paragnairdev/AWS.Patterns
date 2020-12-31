using System;

namespace AWS.Patterns.SQS
{
    public class SQSConsumerConfig : QueueConfig
    {
        private readonly int _itemsPerBatch;

        public SQSConsumerConfig(string queueUrl, int itemsPerBatch) : base(queueUrl)
        {
            _itemsPerBatch = itemsPerBatch > 0 ? itemsPerBatch : throw new ArgumentException("Items per batch should be at least 1", nameof(itemsPerBatch));
        }
    }
}