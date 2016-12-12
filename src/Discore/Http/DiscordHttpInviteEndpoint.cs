﻿using Discore.Http.Net;
using System.Threading.Tasks;

namespace Discore.Http
{
    public class DiscordHttpInviteEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpInviteEndpoint(IDiscordApplication app, RestClient rest)
            : base(app, rest)
        { }

        /// <summary>
        /// Gets an invite by its code.
        /// </summary>
        public async Task<DiscordInvite> Get(string inviteCode)
        {
            DiscordApiData data = await Rest.Get($"invites/{inviteCode}", "GetInvite");
            return new DiscordInvite(App, data);
        }

        /// <summary>
        /// Deletes an invite to a channel.
        /// </summary>
        public async Task<DiscordInvite> Delete(string inviteCode)
        {
            DiscordApiData data = await Rest.Delete($"invites/{inviteCode}", "DeleteInvite");
            return new DiscordInvite(App, data);
        }

        /// <summary>
        /// Accepts an invite to a channel.
        /// Note: This does not work for bot accounts.
        /// </summary>
        public async Task<DiscordInvite> Accept(string inviteCode)
        {
            DiscordApiData data = await Rest.Post($"invites/{inviteCode}", "AcceptInvite");
            return new DiscordInvite(App, data);
        }
    }
}