using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace AWS.Patterns.SQS
{
    public class SQSConsumer<TRecordType> : IQueueConsumer
    {
        private readonly IAmazonSQS _sqs;
        private readonly SQSConsumerConfig _config;
        private readonly IQueueItemProcessor<TRecordType> _processor;
        private readonly int _maxMessagesToPoll = 10; // we cannot poll for more than 10 messages at a time
        private TransformManyBlock<ReceiveMessageResponse, TRecordType> _convertMessageBlock;
        private ActionBlock<TRecordType> _processBlock;
        private BufferBlock<TRecordType> _recordsBlock = new BufferBlock<TRecordType>();
        
        public SQSConsumer(IAmazonSQS sqs, SQSConsumerConfig config, IQueueItemProcessor<TRecordType> processor)
        {
            _sqs = sqs ?? throw new ArgumentNullException(nameof(sqs));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _processor = processor;
            _convertMessageBlock = new TransformManyBlock<ReceiveMessageResponse, TRecordType>(r => ConvertMessages(r),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 5
                });
            _processBlock = new ActionBlock<TRecordType>(r => _recordsBlock.Post(r));

            _convertMessageBlock.LinkTo(_processBlock);
        }
        
        public async Task<int> ConsumeAsync(CancellationToken token)
        {
            // set counter
            var messagesPossible = _config.ItemsPerBatch;

            do
            {
                var messagesToPoll = Math.Min(messagesPossible, _maxMessagesToPoll);
                
                var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest()
                {
                    QueueUrl = _config.QueueUrl,
                    MaxNumberOfMessages = messagesToPoll,
                    WaitTimeSeconds = 20, // long polling
                    VisibilityTimeout = _config.ExpectedTimeToProcessSingleItem, // expected time it will take for a single message to process so it hides the message for that time
                }, token);
                
                // send these messages to the buffer
                _convertMessageBlock.Post(response);
                
                // get the number of messages read
                messagesPossible -= response.Messages.Count;
            } while (messagesPossible > 0 && !token.IsCancellationRequested);
            
            _convertMessageBlock.Complete();

            return 0;
        }

        private IEnumerable<TRecordType> ConvertMessages(ReceiveMessageResponse sqsResponse)
        {
            for (var index = sqsResponse.Messages.Count - 1; index >= 0; index--)
            {
                var message = sqsResponse.Messages[index];
                yield return JsonConvert.DeserializeObject<TRecordType>(message.Body);
            }
        }
    }
}