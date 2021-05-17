using ADB.Config;
using ADB.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ADB.Handlers
{
    public class CommandHandler
    {
        public static CommandHandler instance;

        public readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private List<CommandInfo> commandsModels = new List<CommandInfo>();

        /* Get Everything we need from DI. */

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            HookEvents();
        }

        /* Initialize the CommandService. */

        public async Task InitializeAsync()
        {
            instance = this;

            SetupBot.LogPopup("[info] Starting Advanced Bot");

            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);

            foreach (ModuleInfo mf in _commands.Modules)
            {
                if (mf.Name != "Help")
                {
                    foreach (CommandInfo ci in mf.Commands)
                    {
                        commandsModels.Add(ci);
                    }
                }
            }
            SetupBot.LogPopup($"[info] Added {_commands.Modules.Count()} Module");
        }
        public void HookEvents()
        {
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        /* When a MessageRecived Event triggers from the Client.
              Handle the message here. */

        private Task HandleCommandAsync(SocketMessage socketMessage)
        {
            /* Create the CommandContext for use in modules. */

            var argPos = 0;
            //Check that the message is a valid command, ignore everything we don't care about. (Private message, messages from other Bots, Etc)
            if (!(socketMessage is SocketUserMessage message) || message.Author.IsBot || message.Author.IsWebhook || message.Channel is IPrivateChannel)
                return Task.CompletedTask;

            /* Check that the message has our Prefix */
            if (!message.HasStringPrefix(AppSettings.CommandPrefix, ref argPos))
                return Task.CompletedTask;

            SocketCommandContext context = null;

            try
            {
                context = new SocketCommandContext(_client, socketMessage as SocketUserMessage);
            }
            catch
            {
                return null;
            }

            SetupBot.LogPopup("[User Issued]\t" + socketMessage.Author.Username + "\n[Command]\t" + socketMessage.Content);

            /* Check if the channel ID that the message was sent from is in our Config - Blacklisted Channels. */
            if (AppSettings.BlacklistedChannels != null)
            {
                var blacklistedChannelCheck = from a in AppSettings.BlacklistedChannels
                                              where a == context.Channel.Id
                                              select a;
                var blacklistedChannel = blacklistedChannelCheck.FirstOrDefault();

                /* If the Channel ID is in the list of blacklisted channels. Ignore the command. */
                if (blacklistedChannel == context.Channel.Id)
                {
                    return Task.CompletedTask;
                }
                else
                {
                    var result = _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);

                    /* If everything worked fine, command will run. */
                    return result;
                }
            }
            else
            {
                var result = _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);

                /* If everything worked fine, command will run. */
                return result;
            }
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            /* command is unspecified when there was a search failure (command not found); we don't care about these errors */
            if (!command.IsSpecified)
                return;

            /* the command was succesful, we don't care about this result, unless we want to log that a command succeeded. */
            if (result.IsSuccess)
            {
                SetupBot.LogPopup("[Command Issued]\t" + command.Value.Name);
                return;
            }

            /* the command failed, let's notify the user that something happened. */
            await context.Channel.SendMessageAsync($"error: {result}");
        }

        /*Used whenever we want to log something to the Console.
            Todo: Hook in a Custom LoggingService. */

        private Task LogAsync(LogMessage log)
        {
            SetupBot.LogPopup("[Issued]\t" + log.Message);
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        public async void DisconnectBotAsync()
        {
            if (DiscordService.botRunning)
            {
                SetupBot.LogPopup("Disconnecting..");
                await _client.StopAsync();
                DiscordService.botRunning = false;
                SetupBot.LogPopup("Disconnected!");
            }
        }

        //https://stackoverflow.com/questions/49663096/discord-net-c-sharp-1-0-2-how-to-send-messages-to-specific-channels
        public async Task SendMessageToChannel(string channelName, string msg)
        {
            SocketGuildChannel[] sgc = _client.GetGuild(709709711712452648).Channels.ToArray();
            foreach (SocketGuildChannel c in sgc)
            {
                if (c.Name == channelName)
                {
                    var chnl = c as IMessageChannel;
                    await chnl.SendMessageAsync(msg);
                }
            }
        }

        public async Task SendCommandToBot(string guildName, string channelName, string command)
        {
            SocketGuild guild = null;
            SocketGuildChannel channel = null;

            foreach (SocketGuild sg in _client.Guilds)
            {
                if (sg.Name == guildName)
                {
                    guild = sg;
                    break;
                }
            }

            foreach (SocketGuildChannel sgc in guild.Channels)
            {
                if (sgc.Name == channelName)
                {
                    channel = sgc;
                    break;
                }
            }

            if (guild == null || channel == null)
            {
                SetupBot.LogPopup("Guild or Channel doesn't exist");
                return;
            }

            try
            {
                await guild.GetTextChannel(channel.Id).SendMessageAsync(command);
                SetupBot.LogPopup($"[{guildName}]>[{channelName}]\t" + command);
            }
            catch
            {
                SetupBot.LogPopup("Something went wrong!");
            }
        }
    }
}