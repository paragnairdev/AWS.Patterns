using System;
using System.Collections.Generic;
using System.Linq;
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
        
        // blocks
        private TransformManyBlock<ReceiveMessageResponse, MessagePackage<TRecordType>> serializeBlock;
        private TransformBlock<MessagePackage<TRecordType>, MessagePackage<TRecordType>> processBlock;
        private ActionBlock<MessagePackage<TRecordType>> deleteSingleBlock;
        
        public SQSConsumer(IAmazonSQS sqs, SQSConsumerConfig config, IQueueItemProcessor<TRecordType> processor)
        {
            _sqs = sqs ?? throw new ArgumentNullException(nameof(sqs));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }
        
        public async Task<int> ConsumeAsync(CancellationToken token)
        {
            // setup the pipeline
            StartPipeline();
            
            // start consuming
            var buffer = new BufferBlock<ReceiveMessageResponse>();
            var consumer = StartConsumerAsync(buffer);
            
            // start producing
            await StartProducerAsync(buffer, token);
            
            // wait consumers to complete their work
            await consumer;

            return 0;
        }

        private async Task StartProducerAsync(ITargetBlock<ReceiveMessageResponse> buffer, CancellationToken token)
        {
            // set counter
            var messagesPossible = _config.ItemsPerBatch;

            do
            {
                var messagesToPoll = Math.Min(messagesPossible, _maxMessagesToPoll);
                Console.WriteLine("Polling for messages");
                var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest()
                {
                    QueueUrl = _config.QueueUrl,
                    MaxNumberOfMessages = messagesToPoll,
                    WaitTimeSeconds = 20, // long polling
                    VisibilityTimeout = _config.ExpectedTimeToProcessSingleItem, // expected time it will take for a single message to process so it hides the message for that time
                }, token);
                
                // send these messages to the buffer
                buffer.Post(response);
                
                // get the number of messages read
                messagesPossible -= response.Messages.Count;
            } while (messagesPossible > 0 && !token.IsCancellationRequested);
            
            Console.WriteLine("No longer polling");
            buffer.Complete();
        }

        private async Task StartConsumerAsync(ISourceBlock<ReceiveMessageResponse> buffer)
        {
            while (await buffer.OutputAvailableAsync())
            {
                var sqsResponse = await buffer.ReceiveAsync();
                
                // send it to the first block
                await serializeBlock.SendAsync(sqsResponse);
            }
            
            // inform the starting block we will be sending no more messages
            serializeBlock.Complete();
            
            // wait until the final block is complete
            deleteSingleBlock.Completion.Wait();
        }

        private static IEnumerable<MessagePackage<TRecordType>> ConvertMessages(ReceiveMessageResponse sqsResponse)
        {
            for (var index = sqsResponse.Messages.Count - 1; index >= 0; index--)
            {
                var message = sqsResponse.Messages[index];
                var record = JsonConvert.DeserializeObject<TRecordType>(message.Body);
                yield return new MessagePackage<TRecordType>(message, record);
            }
        }

        private void StartPipeline()
        {
            // setup link options
            var linkOptions = new DataflowLinkOptions {PropagateCompletion = true};
            
            // setup buffer options
            var largeBufferOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1000 };
            var deleteBufferOption = new ExecutionDataflowBlockOptions {BoundedCapacity = 10};
            var processBufferOption = new ExecutionDataflowBlockOptions {BoundedCapacity = 2};
            
            // define the blocks
            // this block converts the sqs response to serialized records
            serializeBlock =
                new TransformManyBlock<ReceiveMessageResponse, MessagePackage<TRecordType>>(response =>
                    ConvertMessages(response), largeBufferOptions);
            
            // this block processes a single record
            processBlock =
                new TransformBlock<MessagePackage<TRecordType>, MessagePackage<TRecordType>>(async message =>
                {
                    try
                    {
                        await _processor.ProcessAsync(message.Record);
                        return message;
                    }
                    catch (Exception e)
                    {
                        // TODO: handle exception or report it
                        return null;
                    }
                }, processBufferOption);

            deleteSingleBlock = new ActionBlock<MessagePackage<TRecordType>>(async message =>
            {
                Console.WriteLine($"Deleting message with value {message.Record}");
                await _sqs.DeleteMessageAsync(new DeleteMessageRequest
                {
                    QueueUrl = _config.QueueUrl,
                    ReceiptHandle = message.ReceiptHandle
                });
            });
            
            // link the blocks
            serializeBlock.LinkTo(processBlock, linkOptions);
            processBlock.LinkTo(deleteSingleBlock, linkOptions);
        }

        
    }
}