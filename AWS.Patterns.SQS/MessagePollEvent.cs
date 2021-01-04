using System;

namespace AWS.Patterns.SQS
{
    public class MessagePollEvent : EventArgs
    {
        public bool IsPolling { get; }

        private MessagePollEvent(bool isPolling)
        {
            IsPolling = isPolling;
        }

        public static MessagePollEvent Polling() => new MessagePollEvent(true);
        public static MessagePollEvent StoppedPolling() => new MessagePollEvent(false);
    }
}