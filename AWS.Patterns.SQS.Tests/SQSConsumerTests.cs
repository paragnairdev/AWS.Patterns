using System;
using Amazon.SQS;
using Moq;
using Xunit;

namespace AWS.Patterns.SQS.Tests
{
    public class SQSConsumerTests
    {
        [Fact]
        public void Constructor_Should_Throw()
        {
            Assert.Throws<ArgumentNullException>("sqs", () => new SQSConsumer<long>(null, null, null));
            Assert.Throws<ArgumentNullException>("config", () => new SQSConsumer<long>(Mock.Of<IAmazonSQS>(), null, null));
        }
    }
}