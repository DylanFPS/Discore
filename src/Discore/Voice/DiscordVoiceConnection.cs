﻿using Discore.Voice.Net;
using Discore.WebSocket;
using Discore.WebSocket.Net;
using System;
using System.Threading;

namespace Discore.Voice
{
    public class VoiceConnectionEventArgs : EventArgs
    {
        public Shard Shard { get; }
        public DiscordVoiceConnection Connection { get; }

        internal VoiceConnectionEventArgs(Shard shard, DiscordVoiceConnection connection)
        {
            Shard = shard;
            Connection = connection;
        }
    }

    public class VoiceConnectionErrorEventArgs : VoiceConnectionEventArgs
    {
        public Exception Exception { get; }

        internal VoiceConnectionErrorEventArgs(Shard shard, DiscordVoiceConnection connection, Exception exception)
            : base(shard, connection)
        {
            Exception = exception;
        }
    }

    public sealed class DiscordVoiceConnection : IDisposable
    {
        /// <summary>
        /// The byte size of a single PCM audio block.
        /// </summary>
        public const int PCM_BLOCK_SIZE = 3840;

        /// <summary>
        /// Called when the voice connection first connects.
        /// </summary>
        public event EventHandler<VoiceConnectionEventArgs> OnConnected;
        /// <summary>
        /// Called when the voice connection is disconnected.
        /// </summary>
        public event EventHandler<VoiceConnectionEventArgs> OnDisconnected;
        /// <summary>
        /// Called when the voice connection unexpectedly closes.
        /// </summary>
        public event EventHandler<VoiceConnectionErrorEventArgs> OnError;
        /// <summary>
        /// Called when this voice connection is no longer useable. (eg. disconnected, error, failure to connect).
        /// </summary>
        public event EventHandler<VoiceConnectionEventArgs> OnInvalidated;

        /// <summary>
        /// Gets the shard this connection is managed by.
        /// </summary>
        public Shard Shard { get; }
        /// <summary>
        /// Gets the guild this voice connection is in.
        /// </summary>
        public DiscordGuild Guild { get { return guildCache.Value; } }
        /// <summary>
        /// Gets the member this connection is communicating through.
        /// </summary>
        public DiscordGuildMember Member { get { return memberCache.Value; } }

        /// <summary>
        /// Gets the current voice channel this connection is in.
        /// </summary>
        public DiscordGuildVoiceChannel VoiceChannel
        {
            // Voice state will not be immediately available,
            // so return the initial voice channel while we are still connecting.
            get
            {
                return voiceState != null && voiceState.ChannelId.HasValue 
                    ? guildCache.VoiceChannels.Get(voiceState.ChannelId.Value).Value 
                    : intialVoiceChannel;
            }
        }
        /// <summary>
        /// Gets whether this connection is connected.
        /// </summary>
        public bool IsConnected { get { return socket.IsConnected; } }
        /// <summary>
        /// Gets whether this connection is currently performing its handshake.
        /// </summary>
        public bool IsConnecting { get { return isConnecting; } }
        /// <summary>
        /// Gets whether this connection is available to use.
        /// </summary>
        public bool IsValid { get { return isValid; } }
        /// <summary>
        /// Gets the speaking state of this connection.
        /// </summary>
        public bool IsSpeaking { get { return isSpeaking; } }
        /// <summary>
        /// Gets the number of unsent voice data bytes.
        /// </summary>
        public int BytesToSend { get { return socket.BytesToSend; } }
        /// <summary>
        /// Gets or sets whether the sending of voice data is paused.
        /// </summary>
        public bool IsPaused
        {
            get { return socket.IsPaused; }
            set { socket.IsPaused = value; }
        }

        DiscoreGuildCache guildCache;
        DiscoreMemberCache memberCache;

        Gateway gateway;

        VoiceSocket socket;
        DiscordVoiceState voiceState;
        DiscoreLogger log;
        DiscordGuildVoiceChannel intialVoiceChannel;

        string token;
        string endpoint;
        bool isDisposed;
        bool isValid;
        bool isConnecting;

        bool isSpeaking;

        internal DiscordVoiceConnection(Shard shard, Gateway gateway, DiscoreGuildCache guildCache, DiscoreMemberCache memberCache,
            DiscordGuildVoiceChannel intialVoiceChannel)
        {
            Shard = shard;

            this.gateway = gateway;
            this.guildCache = guildCache;
            this.memberCache = memberCache;

            this.intialVoiceChannel = intialVoiceChannel;

            log = new DiscoreLogger($"VoiceConnection:{guildCache.Value.Name}");

            isValid = true;
            isSpeaking = true;

            socket = new VoiceSocket(guildCache, memberCache);
            socket.OnError += Socket_OnError;
        }

        /// <summary>
        /// Initiates this voice connection.
        /// </summary>
        /// <param name="startMute">Whether the authenticated user should connect self-muted.</param>
        /// <param name="startDeaf">Whether the authenticated user should connect self-deafened.</param>
        /// <exception cref="InvalidOperationException">Thrown if connect is called more than once.</exception>
        public void Connect(bool startMute = false, bool startDeaf = false)
        {
            if (isValid)
            {
                if (!isConnecting && !IsConnected)
                {
                    isConnecting = true;
                    gateway.SendVoiceStateUpdatePayload(intialVoiceChannel.GuildId, intialVoiceChannel.Id, startMute, startDeaf);

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        // Wait 10s
                        Thread.Sleep(10000);

                        // If still not connected, timeout and disconnect.
                        if (isConnecting)
                        {
                            socket.Disconnect();
                            Invalidate();
                        }
                    });
                }
                else
                    throw new InvalidOperationException("Voice connection is already connecting or is currently connected.");
            }
        }

        /// <summary>
        /// Closes this voice connection.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public bool Disconnect()
        {
            if (isValid)
            {
                socket.Disconnect();
                Invalidate();
                OnDisconnected?.Invoke(this, new VoiceConnectionEventArgs(Shard, this));
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets whether the specified number of bytes can currently 
        /// be sent to this voice connection.
        /// Will return false if not yet connected or invalid.
        /// </summary>
        public bool CanSendVoiceData(int size)
        {
            return isValid && IsConnected && socket.CanSendData(size);
        }

        /// <summary>
        /// Sends the specified PCM bytes to this voice connection.
        /// <para>
        /// The size of the data sent should be equal to or less than <see cref="PCM_BLOCK_SIZE"/>.
        /// </para>
        /// <para>
        /// Should be used along-side <see cref="CanSendVoiceData(int)"/> to
        /// avoid overflowing the buffer.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified number of bytes will exceed the buffer size.
        /// </exception>
        public void SendVoiceData(byte[] buffer, int offset, int count)
        {
            if (isValid)
            {
                socket.SendPCMData(buffer, offset, count);
            }
        }

        /// <summary>
        /// Sets the speaking state of this connection.
        /// </summary>
        public void SetSpeaking(bool speaking)
        {
            if (isValid)
            {
                isSpeaking = speaking;

                if (IsConnected)
                    socket.SetSpeaking(speaking);
            }
        }

        /// <summary>
        /// Clears all queued voice data.
        /// </summary>
        public void ClearVoiceBuffer()
        {
            if (isValid)
            {
                socket.ClearVoiceBuffer();
            }
        }

        internal void OnVoiceStateUpdated(DiscordVoiceState voiceState)
        {
            if (isValid)
            {
                this.voiceState = voiceState;

                if (!IsConnected && token != null && endpoint != null)
                    // Either the token or session id can be received first,
                    // so we must check if we are ready to start in both cases.
                    ConnectSocket();
            }
        }

        internal void OnVoiceServerUpdated(string token, string endpoint)
        {
            if (isValid)
            {
                this.token = token;
                this.endpoint = endpoint;

                if (voiceState != null)
                {
                    // Server updates can be sent twice, the second time
                    // is when the voice server changes, so we need to
                    // reconnect.
                    if (IsConnected)
                    {
                        socket.Disconnect();
                        socket.JoinThreads();
                    }

                    // Either the token or session id can be received first,
                    // so we must check if we are ready to start in both cases.
                    ConnectSocket();
                }
            }
        }

        private void Socket_OnError(object sender, Exception e)
        {
            Disconnect();
            OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, e));
        }

        void ConnectSocket()
        {
            if (socket.ConnectAsync(endpoint, token))
            {
                isConnecting = false;
                socket.SetSpeaking(isSpeaking);
                OnConnected?.Invoke(this, new VoiceConnectionEventArgs(Shard, this));
            }
            else
            {
                log.LogError($"Failed to connect to {endpoint}!");
                Invalidate();
            }
        }

        void Invalidate()
        {
            if (isValid)
            {
                isValid = false;
                isConnecting = false;

                log.LogVerbose("[Invalidate] Disconnecting...");

                gateway.SendVoiceStateUpdatePayload(Guild.Id, null, false, false);

                Shard.Voice.RemoveVoiceConnection(Guild.Id);

                OnInvalidated?.Invoke(this, new VoiceConnectionEventArgs(Shard, this));
            }
        }

        /// <summary>
        /// Invalidates the connection and releases all resources used by this voice connection.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                Invalidate();

                socket.Dispose();
            }
        }
    }
}
