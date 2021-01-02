using System;
using Xunit;

namespace AWS.Patterns.SQS.Tests
{
    public class SQSConsumerConfigTests
    {
        [Fact]
        public void Constructor_Should_Throw_WhenQueue_NotValid()
        {
            Assert.Throws<ArgumentNullException>("queueUrl", () => new SQSConsumerConfig(null, 10, 0));
            Assert.Throws<ArgumentNullException>("queueUrl", () => new SQSConsumerConfig("", 10, 0));
            Assert.Throws<ArgumentNullException>("queueUrl", () => new SQSConsumerConfig(" ", 10, 0));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(-100)]
        public void Constructor_Should_Throw_When_BatchSize_NotPositive(int batchSize)
        {
            Assert.Throws<ArgumentException>("itemsPerBatch", () => new SQSConsumerConfig("some-url", batchSize, 0));
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(-100)]
        public void Constructor_Should_Throw_When_TimeToProcess_IsNotPositive(int timeToProcess)
        {
            Assert.Throws<ArgumentException>("expectedTimeToProcessSingleItem", () => new SQSConsumerConfig("some-url", 10, timeToProcess));
        }
    }
}