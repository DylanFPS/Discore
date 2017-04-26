﻿namespace Discore.Http
{
    /// <summary>
    /// A set of options used to modify the properties of a guild embed.
    /// </summary>
    public class ModifyGuildEmbedParameters
    {
        /// <summary>
        /// Gets or sets whether the embed is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets or sets the ID of the guild channel this embed is for.
        /// </summary>
        public Snowflake ChannelId { get; set; }

        /// <summary>
        /// Sets whether the embed is enabled.
        /// </summary>
        public ModifyGuildEmbedParameters SetEnabled(bool enabled)
        {
            Enabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets the ID of the guild channel this embed is for.
        /// </summary>
        public ModifyGuildEmbedParameters SetChannel(Snowflake channelId)
        {
            ChannelId = channelId;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("enabled", Enabled);
            data.Set("channel_id", ChannelId);

            return data;
        }
    }
}
