using System;
using Xunit;

namespace AWS.Patterns.SQS.Tests
{
    public class QueueConfigTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_Should_Throw(string queueUrl)
        {
            Assert.Throws<ArgumentNullException>("queueUrl", () => new QueueConfig(queueUrl));
        }
    }
}