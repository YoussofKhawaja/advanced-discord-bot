using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADB.Helpers;
using ADB.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace ADB.Modules
{
    [Summary("Moderation Commands")]
    public class Moderation : ModuleBase<SocketCommandContext>

    {
        private readonly ulong ownerID = 493354953734225921;

        private readonly DiscordSocketClient _client;

        public Moderation(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("kick")]
        [Summary("Kick a user from the server.")]
        [RequireBotPermission(GuildPermission.KickMembers)] // Require the bot to have permission to kick users.
        [RequireUserPermission(GuildPermission.KickMembers)] // Require the user to have permission to kick users.
        public async Task Kick(SocketGuildUser targetUser, [Remainder] string reason = "No reason provided.")
        {
            await targetUser.KickAsync(reason); // Kick the user
            await ReplyAsync($"**{targetUser}** has been kicked. Bye bye :wave:");
        }

        [Command("ban")]
        [Summary("Ban a user from the server")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban(SocketGuildUser targetUser, [Remainder] string reason = "No reason provided.")
        {
            await Context.Guild.AddBanAsync(targetUser.Id, 0, reason);
            await ReplyAsync($"**{targetUser}** has been banned. Bye bye :wave:");
        }

        [Command("unban")]
        [Summary("Unban a user from the server")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Unban(ulong targetUser)
        {
            await Context.Guild.RemoveBanAsync(targetUser);
            await Context.Channel.SendMessageAsync($"The user has been unbanned :clap:");
        }

        [Command("purge")]
        [Summary("Bulk deletes messages in chat")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int delNumber)
        {
            var channel = Context.Channel as SocketTextChannel;
            var items = await Context.Channel.GetMessagesAsync(delNumber + 1).FlattenAsync();
            await channel.DeleteMessagesAsync(items);
        }

        [Command("dm")]
        [Summary("DMs a specified user.")]
        public async Task Dm(IGuildUser user, [Remainder] string dm)
        {
            var message = await user.GetOrCreateDMChannelAsync();

            var embed = new EmbedBuilder()
            {
                Color = new Color(37, 152, 255)
            };

            embed.WithTitle($":mailbox_with_mail:  | You have recieved a DM from {Context.User.Username}!");
            embed.Description = $"{dm}";
            embed.WithFooter(new EmbedFooterBuilder().WithText($"Guild: {Context.Guild.Name}"));
            await message.SendMessageAsync("", embed: embed.Build());
            embed.Description = $":e_mail: | You have sent a message to {user.Username}, they will read the message soon.";

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("delete category", RunMode = RunMode.Async)]
        [Summary("Deletes a whole category and all channels in it")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task DeleteCategory([Remainder] string category)
        {
            SocketCategoryChannel categoryChannel =
                Context.Guild.CategoryChannels.FirstOrDefault(c => c.Name == category);

            if (categoryChannel == null)
            {
                await Context.Channel.SendMessageAsync($"Cannot find a category with the name of '{category}'.");
                return;
            }

            foreach (SocketGuildChannel channel in categoryChannel.Channels)
            {
                await channel.DeleteAsync();
                await Task.Delay(200);
            }

            await Task.Delay(500);
            await categoryChannel.DeleteAsync();
        }

        [Command("FindMessageID")]
        [Remarks("Gets the message id of a message in the current channel with the provided message text")]
        [RequireContext(ContextType.Guild)]
        public async Task FindMessageIDAsync([Summary("The content of the message to search for")][Remainder] string messageContent)
        {
            const int searchDepth = 100;
            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(searchDepth).FlattenAsync().ConfigureAwait(false);
            IEnumerable<IMessage> matches = messages.Where(x => x.Content.StartsWith(messageContent.Trim(), StringComparison.OrdinalIgnoreCase));
            if (matches == null || !matches.Any())
            {
                _ = await ReplyAsync($"Message not found. Hint: Only the last {searchDepth} messages in this channel are scanned.").ConfigureAwait(false);
                return;
            }
            else if (matches.Count() > 1)
            {
                _ = await ReplyAsync($"{matches.Count()} Messages found. Please be more specific").ConfigureAwait(false);
                return;
            }
            else
            {
                _ = await ReplyAsync($"The message Id is: {matches.First().Id}").ConfigureAwait(false);
            }
        }

        [Command("SetGame")] // Command declaration
        [Summary("Sets a 'Game' for the bot :video_game: (Only Moderators can use this command)")] // command summary
        [RequireUserPermission(GuildPermission.Administrator)]
        // Needed User Permissions //
        public async Task Setgame([Remainder] string game) // command async task that takes in a parameter (remainder represents a space between the command and the parameter)

        {
            await Context.Client.SetGameAsync(game); // change bots playing status to the provided string parameter
            await Context.Channel.SendMessageAsync($"Successfully set my playing status to '**{game}**'"); // notify user in the text channel the command was used in
        }

        [Command("lockdown")]
        [Summary("Lock all channel")]
        public async Task LockDown()
        {
            if (Context.User.Id.Equals(ownerID) && Context.User is SocketGuildUser)
            {
                foreach (var channel in Context.Guild.Channels)
                {
                    var role = Context.Guild.EveryoneRole;
                    await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(channel)
                        .Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow));
                }
            }
            else
                await ReplyAsync("Permission denied!");
        }

        [Command("unlockchannel")]
        [Summary("unlock all channel")]
        public async Task UnlockChannel()
        {
            if (Context.User.Id.Equals(ownerID) && Context.User is SocketGuildUser)
            {
                foreach (var channel in Context.Guild.Channels)
                {
                    var role = Context.Guild.EveryoneRole;
                    await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel)
                        .Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow));
                }
            }
            else
                await ReplyAsync("Permission denied!");
        }

        [Command("nameall")]
        [Summary("Name all username in Server")]
        public async Task NameAll([Remainder] string msg)
        {
            if (Context.User.Id.Equals(ownerID) && Context.User is SocketGuildUser)
            {
                foreach (var user in Context.Guild.Users)
                    try
                    {
                        await user.ModifyAsync(r => r.Nickname = msg);
                    }
                    catch
                    {
                        Console.WriteLine($"{user}'s role is the same or higher than bot!!");
                    }

                await ReplyAsync($"All Users's name changed to {msg}");
            }
            else
            {
                await ReplyAsync("Permission denied!");
            }
        }

        [Command("setname")]
        [Summary("Change user nickname")]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        public async Task SetName(IGuildUser user, [Remainder] string nickName)
        {
            if (nickName == null) return;

            if (!(Context.User is SocketGuildUser userSend)
                || !userSend.GuildPermissions.ManageNicknames)
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }

            await user.ModifyAsync(c => c.Nickname = nickName);
            var builder = new EmbedBuilder()
                .WithTitle("Name changed")
                .WithDescription(
                    $"{user}'s name has been changed to {nickName}!")
                .WithCurrentTimestamp()
                .WithColor(new Color(Utils.RandomColor(), Utils.RandomColor(), Utils.RandomColor()));
            await ReplyAsync(embed: builder.Build());
        }

        [Command("create")]
        [Summary("Create a new role")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task CreateRole([Remainder] string role)
        {
            if (!(Context.User is SocketGuildUser userSend) || !userSend.GuildPermissions.ManageRoles)
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }
            if (Context.IsPrivate || role == null) return;
            await Context.Guild.CreateRoleAsync(role, GuildPermissions.None, null, false, false);
        }

        [Command("revoke")]
        [Summary("Revoke someone role. Need admin perm & bot manage role perm")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RevokeRole(SocketGuildUser user, SocketRole role)
        {
            if (Context.IsPrivate || role == null) return;

            var builder = new EmbedBuilder();

            builder.WithTitle("Logged Information")
                .AddField("User", $"{user.Mention}")
                .AddField("Moderator", $"{Context.User.Mention}")
                .WithDescription(
                    $"{role} does not exist from {user}")
                .WithFooter($"{Context.User.Username}", Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(54, 57, 62));

            if (!(Context.User is SocketGuildUser userSend) || !userSend.GuildPermissions.ManageRoles ||
                !Utils.CanInteractRole(userSend, role))
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }

            if (user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                builder.WithDescription($"{role.Mention} has been revoke from {user.Mention} by {Context.User.Mention}");
            }
            await ReplyAsync(embed: builder.Build());
        }

        [Command("Nuke")] // Command declaration
        [Summary("Nuke the channel")] // command summary
        [RequireUserPermission(GuildPermission.ManageChannels)]
        // Needed User Permissions //
        public async Task Nuke() // command async task that takes in a parameter (remainder represents a space between the command and the parameter)
        {
            var channel = Context.Guild.GetChannel(Context.Channel.Id);
            await channel.DeleteAsync();
            var newChannel = await Context.Guild.CreateTextChannelAsync(channel.Name);
            await newChannel.SendMessageAsync("Nuked!");
        }

        [Command("lock", true)]
        [Summary("lock channel")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task LockChannel()
        {
            if (!(Context.Channel is SocketGuildChannel channel)) return;
            if (!(Context.User is SocketGuildUser userSend)
                || !userSend.GuildPermissions.ManageChannels)
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }
            var role = Context.Guild.EveryoneRole;
            await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(channel)
                .Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow));
            var builder = new EmbedBuilder()
                .WithDescription("`Channel locked`");
            await ReplyAsync(null, false, builder.Build());
        }

        [Command("unlock", true)]
        [Summary("unlock channel")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Unlock()
        {
            if (!(Context.Channel is SocketGuildChannel channel)) return;
            if (!(Context.User is SocketGuildUser userSend)
                || !userSend.GuildPermissions.ManageChannels)
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }
            var role = Context.Guild.EveryoneRole;
            await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.InheritAll);
            var builder = new EmbedBuilder()
                .WithDescription("`Channel unlocked`");
            await ReplyAsync(null, false, builder.Build());
        }

        [Command("lock", true)]
        [Summary("lock channel")]
        [Alias("rl")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task LockChannelRemote(SocketGuildChannel channel)
        {
            if (!(Context.User is SocketGuildUser userSend)
                || !userSend.GuildPermissions.ManageChannels)
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }
            var role = Context.Guild.EveryoneRole;
            await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(channel)
                .Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow));
            var builder = new EmbedBuilder()
                .WithDescription("`Channel locked`");
            if (!(Context.Client.GetChannel(channel.Id) is SocketTextChannel channelId)) return;
            await channelId.SendMessageAsync(null, false, builder.Build());
        }

        [Command("giverole")]
        [Summary("Grant someone role. Need admin perm & bot manage role perm")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRole(SocketGuildUser user, SocketRole role)
        {
            if (Context.IsPrivate || role == null) return;
            if (!(Context.User is SocketGuildUser userSend) || !userSend.GuildPermissions.ManageRoles ||
                !Utils.CanInteractRole(userSend, role))
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }
            var builder = new EmbedBuilder();

            builder.WithTitle("Logged Information")
                .AddField("User", $"{user.Mention}")
                .AddField("Moderator", $"{Context.User.Mention}")
                .WithDescription(
                    $"{user.Mention} already has the role {role.Mention}")
                .WithFooter($"{Context.User.Username}", Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(54, 57, 62));

            if (!user.Roles.Contains(role))
            {
                await user.AddRoleAsync(role);
                builder.WithDescription($"{user.Mention} has been granted {role.Mention} by {Context.User.Mention}");
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("mute")]
        [Summary("Mute someone. Need admin perm & bot manage role perm")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Mute(IGuildUser user)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Logged Information")
                .AddField("User", $"{user.Mention}")
                .AddField("Moderator", $"{Context.User.Mention}")
                .AddField("Other Information", "Violate rules / Personal")
                .WithDescription(
                    $"This User has been muted from {Context.Guild.Name} by {Context.User.Username}")
                .WithFooter($"{Context.User.Username}", Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(54, 57, 62));

            if (!(Context.User is SocketGuildUser userSend)
                || !(userSend.GuildPermissions.ManageRoles
                     || userSend.GuildPermissions.KickMembers
                     || user.GuildPermissions.BanMembers
                     || user.GuildPermissions.ManageRoles
                     || Utils.CanInteractUser(userSend, (SocketGuildUser)user)))
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }

            var mutedRole = Context.Guild.Roles.FirstOrDefault(t => t.Name.ToLower().Equals("muted"));
            if (mutedRole == null)
            {
                var roleCreation =
                    await Context.Guild.CreateRoleAsync("Muted", Utils.MutedPermissions, null, false, false);
                try
                {
                    await user.AddRoleAsync(roleCreation);
                    await Context.Channel.SendMessageAsync(null, false, builder.Build());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                await user.AddRoleAsync(mutedRole);
                await ReplyAsync(embed: builder.Build());
            }
        }

        [Command("unmute")]
        [Summary("Unmute someone. Need admin perm & bot manage role perm")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnMute(SocketGuildUser user)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("muted"));

            if (role == null || !(Context.User is SocketGuildUser userSend)
                  || !(userSend.GuildPermissions.KickMembers
                  || user.GuildPermissions.BanMembers
                  || user.GuildPermissions.ManageRoles))
            {
                await Utils.SendInvalidPerm(Context.User, Context.Channel);
                return;
            }

            await user.RemoveRoleAsync(role);
            var builder = new EmbedBuilder()
                .WithTitle("Logged Information")
                .AddField("User", $"{user.Mention}")
                .AddField("Moderator", $"{Context.User.Mention}")
                .AddField("Other Information", "Released from jail!!")
                .WithDescription(
                    $"This User has been unmuted from {Context.Guild.Name} by {Context.User.Username}")
                .WithFooter($"{Context.User.Username}", Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .WithColor(new Color(54, 57, 62));
            await ReplyAsync(embed: builder.Build());
        }
    }
}