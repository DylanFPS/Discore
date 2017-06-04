﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a DM or guild channel by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> GetChannel(Snowflake channelId)
        {
            return GetChannel<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Gets a DM or guild channel by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<T> GetChannel<T>(Snowflake channelId)
            where T : DiscordChannel
        {
            DiscordApiData data = await rest.Get($"channels/{channelId}", $"channels/{channelId}").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }

        /// <summary>
        /// Updates the settings of a guild text channel.
        /// </summary>
        /// <param name="textChannelId">The ID of the guild text channel to modify.</param>
        /// <param name="parameters">A set of parameters to modify the channel with.</param>
        /// <returns>Returns the updated guild text channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameters"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildTextChannel> ModifyTextChannel(Snowflake textChannelId,
            GuildTextChannelParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await rest.Patch($"channels/{textChannelId}", requestData,
                $"channels/{textChannelId}").ConfigureAwait(false);
            return (DiscordGuildTextChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Updates the settings of a guild voice channel.
        /// </summary>
        /// <param name="voiceChannelId">The ID of the guild voice channel to modify.</param>
        /// <param name="parameters">A set of parameters to modify the channel with.</param>
        /// <returns>Returns the updated guild voice channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameters"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildVoiceChannel> ModifyVoiceChannel(Snowflake voiceChannelId,
            GuildVoiceChannelParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await rest.Patch($"channels/{voiceChannelId}", requestData,
                $"channels/{voiceChannelId}").ConfigureAwait(false);
            return (DiscordGuildVoiceChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> DeleteChannel(Snowflake channelId)
        {
            return DeleteChannel<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<T> DeleteChannel<T>(Snowflake channelId)
            where T : DiscordChannel
        {
            DiscordApiData data = await rest.Delete($"channels/{channelId}", $"channels/{channelId}").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }

        /// <summary>
        /// Edits a guild channel permission overwrite for a user or role.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task EditChannelPermissions(Snowflake channelId, Snowflake overwriteId,
            DiscordPermission allow, DiscordPermission deny, DiscordOverwriteType type)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("allow", (int)allow);
            data.Set("deny", (int)deny);
            data.Set("type", type.ToString().ToLower());

            await rest.Put($"channels/{channelId}/permissions/{overwriteId}", data,
                $"channels/{channelId}/permissions/permission").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a guild channel permission overwrite for a user or role.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteChannelPermission(Snowflake channelId, Snowflake overwriteId)
        {
            await rest.Delete($"channels/{channelId}/permissions/{overwriteId}",
                $"channels/{channelId}/permissions/permission").ConfigureAwait(false);
        }

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task TriggerTypingIndicator(Snowflake channelId)
        {
            await rest.Post($"channels/{channelId}/typing",
                $"channels/{channelId}/typing").ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list of all channels in a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildChannel>> GetGuildChannels(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/channels",
                $"guilds/{guildId}/channels").ConfigureAwait(false);

            DiscordGuildChannel[] channels = new DiscordGuildChannel[data.Values.Count];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = (DiscordGuildChannel)DeserializeChannelData(data.Values[i]);

            return channels;
        }

        /// <summary>
        /// Creates a new text or voice channel for a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildChannel> CreateGuildChannel(Snowflake guildId, CreateGuildChannelParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await rest.Post($"guilds/{guildId}/channels",
                $"guilds/{guildId}/channels").ConfigureAwait(false);
            return (DiscordGuildChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Changes the settings of a channel in a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildChannel>> ModifyGuildChannelPositions(Snowflake guildId,
            IEnumerable<PositionParameters> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (PositionParameters positionParam in positions)
                requestData.Values.Add(positionParam.Build());

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}/channels", requestData,
                $"guilds/{guildId}/channels").ConfigureAwait(false);

            DiscordGuildChannel[] channels = new DiscordGuildChannel[returnData.Values.Count];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = (DiscordGuildChannel)DeserializeChannelData(returnData.Values[i]);

            return channels;
        }
    }
}
