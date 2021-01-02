using System;

namespace AWS.Patterns.SQS
{
    public class SQSConsumerConfig : QueueConfig
    {
        public int ExpectedTimeToProcessSingleItem { get; }
        public readonly int ItemsPerBatch;

        public SQSConsumerConfig(string queueUrl, int itemsPerBatch, int expectedTimeToProcessSingleItem) : base(queueUrl)
        {
            ExpectedTimeToProcessSingleItem = expectedTimeToProcessSingleItem > 0 ? expectedTimeToProcessSingleItem : throw new ArgumentException("Expected time to process needs to be at least 1 second", nameof(expectedTimeToProcessSingleItem));
            ItemsPerBatch = itemsPerBatch > 0 ? itemsPerBatch : throw new ArgumentException("Items per batch should be at least 1", nameof(itemsPerBatch));
        }
    }
}