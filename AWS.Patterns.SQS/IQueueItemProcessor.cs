using System.Threading.Tasks;

namespace AWS.Patterns.SQS
{
    public interface IQueueItemProcessor<in TRecordType>
    {
        Task ProcessAsync(TRecordType record);
    }
}