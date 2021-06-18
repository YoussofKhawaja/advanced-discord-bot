using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordClient.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Victoria;

namespace DiscordClient.Services
{
    public class DiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly LavaLinkAudio _audioService;

        public DiscordService()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _audioService = _services.GetRequiredService<LavaLinkAudio>();

            SubscribeLavaLinkEvents();
            SubscribeDiscordEvents();
            SubscribeDiscordUserEvents();
        }

        /* Initialize the Discord Client. */

        private async Task AnnounceJoinedUser(SocketGuildUser user)
        {
            await (user.Guild.DefaultChannel).SendMessageAsync("hey");
            return;
        }

        private async Task AnnounceLeftUser(SocketGuildUser user)
        {
            await (user.Guild.DefaultChannel).SendMessageAsync("bye");
            return;
        }

        public async Task InitializeAsync()
        {
            await _client.SetGameAsync(Bot.AppData.GameStatus);

            await _client.LoginAsync(TokenType.Bot, Bot.AppData.Token);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();

            //keep bot running
            //await Task.Delay(-1);
        }

        /* Hook Any Client Events Up Here. */

        private void SubscribeLavaLinkEvents()
        {
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackEnded += _audioService.TrackEnded;
        }

        public void SubscribeDiscordEvents()
        {
            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;
        }

        public async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;
        }

        private void SubscribeDiscordUserEvents()
        {
            #region spam & filter

            _client.MessageReceived += MessageReceivedAsync;

            #endregion spam & filter

            #region log

            _client.MessageDeleted += OnMessageDeleted;
            _client.MessageUpdated += OnMessageUpdated;

            _client.UserJoined += AnnounceJoinedUser;
            _client.UserLeft += AnnounceLeftUser;
            _client.UserBanned += OnUserBanned;
            _client.UserUnbanned += OnUserUnbanned;

            _client.RoleCreated += OnRoleCreated;
            _client.RoleUpdated += OnRoleUpdated;
            _client.RoleDeleted += OnRoleDeleted;

            _client.ChannelCreated += OnChannelCreated;
            _client.ChannelUpdated += OnChannelUpdated;
            _client.ChannelDestroyed += OnChannelDestroyed;

            _client.JoinedGuild += JoinGuild;

            #endregion log
        }

        public async Task JoinGuild(SocketGuild guild)
        {
            var embed = new EmbedBuilder();

            embed.WithTitle("Welcome to DiscordClient bot")
                .WithDescription($"Thanks for adding! If you wanna know what can I do, use `{Bot.AppData.Prefix}help` command! \n\n Have an idea for command? Maybe found bug? Or just help bot grow? Please join to my Discord Support Server! \nhttps://discord.gg/RN4EPD34")
                .WithFooter("DiscordClient Bot");

            await guild.TextChannels.First().SendMessageAsync(embed: embed.Build());
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageCache, SocketMessage messageAfter, ISocketMessageChannel messageChannel)
        {
        }

        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> messageCache, ISocketMessageChannel messageChannel)
        {
        }

        private async Task OnChannelCreated(SocketChannel channel)
        {
        }

        private async Task OnChannelUpdated(SocketChannel channelBefore, SocketChannel channelAfter)
        {
        }

        private async Task OnChannelDestroyed(SocketChannel channel)
        {
            
        }

        private async Task OnRoleCreated(SocketRole role)
        {
        }

        private async Task OnRoleUpdated(SocketRole roleBefore, SocketRole roleAfter)
        {
        }

        private async Task OnRoleDeleted(SocketRole role)
        {
        }

        private async Task OnUserUnbanned(SocketUser userBanned, SocketGuild guild)
        {
        }

        private async Task OnUserBanned(SocketUser userBanned, SocketGuild guild)
        {
            var Guild = guild;
            ulong logChannelID = 0;
            foreach (SocketGuildChannel sg in Guild.Channels)
            {
                if (sg.Name == "log")
                {
                    logChannelID = sg.Id;
                    break;
                }
            }
            if (logChannelID == 0)
                return;

            var banAsync = await guild.GetBanAsync(userBanned);
            var reason = banAsync.Reason;
            if (_client.GetChannel(logChannelID) is IMessageChannel logChannel)
            {
                await logChannel.SendMessageAsync($"{userBanned.Mention} has been banned from {guild.Name} " +
                                                  $"at {DateTime.Now} with reason : {reason}!");
            }
        }

        private async Task _client_UserIsTyping(SocketUser User, ISocketMessageChannel arg2)
        {
        }

        /* Used when the Client Fires the ReadyEvent. */

        private async Task ReadyAsync()
        {
            try
            {
                await _lavaNode.ConnectAsync();
                await _client.SetGameAsync(Bot.AppData.GameStatus);

                //bot fully running
                Bot.IsRunning = true;

                //SetupBot.LogPopup("[State]\t" + "Active");
            }
            catch (Exception ex)
            {
                //SetupBot.LogPopup($"[{ex.Source}]\t" + ex.Message);
            }
        }

        /*Used whenever we want to log something to the Console.
            Todo: Hook in a Custom LoggingService. */

        private async Task LogAsync(LogMessage logMessage)
        {
        }

        /* Configure our Services for Dependency Injection. */

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 100, AlwaysDownloadUsers = true }))
                    .AddSingleton<CommandService>()
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<LavaNode>()
                    .AddSingleton(new LavaConfig())
                    .AddSingleton<LavaLinkAudio>()
                    .AddSingleton<InteractiveService>()
                    .AddSingleton<EmbedService>()
                    .BuildServiceProvider();
        }
    }
}