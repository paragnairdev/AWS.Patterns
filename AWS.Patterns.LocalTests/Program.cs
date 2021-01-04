using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SQS;
using AWS.Patterns.SQS;

namespace AWS.Patterns.LocalTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting app");
            
            // create the consumer
            var sqsClient = new AmazonSQSClient(FallbackCredentialsFactory.GetCredentials());
            var consumer = new SQSConsumer<int>(sqsClient, new SQSConsumerConfig(Environment.GetEnvironmentVariable("QueueUrl"), 10, 20), new ExampleQueueProcessor());

            consumer.Log += (sender, message) =>
            {
                Console.WriteLine($"Log: {message}");
            };
            
            var tokenSource = new CancellationTokenSource();

            try
            {
                Task.WaitAll(
                    // look for cancellation key
                    Task.Run(() =>
                    {
                        Console.CancelKeyPress += (sender, eventArgs) =>
                        {
                            eventArgs.Cancel = true;
                            Console.WriteLine("Stopping the service, please wait...");
                            tokenSource.Cancel();
                        };
                    }, tokenSource.Token),
                    // run the consumer
                    consumer.ConsumeAsync(tokenSource.Token)
                );

            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Cancellation was requested");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed with errors: {e}");
            }
            
            Console.WriteLine("Completed");
        }
    }

    class ExampleQueueProcessor : IQueueItemProcessor<int>
    {
        public async Task ProcessAsync(int record)
        {
            Console.WriteLine($"Service: Processing record with value {record}");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Console.WriteLine($"Service: Processed record with value {record}");
        }
    }
}