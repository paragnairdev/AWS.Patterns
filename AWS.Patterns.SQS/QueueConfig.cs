using System;

namespace AWS.Patterns.SQS
{
    public class QueueConfig
    {
        public readonly string QueueUrl;

        public QueueConfig(string queueUrl)
        {
            QueueUrl = !string.IsNullOrWhiteSpace(queueUrl) ? queueUrl : throw new ArgumentNullException(nameof(queueUrl));
        }
    }
}