using Amazon.SQS.Model;

namespace AWS.Patterns.SQS
{
    public class MessagePackage<TRecordType>
    {
        public string MessageId { get; }
        public string ReceiptHandle { get; }
        public TRecordType Record { get; }

        public MessagePackage(Message message, TRecordType record)
        {
            MessageId = message.MessageId;
            ReceiptHandle = message.ReceiptHandle;
            Record = record;
        }
    }
}