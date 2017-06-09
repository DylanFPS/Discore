﻿namespace Discore.WebSocket
{
    public class ShardStartException : DiscoreException
    {
        public Shard Shard { get; }
        public ShardFailureReason Reason { get; }

        public ShardStartException(string message, Shard shard, ShardFailureReason reason)
            : base(message)
        {
            Shard = shard;
            Reason = reason;
        }
    }
}
