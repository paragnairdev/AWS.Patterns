using System;

namespace AWS.Patterns.SQS
{
    public class QueueConfig
    {
        private readonly string _queueUrl;

        public QueueConfig(string queueUrl)
        {
            _queueUrl = !string.IsNullOrWhiteSpace(queueUrl) ? queueUrl : throw new ArgumentNullException(nameof(queueUrl));
        }
    }
}