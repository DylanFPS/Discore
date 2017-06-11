﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class GatewaySocket
    {
        delegate void PayloadSynchronousCallback(DiscordApiData payload, DiscordApiData data);
        delegate Task PayloadAsynchronousCallback(DiscordApiData payload, DiscordApiData data);

        class PayloadCallback
        {
            public PayloadSynchronousCallback Synchronous { get; }
            public PayloadAsynchronousCallback Asynchronous { get; }

            public PayloadCallback(PayloadSynchronousCallback synchronous)
            {
                Synchronous = synchronous;
            }

            public PayloadCallback(PayloadAsynchronousCallback asynchronous)
            {
                Asynchronous = asynchronous;
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class PayloadAttribute : Attribute
        {
            public GatewayOPCode OPCode { get; }

            public PayloadAttribute(GatewayOPCode opCode)
            {
                OPCode = opCode;
            }
        }

        Dictionary<GatewayOPCode, PayloadCallback> payloadHandlers;

        void InitializePayloadHandlers()
        {
            payloadHandlers = new Dictionary<GatewayOPCode, PayloadCallback>();

            Type taskType = typeof(Task);
            Type gatewaySocketType = typeof(GatewaySocket);
            Type payloadSynchronousType = typeof(PayloadSynchronousCallback);
            Type payloadAsynchronousType = typeof(PayloadAsynchronousCallback);

            foreach (MethodInfo method in gatewaySocketType.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PayloadAttribute attr = method.GetCustomAttribute<PayloadAttribute>();
                if (attr != null)
                {
                    PayloadCallback payloadCallback;
                    if (method.ReturnType == taskType)
                    {
                        Delegate callback = method.CreateDelegate(payloadAsynchronousType, this);
                        payloadCallback = new PayloadCallback((PayloadAsynchronousCallback)callback);
                    }
                    else
                    {
                        Delegate callback = method.CreateDelegate(payloadSynchronousType, this);
                        payloadCallback = new PayloadCallback((PayloadSynchronousCallback)callback);
                    }

                    payloadHandlers[attr.OPCode] = payloadCallback;
                }
            }
        }

        #region Receiving
        [Payload(GatewayOPCode.Dispatch)]
        void HandleDispatchPayload(DiscordApiData payload, DiscordApiData data)
        {
            sequence = payload.GetInteger("s").Value;
            string eventName = payload.GetString("t");

            OnDispatch?.Invoke(this, new DispatchEventArgs(eventName, data));
        }

        [Payload(GatewayOPCode.Hello)]
        async Task HandleHelloPayload(DiscordApiData payload, DiscordApiData data)
        {
            if (!receivedHello)
            {
                receivedHello = true;

                // Set heartbeat interval
                heartbeatInterval = data.GetInteger("heartbeat_interval").Value;
                log.LogVerbose($"[Hello] heartbeat_interval = {heartbeatInterval}ms");

                // Begin heartbeat loop
                heartbeatCancellationSource = new CancellationTokenSource();
                heartbeatTask = HeartbeatLoop();

                // Notify so the IDENTIFY or RESUME payloads are sent
                await OnHello?.Invoke();
            }
            else
                log.LogWarning("Received more than one HELLO payload.");
        }

        [Payload(GatewayOPCode.HeartbeatAck)]
        void HandleheartbeatAckPayload(DiscordApiData payload, DiscordApiData data)
        {
            receivedHeartbeatAck = true;
        }

        [Payload(GatewayOPCode.Reconnect)]
        void HandleReconnectPayload(DiscordApiData payload, DiscordApiData data)
        {
            // Resume
            log.LogInfo("[Reconnect] Performing resume...");
            OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false));
        }

        [Payload(GatewayOPCode.InvalidSession)]
        void HandleInvalidSessionPayload(DiscordApiData payload, DiscordApiData data)
        {
            bool isResumable = data.ToBoolean().Value;

            if (isResumable)
            {
                // Resume
                log.LogInfo("[InvalidSession] Resuming...");
                OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false, 5000));
            }
            else
            {
                // Start new session
                log.LogInfo("[InvalidSession] Starting new session...");
                OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(true, 5000));
            }
        }
        #endregion

        #region Sending
        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        /// <exception cref="JsonWriterException">Thrown if the given data cannot be serialized as JSON.</exception>
        async Task SendPayload(GatewayOPCode op, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData(DiscordApiDataType.Container);
            payload.Set("op", (int)op);
            payload.Set("d", data);

            // Check with the payload rate limiter
            await outboundPayloadRateLimiter.Invoke().ConfigureAwait(false);
            // Send payload
            await SendAsync(payload).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        Task SendHeartbeatPayload()
        {
            return SendPayload(GatewayOPCode.Heartbeat, new DiscordApiData(sequence));
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public async Task SendIdentifyPayload(string token, int largeThreshold, int shardId, int totalShards)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", token);
            data.Set("compress", true);
            data.Set("large_threshold", largeThreshold);

            if (totalShards > 1)
            {
                DiscordApiData shardData = new DiscordApiData(DiscordApiDataType.Array);
                shardData.Values.Add(new DiscordApiData(shardId));
                shardData.Values.Add(new DiscordApiData(totalShards));
                data.Set("shard", shardData);
            }

            DiscordApiData props = data.Set("properties", new DiscordApiData(DiscordApiDataType.Container));
            props.Set("$os", RuntimeInformation.OSDescription);
            props.Set("$browser", "discore");
            props.Set("$device", "discore");
            props.Set("$referrer", "");
            props.Set("$referring_domain", "");

            log.LogVerbose("[Identify] Sending payload...");

            // Make sure we don't send IDENTIFY's too quickly
            await identifyRateLimiter.Invoke(CancellationToken.None).ConfigureAwait(false);
            // Send IDENTIFY
            await SendPayload(GatewayOPCode.Identify, data).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendResumePayload(string token, string sessionId, int sequence)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", token);
            data.Set("session_id", sessionId);
            data.Set("seq", sequence);

            log.LogVerbose("[Resume] Sending payload...");

            return SendPayload(GatewayOPCode.Resume, data);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public async Task SendStatusUpdate(string game = null, int? idleSince = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("idle_since", idleSince);

            if (game != null)
            {
                DiscordApiData gameData = new DiscordApiData(DiscordApiDataType.Container);
                data.Set("game", gameData);

                gameData.Set("name", game);
            }

            // Check with the game status update limiter
            await gameStatusUpdateRateLimiter.Invoke().ConfigureAwait(false);
            // Send status update
            await SendPayload(GatewayOPCode.StatusUpdate, data).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendRequestGuildMembersPayload(Snowflake guildId, string query, int limit)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("guild_id", guildId);
            data.Set("query", query);
            data.Set("limit", limit);

            return SendPayload(GatewayOPCode.RequestGuildMembers, data);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendVoiceStateUpdatePayload(Snowflake guildId, Snowflake? channelId, bool isMute, bool isDeaf)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("guild_id", guildId);
            data.Set("channel_id", channelId);
            data.Set("self_mute", isMute);
            data.Set("self_deaf", isDeaf);

            return SendPayload(GatewayOPCode.VoiceStateUpdate, data);
        }
        #endregion
    }
}