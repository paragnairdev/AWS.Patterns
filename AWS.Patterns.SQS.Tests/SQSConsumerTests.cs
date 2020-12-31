using System;
using Xunit;

namespace AWS.Patterns.SQS.Tests
{
    public class SQSConsumerTests
    {
        [Fact]
        public void Constructor_Should_Throw()
        {
            Assert.Throws<ArgumentNullException>("sqs", () => new SQSConsumer<long>(null));
        }
    }
}