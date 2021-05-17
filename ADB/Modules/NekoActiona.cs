﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NekosSharp;

namespace ADB.Modules
{
    [Summary("NekoActions Commands")]
    public class NekoActiona : ModuleBase<SocketCommandContext>
    {
        private NekoClient _nekoClient;

        public NekoActiona()
        {
            _nekoClient = new NekoClient("ADB");
            _nekoClient.LogType = LogType.None;
        }

        [Command("slap")]
        public async Task NekoSlap(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.SlapGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** slaps **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            embed.WithCurrentTimestamp();
            await ReplyAsync("", false, embed.Build());
        }

        [Command("poke")]
        public async Task NekoPoke(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.PokeGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** pokes **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("hug")]
        public async Task NekoHug(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.HugGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** is getting hugged by **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("kiss")]
        public async Task NekoKiss(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.KissGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** kisses **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("pat")]
        public async Task NekoPat(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.PatGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** pats **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("tickle")]
        public async Task NekoTickle(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.TickleGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** tickles **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("feed")]
        public async Task NekoFeed(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.FeedGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** feeds **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("cuddle")]
        public async Task NekoCuddle(IGuildUser user = null)
        {
            user ??= (IGuildUser)Context.User;

            Request req = await _nekoClient.Action_v3.CuddleGif();

            if (!req.Success)
            {
                await ReplyAsync("Sorry something went wrong, If this happens more than a few times please message my developer");
                return;
            }

            await Context.Message.DeleteAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"**{Context.User.Username}** cuddles **{user.Username}**");
            embed.WithImageUrl(req.ImageUrl);
            await ReplyAsync("", false, embed.Build());
        }
    }
}