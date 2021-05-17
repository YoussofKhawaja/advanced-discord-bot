using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace ADB.Services
{
    public abstract class EventReplyUtils
    {
        public static async Task MessageUpdatedEmbed(IUser user, IMessageChannel logChannel, IMessageChannel channel, string before, SocketMessage after)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Message edited")
                .WithDescription($"{user.Mention} has updated the following in <#{channel.Id}>!" +
                                 $"\n{before} \n-> {after}")
                .WithFooter($"{user.Username}", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task MessageDeletedEmbed(IUser user, IMessageChannel logChannel, IMessageChannel channel, string after)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Message deleted")
                .WithDescription($"{user.Mention} has deleted the following in <#{channel.Id}>!" +
                                 $"\n{after}")
                .WithFooter($"{user.Username}", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task UserNameUpdatedEmbed(IUser userInitial, IUser userUpdated, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("User name updated")
                .WithDescription($"{userInitial.Mention} updated their name!" +
                                 $"\n**From** : {userInitial.Username} " +
                                 $"\n**To*** : {userUpdated.Username} ")
                .WithThumbnailUrl(userUpdated.GetAvatarUrl() ?? userUpdated.GetDefaultAvatarUrl())
                .WithFooter($"{userUpdated.Username}", userUpdated.GetAvatarUrl() ?? userUpdated.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task UserAvatarUpdatedEmbed(IUser userInitial, IUser userUpdated, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("User avatar updated")
                .WithDescription($"{userInitial.Mention} updated their avatar!" +
                                 $"\n**From** : {userInitial.GetAvatarUrl()} " +
                                 $"\n**To*** : {userUpdated.GetAvatarUrl()} ")
                .WithThumbnailUrl(userUpdated.GetAvatarUrl() ?? userUpdated.GetDefaultAvatarUrl())
                .WithFooter($"{userUpdated.Username}", userUpdated.GetAvatarUrl() ?? userUpdated.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task RoleCreatedEmbed(SocketRole role, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Role created")
                .WithDescription($"Role {role.Mention} created !")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task RoleNameUpdatedEmbed(SocketRole roleBefore, SocketRole roleAfter, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Role name updated")
                .WithDescription($"Role {roleBefore.Mention} updated !")
                .AddField("Updated name", $"{roleBefore.Name} changed to {roleAfter.Name}")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task RolePermUpdatedEmbed(SocketRole roleBefore, SocketRole roleAfter, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Role permission updated")
                .WithDescription($"Role {roleBefore.Mention} updated !")
                .AddField("Updated permission", $"{roleBefore.Permissions.ToList()} changed to {roleAfter.Permissions.ToList()}")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task RoleColorUpdatedEmbed(SocketRole roleBefore, SocketRole roleAfter, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Role color updated")
                .WithDescription($"Role {roleBefore.Mention} updated !")
                .AddField("Updated color", $"{roleBefore.Color} changed to {roleAfter.Color}")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task RoleRemovedEmbed(SocketRole role, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Role removed")
                .WithDescription($"Role {role} removed !")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task ChannelCreatedEmbed(SocketChannel channel, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Channel created")
                .WithDescription($"Channel <#{channel.Id}> created at {channel.CreatedAt.LocalDateTime}!")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task ChannelNameUpdatedEmbed(SocketChannel channelBefore, SocketChannel channelAfter, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Channel name updated")
                .WithDescription($"Channel <#{channelBefore.Id}> updated at {DateTime.Now.ToLocalTime()}!")
                .AddField("Channel name", ((SocketGuildChannel)channelBefore).Name)
                .AddField("Updated name", ((SocketGuildChannel)channelAfter).Name)
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task ChannelPermUpdatedEmbed(SocketChannel channelAfter, SocketChannel channelBefore, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Channel permission updated")
                .WithDescription($"Channel <#{channelBefore.Id}> updated at {DateTime.Now.ToLocalTime()}!")
                .AddField("Updated perm: ", ((SocketGuildChannel)channelBefore).PermissionOverwrites.ToList())
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task ChannelDeletedEmbed(SocketChannel channel, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Channel deleted")
                .WithDescription($"Channel {channel} delete at {DateTime.Now.ToLocalTime()}!")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task UserVoiceJoined(IUser user, IVoiceChannel channel, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("User voice activity")
                .WithDescription($"{user.Mention} joined voice channel <#{channel.Id}>!")
                .WithFooter($"{user.Username}", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
        public static async Task UserVoiceLeft(IUser user, IVoiceChannel channel, IMessageChannel logChannel)
        {
            var builder = new EmbedBuilder()
                .WithTitle("User voice activity")
                .WithDescription($"{user.Mention} left voice channel <#{channel.Id}>!")
                .WithFooter($"{user.Username}", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await logChannel.SendMessageAsync(embed: builder.Build());
        }
    }
}
