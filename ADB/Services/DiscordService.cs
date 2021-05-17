using ADB.Config;
using ADB.Handlers;
using ADB.Helpers.embed;
using ADB.Models;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Victoria;

namespace ADB.Services
{
    public class DiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly LavaLinkAudio _audioService;
        private bool firstStartup = false;

        public static bool botRunning = false;

        public DiscordService()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _audioService = _services.GetRequiredService<LavaLinkAudio>();
            SetUpFilesystem();

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
            await _client.LoginAsync(TokenType.Bot, AppSettings.token);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();

            await Task.Delay(-1);
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

            embed.WithTitle("Welcome to ADB bot")
                .WithDescription($"Thanks for adding! If you wanna know what can I do, use `{AppSettings.CommandPrefix}help` command! \n\n Have an idea for command? Maybe found bug? Or just help bot grow? Please join to my Discord Support Server! \nhttps://discord.gg/RN4EPD34")
                .WithFooter("ADB Bot");

            await guild.TextChannels.First().SendMessageAsync(embed: embed.Build());
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageCache, SocketMessage messageAfter, ISocketMessageChannel messageChannel)
        {
            if (!(messageAfter is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            var user = messageAfter.Author;
            if (messageCache.HasValue)
            {
                var content = messageCache.Value.Content;

                var chnl = messageChannel as SocketGuildChannel;
                var Guild = chnl.Guild;
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

                if (_client.GetChannel(logChannelID) is IMessageChannel logChannel
                    && messageCache.HasValue
                    && !messageCache.Value.IsPinned)
                {
                    await EventReplyUtils.MessageUpdatedEmbed(user, logChannel, messageChannel, content, messageAfter);
                }
            }
        }

        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> messageCache, ISocketMessageChannel messageChannel)
        {
            if (!messageCache.HasValue) return;
            var message = ((SocketUserMessage)messageCache.Value);
            if (message.Source != MessageSource.User) return;
            var user = ((SocketMessage)messageCache.Value).Author;
            var content = messageCache.Value.Content;

            var chnl = messageChannel as SocketGuildChannel;
            var Guild = chnl.Guild;
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

            if (_client.GetChannel(logChannelID) is IMessageChannel logChannel)
            {
                await EventReplyUtils.MessageDeletedEmbed(user, logChannel, messageChannel, content);
            }
        }

        private void SetUpServersXML()
        {
            Console.WriteLine("Created Servers.xml");

            string serversPath = $"{Globals.path}Servers.xml";

            XDocument xml = XDocument.Load(serversPath);

            //Get the ids of every server already written in Servers.xml
            var ids = (from element in xml.Root.Descendants("server") select ulong.Parse(element.Attribute("id").Value)).ToList();

            foreach (SocketGuild guild in _client.Guilds)
            {
                if (!ids.Contains(guild.Id))
                {
                    xml.Root.Add(new XElement("server",
                                 new XAttribute("id", guild.Id),
                                 new XElement("CustomColor", false),
                                 new XElement("InVoiceChannel", null),
                                 new XElement("PrivateChannel", null)));
                }
            }

            xml.Save(serversPath);
        }

        private void SetUpFilesystem()
        {
            Console.WriteLine("Setting up filesystem...");
            if (!Directory.Exists(Globals.path))
                Directory.CreateDirectory(Globals.path);

            List<string> filenames = new List<string>()
            {
                "profiles.xml",
                "tags.xml"
            };

            foreach (string file in filenames)
            {
                if (!File.Exists(Globals.path + file))
                {
                    CreateEmptyXML(file);
                    Console.WriteLine("Created " + file);
                }
            }

            if (!File.Exists(Globals.path + "Servers.xml"))
            {
                CreateEmptyXML("Servers.xml");
                firstStartup = true;
            }
        }

        private async Task OnReady()
        {
            if (firstStartup)
            {
                SetUpServersXML();
                firstStartup = false;
            }
            await Task.CompletedTask;
        }

        private void CreateEmptyXML(string name)
        {
            using (StreamWriter sw = File.CreateText($"{Globals.path}{name}"))
            {
                sw.WriteLine("<root></root>");
            }
        }

        private async Task OnChannelCreated(SocketChannel channel)
        {
            var chnl = channel as SocketGuildChannel;
            if (chnl == null)
                return;

            var Guild = chnl.Guild;
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

            if (_client.GetChannel(logChannelID) is IMessageChannel logChannel)
                await EventReplyUtils.ChannelCreatedEmbed(channel, logChannel);
        }

        private async Task OnChannelUpdated(SocketChannel channelBefore, SocketChannel channelAfter)
        {
            var chnl = channelBefore as SocketGuildChannel;
            var Guild = chnl.Guild;
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

            if (!(_client.GetChannel(logChannelID) is IMessageChannel logChannel)) return;
            // First, we want both SocketChannels to be converted to SocketGuildChannels
            var after = channelAfter as SocketGuildChannel;

            if (!(channelBefore is SocketGuildChannel before)) return;
            var differences = after?.PermissionOverwrites.Except(before.PermissionOverwrites); // Go through the PermissionOverwrites to extract only the differences
            if (differences != null)
            {
                foreach (var difference in differences) // Loop over every difference
                {
                    Console.WriteLine($"\n{difference.Permissions} test\n");
                    Console.WriteLine(
                        $"{difference.TargetType}: {difference.TargetId}"); // Let's you know what the TargetType is (a User or role) followed by their ID
                    Console.WriteLine("Allowed: " +
                                      string.Join(" ",
                                          difference.Permissions
                                              .ToAllowList())); // Get all the permissions that were changed to allow
                    Console.WriteLine("Denied: " +
                                      string.Join(" ",
                                          difference.Permissions
                                              .ToDenyList())); // Get all the permissions that were changed to denied
                }
            }
            if (((SocketGuildChannel)channelBefore).Name != ((SocketGuildChannel)channelAfter).Name)
            {
                await EventReplyUtils.ChannelNameUpdatedEmbed(channelBefore, channelAfter, logChannel);
            }
        }

        private async Task OnChannelDestroyed(SocketChannel channel)
        {
            var chnl = channel as SocketGuildChannel;
            var Guild = chnl.Guild;
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

            if (_client.GetChannel(logChannelID) is IMessageChannel logChannel)
            {
                await EventReplyUtils.ChannelDeletedEmbed(channel, logChannel);
            }
        }

        private async Task OnRoleCreated(SocketRole role)
        {
            var Guild = role.Guild;
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

            if (_client.GetChannel(logChannelID) is IMessageChannel logChannel)
            {
                await EventReplyUtils.RoleCreatedEmbed(role, logChannel);
            }
        }

        private async Task OnRoleUpdated(SocketRole roleBefore, SocketRole roleAfter)
        {
            var Guild = roleBefore.Guild;
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

            if (!(_client.GetChannel(780701073711038474) is IMessageChannel logChannel)) return;
            if (!Equals(roleBefore.Permissions, roleAfter.Permissions))
            {
                await EventReplyUtils.RolePermUpdatedEmbed(roleBefore, roleAfter, logChannel);
            }
            if (!Equals(roleBefore.Name, roleAfter.Name))
            {
                await EventReplyUtils.RoleNameUpdatedEmbed(roleBefore, roleAfter, logChannel);
            }
            if (!Equals(roleBefore.Color, roleAfter.Color))
            {
                await EventReplyUtils.RoleColorUpdatedEmbed(roleBefore, roleAfter, logChannel);
            }
        }

        private async Task OnRoleDeleted(SocketRole role)
        {
            var Guild = role.Guild;
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

            if (_client.GetChannel(logChannelID) is IMessageChannel logChannel)
            {
                await EventReplyUtils.RoleRemovedEmbed(role, logChannel);
            }
        }

        private async Task OnUserUnbanned(SocketUser userBanned, SocketGuild guild)
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

            if (_client.GetChannel(logChannelID) is IMessageChannel logChannel)
            {
                await logChannel.SendMessageAsync($"{userBanned.Mention} has been unbanned from {guild.Name} " +
                                                  $"at {DateTime.Now}!");
            }
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
                await _client.SetGameAsync(AppSettings.GameStatus);

                //bot fully running
                botRunning = true;

                SetupBot.LogPopup("[State]\t" + "Active");
            }
            catch (Exception ex)
            {
                SetupBot.LogPopup($"[{ex.Source}]\t" + ex.Message);
                await LoggingService.LogInformationAsync(ex.Source, ex.Message);
            }
        }

        /*Used whenever we want to log something to the Console.
            Todo: Hook in a Custom LoggingService. */

        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }

        /* Configure our Services for Dependency Injection. */

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                    .AddHttpClient()
                    .AddTransient<HttpClient>(x => x.GetService<IHttpClientFactory>().CreateClient("default"))

                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 100, AlwaysDownloadUsers = true }))
                    .AddSingleton<CommandService>()
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<LavaNode>()
                    .AddSingleton(new LavaConfig())

                    .AddSingleton<LavaLinkAudio>()

                    .AddSingleton<InteractiveService>()

                    .AddSingleton<BotService>()
                    .AddSingleton<EmbedService>()
                    .BuildServiceProvider();
        }
    }
}