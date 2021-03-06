using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordClient.Services;
using System.Threading.Tasks;

namespace DiscordClient.Modules
{
    [Summary("Music")]
    public class Music : ModuleBase<SocketCommandContext>
    {
        /* Get our AudioService from DI */
        public LavaLinkAudio AudioService { get; set; }

        /* All the below commands are ran via Lambda Expressions to keep this file as neat and closed off as possible.
              We pass the AudioService Task into the section that would normally require an Embed as that's what all the
              AudioService Tasks are returning. */

        [Command("Join")]
        public async Task JoinAndPlay()
            => await ReplyAsync(embed: await AudioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("Leave")]
        public async Task Leave()
            => await ReplyAsync(embed: await AudioService.LeaveAsync(Context.Guild));

        [Command("Play")]
        public async Task Play([Remainder] string search)
            => await ReplyAsync(embed: await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search));

        [Command("Stop")]
        public async Task Stop()
            => await ReplyAsync(embed: await AudioService.StopAsync(Context.Guild));

        [Command("queue")]
        public async Task List()
            => await ReplyAsync(embed: await AudioService.ListAsync(Context.Guild));

        [Command("Skip")]
        public async Task Skip()
            => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild));

        [Command("Volume")]
        public async Task Volume(int volume)
            => await ReplyAsync(await AudioService.SetVolumeAsync(Context.Guild, volume));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await AudioService.PauseAsync(Context.Guild));

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync(await AudioService.ResumeAsync(Context.Guild));

        [Command("Loop")]
        public async Task Loop()
        {
            if (AudioService.loop)
            {
                AudioService.loop = false;
                await ReplyAsync("Loop stopped");
            }
            else
            {
                AudioService.loop = true;
                await ReplyAsync("Loop started");
            }
        }

        [Command("Lyrics")]
        public async Task Lyrics()
        {
            await ReplyAsync(embed: await AudioService.Lyrics((SocketGuildUser)Context.User));
        }

        [Command("NowPlaying")]
        public async Task Playing()
        {
            await ReplyAsync(embed: await AudioService.NowPlaying((SocketGuildUser)Context.User));
        }

        [Command("Art")]
        public async Task Art()
        {
            await ReplyAsync(embed: await AudioService.GetTrackArtt((SocketGuildUser)Context.User));
        }
    }
}