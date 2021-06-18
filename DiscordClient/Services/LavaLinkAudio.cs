using Discord;
using Discord.WebSocket;
using DiscordClient.Handlers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Rest;

namespace DiscordClient.Services
{
    public sealed class LavaLinkAudio
    {
        private readonly LavaNode _lavaNode;
        private readonly EmbedService _embedService;
        public bool loop;
        public SearchResponse currentSong;
        public SocketGuildUser _user;
        public IGuild _guild;
        /* public LavaLinkAudio(EmbedService embedService)
         {
             _embedService = embedService;
         }*/

        public LavaLinkAudio(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        public async Task<Embed> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild))
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
            }

            if (voiceState.VoiceChannel is null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
            }
        }

        /*This is ran when a user uses either the command Join or Play
            I decided to put these two commands as one, will probably change it in future.
            Task Returns an Embed which is used in the command call.. */

        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query)
        {
            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }

            //Check the guild has a player available.
            if (!_lavaNode.HasPlayer(guild))
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", "I'm not connected to a voice channel.");
            }

            try
            {
                //Get the player for that guild.
                var player = _lavaNode.GetPlayer(guild);

                //Find The Youtube Track the User requested.
                LavaTrack track;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);

                //my add
                currentSong = search;
                _user = user;
                _guild = guild;

                //If we couldn't find anything, tell the user.
                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                }

                //Get the first track from the search results.
                //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
                track = search.Tracks.FirstOrDefault();

                //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", Color.Blue);
                }

                //Player was not playing anything, so lets play the requested track.
                await player.PlayAsync(track);
                return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}", Color.Blue);
            }

            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
            }
        }

        /*This is ran when a user uses the command Leave.
            Task Returns an Embed which is used in the command call. */

        public async Task<Embed> LeaveAsync(IGuild guild)
        {
            try
            {
                //Get The Player Via GuildID.
                var player = _lavaNode.GetPlayer(guild);

                //if The Player is playing, Stop it.
                if (player.PlayerState is PlayerState.Playing)
                {
                    loop = false;
                    await player.StopAsync();
                }

                //Leave the voice channel.
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                return await EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing moosik.", Color.Blue);
            }
            //Tell the user about the error so they can report it back to us.
            catch (InvalidOperationException ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message);
            }
        }

        /*This is ran when a user uses the command List
            Task Returns an Embed which is used in the command call. */

        public async Task<Embed> ListAsync(IGuild guild)
        {
            try
            {
                /* Create a string builder we can use to format how we want our list to be displayed. */
                var descriptionBuilder = new StringBuilder();

                /* Get The Player and make sure it isn't null. */
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{Bot.AppData.Prefix}Help for info on how to use the bot.");

                if (player.PlayerState is PlayerState.Playing)
                {
                    /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
                        In this situation we simply return an embed that displays the current track instead. */
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", "Nothing Else Is Queued.", Color.Blue);
                    }
                    else
                    {
                        /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
                         *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                            This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                        var trackNum = 2;
                        foreach (LavaTrack track in player.Queue)
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
                            trackNum++;
                        }
                        return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue);
                    }
                }
                else
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now. If this is an error, Please Contact Draxis.");
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }
        }

        /*This is ran when a user uses the command Skip
            Task Returns an Embed which is used in the command call. */

        public async Task<Embed> SkipTrackAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                /* Check if the player exists */
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{Bot.AppData.Prefix}Help for info on how to use the bot.");
                /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
                     User is expected to use the Stop command if they're only wanting to skip the current song. */
                if (player.Queue.Count < 1)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only One or No songs currently playing." +
                        $"\n\nDid you mean {Bot.AppData.Prefix}Stop?");
                }
                else
                {
                    try
                    {
                        /* Save the current song for use after we skip it. */
                        var currentTrack = player.Track;
                        /* Skip the current song. */
                        await player.SkipAsync();
                        return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue);
                    }
                    catch (Exception ex)
                    {
                        return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
            }
        }

        /*This is ran when a user uses the command Stop
            Task Returns an Embed which is used in the command call. */

        public async Task<Embed> StopAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{Bot.AppData.Prefix}Help for info on how to use the bot.");

                /* Check if the player exists, if it does, check if it is playing.
                     If it is playing, we can stop.*/
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
            }
        }

        /*This is ran when a user uses the command Volume
            Task Returns a String which is used in the command call. */

        public async Task<string> SetVolumeAsync(IGuild guild, int volume)
        {
            if (volume > 150 || volume <= 0)
            {
                return $"Volume must be between 1 and 150.";
            }
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volume);
                return $"Volume has been set to {volume}.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> PauseAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"There is nothing to pause.";
                }

                await player.PauseAsync();
                return $"**Paused:** {player.Track.Title}, what a bamboozle.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                }

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs args)
        {
            if (loop)
            {
                await PlayAsync(_user, _guild, currentSong.Tracks[currentSong.Tracks.Count - 1].Url);
                await args.Player.TextChannel.SendMessageAsync(
                    embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{currentSong.Tracks[currentSong.Tracks.Count - 1].Title}]({currentSong.Tracks[currentSong.Tracks.Count - 1].Url})", Color.Blue));
            }

            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                //await args.Player.TextChannel.SendMessageAsync("Playback Finished.");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue));
        }

        public async Task<Embed> Lyrics(SocketGuildUser user)
        {
            //if the player is not in that guild, let em know
            if (!_lavaNode.HasPlayer(user.Guild))
            {
                return await _embedService.CreateBasicEmbedAsync("Lyrics", "No player in guild");
            }

            //Get the player and the track
            var player = _lavaNode.GetPlayer(user.Guild);
            var track = player.Track;

            //If there is no song playing, tell em
            if (player.PlayerState != PlayerState.Playing)
            {
                return await _embedService.CreateBasicEmbedAsync("Lyrics", "No song is playing.");
            }

            try
            {
                //TODO Learn regex, this is horrifying
                //Remove parentheses bs from the title
                var withoutParentheses = track.Title.Split("(")[0];
                var withoutFeat = withoutParentheses.Split("feat")[0];
                var withoutft = withoutFeat.Split("ft")[0];
                var withoutPipe = withoutft.Split("|")[0];

                //Create a dummy track for the purpose of searching
                var dummyTrack = new LavaTrack(track.Hash, track.Id, withoutPipe, " ", track.Url, TimeSpan.Zero, 0,
                    false, false);

                //Get the lyrics from the host
                var lyrics = await dummyTrack.FetchLyricsFromOVHAsync();

                //Create embed builder for lyrics
                var embedBuilder = new EmbedBuilder();

                //if the lyrics are longer than discord supports return an embed saying that
                if (lyrics.Length >= 6000)
                {
                    return await _embedService.CreateBasicEmbedAsync("Lyrics", "Lyrics are too long to post, blame discord.");
                }

                //Calculate iterations as the lyrics length / 1000
                var iterations = (int)Math.Ceiling(lyrics.Length / 1024.0);

                //Iterate through the iterations and split the embed up
                for (var index = 0; index < iterations; index++)
                {
                    //Add the section
                    embedBuilder.AddField($"Section {index + 1}",
                        index == iterations - 1
                            ? lyrics.Substring(index * 1024)
                            : lyrics.Substring(index * 1024, 1024));
                }

                //add the author to the embed
                embedBuilder.Author = new EmbedAuthorBuilder
                {
                    IconUrl = await track.FetchArtworkAsync(),
                    Name = $"Lyrics for {track.Title}"
                };

                //add the current timestamp
                embedBuilder.WithCurrentTimestamp();

                //Build the embed
                return embedBuilder.Build();
            }
            catch (Exception exception)
            {
                return await _embedService.CreateBasicEmbedAsync("lyrics",
                    "Failed to retrieve lyrics by either an error, or the source doesn't have any\n" +
                    exception.Message);
            }
        }

        public async Task<Embed> NowPlaying(SocketGuildUser user)
        {
            //get the guild
            IGuild guild = user.Guild;

            //check if the guild has a player
            if (!_lavaNode.HasPlayer(guild))
            {
                return await _embedService.CreateBasicEmbedAsync("Playing",
                    "The guild does not currently have a player");
            }

            //get the player
            var player = _lavaNode.GetPlayer(user.Guild);

            //if the guild isn't playing anything return an embed that lets them know that
            if (player.PlayerState != PlayerState.Playing)
            {
                return await _embedService.CreateBasicEmbedAsync("Music Artwork", "No songs playing");
            }

            try
            {
                //get the track
                var track = player.Track;
                var artwork = await track.FetchArtworkAsync(); //get the artwork for the track
                //get runtime in seconds
                var progress = track.Position.Divide(track.Duration);

                //TODO calculate ticks based on resolution of the image
                var ticks = 54;
                var position = (int)(progress * ticks);

                //Create an output string using the string builder
                var output = new StringBuilder();

                //iterate through each index in the ticks
                for (var index = 0; index < ticks; index++)
                {
                    output.Append(index == position ? ":blue_circle:" : "-");
                }

                //create embed builder
                var builder = new EmbedBuilder
                {
                    ImageUrl = artwork,
                    Author = new EmbedAuthorBuilder { Name = $"{track.Title} - {track.Author}", IconUrl = artwork }
                };

                builder.WithCurrentTimestamp();
                builder.Description = output.ToString();
                return builder.Build();
            }
            catch
            {
                return await _embedService.CreateBasicEmbedAsync("Music Artwork",
                    "Exception occured while trying to retrieve artwork");
            }
        }

        public async Task<Embed> GetTrackArtt(SocketGuildUser user)
        {
            //get the guild
            IGuild guild = user.Guild;

            //if the guild doesn't have a player
            if (!_lavaNode.HasPlayer(guild))
            {
                return await _embedService.CreateBasicEmbedAsync("Music Artwork", "The guild does not have a player.");
            }

            //get the player for the current guild
            var player = _lavaNode.GetPlayer(user.Guild);

            //if the bot is not playing anything say no songs are playing
            if (player.PlayerState != PlayerState.Playing)
            {
                return await _embedService.CreateBasicEmbedAsync("Music Artwork", "No songs playing.");
            }

            try
            {
                //get the track
                var track = player.Track;
                var artwork = await track.FetchArtworkAsync(); //get the artwork for the track

                //create embed builder
                var builder = new EmbedBuilder
                {
                    ImageUrl = artwork,
                    Author = new EmbedAuthorBuilder { Name = $"{track.Title} - {track.Author}", IconUrl = artwork }
                };

                //add the current timestamp to the builder
                builder.WithCurrentTimestamp();

                return builder.Build();
            }
            catch
            {
                return await _embedService.CreateBasicEmbedAsync("Music Artwork",
                    "Exception occured while trying to retrieve artwork");
            }
        }
    }
}