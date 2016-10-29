﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Discore
{
    public class ShardManager
    {
        public int ShardCount { get { return shards?.Length ?? 0; } }
        public IReadOnlyList<Shard> Shards { get { return new ReadOnlyCollection<Shard>(shards); } }

        DiscordApplication app;
        Shard[] shards;

        internal ShardManager(DiscordApplication app)
        {
            this.app = app;
        }

        public void CreateShards(int numberOfShards = 1)
        {
            if (numberOfShards < 1)
                throw new ArgumentOutOfRangeException("numberOfShards", "numberOfShards must be above or equal to 1");

            // Stop existing shards
            ShutdownShards();

            // Create and start new shards
            shards = new Shard[numberOfShards];
            for (int i = 0; i < numberOfShards; i++)
            {
                Shard shard = new Shard(app, i);
                shards[i] = shard;

                shard.Start();
            }
        }

        public void ShutdownShards()
        {
            if (shards != null)
            {
                for (int i = 0; i < shards.Length; i++)
                    shards[i].Stop();

                shards = null;
            }
        }
    }
}
