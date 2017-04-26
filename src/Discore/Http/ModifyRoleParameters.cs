﻿namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters used for modifying a guild role.
    /// </summary>
    public class ModifyRoleParameters
    {
        /// <summary>
        /// Gets or sets the name of the role (or null to leave unchanged).
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the permissions granted by this role (or null to leave unchanged).
        /// </summary>
        public DiscordPermission? Permissions { get; set; }
        /// <summary>
        /// Gets or sets the display color of the role (or null to leave unchanged).
        /// </summary>
        public DiscordColor? Color { get; set; }
        /// <summary>
        /// Gets or sets whether the role is displayed in the sidebar (or null to leave unchanged).
        /// </summary>
        public bool? IsHoisted { get; set; }
        /// <summary>
        /// Gets or sets whether the role is mentionable (or null to leave unchanged).
        /// </summary>
        public bool? IsMentionable { get; set; }

        /// <summary>
        /// Sets the name of the role.
        /// </summary>
        public ModifyRoleParameters SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the permissions granted by the role.
        /// </summary>
        public ModifyRoleParameters SetPermissions(DiscordPermission? permissions)
        {
            Permissions = permissions;
            return this;
        }

        /// <summary>
        /// Sets the display color of the role.
        /// </summary>
        public ModifyRoleParameters SetColor(DiscordColor? color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets whether the role should be displayed in the sidebar.
        /// </summary>
        public ModifyRoleParameters SetHoisted(bool? isHoisted)
        {
            IsHoisted = isHoisted;
            return this;
        }

        /// <summary>
        /// Sets whether the role is mentionable.
        /// </summary>
        public ModifyRoleParameters SetMentionable(bool? isMentionable)
        {
            IsMentionable = isMentionable;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", Name);
            data.Set("permissions", (long?)Permissions);
            data.Set("color", Color?.ToHexadecimal());
            data.Set("hoist", IsHoisted);
            data.Set("mentionable", IsMentionable);

            return data;
        }
    }
}
