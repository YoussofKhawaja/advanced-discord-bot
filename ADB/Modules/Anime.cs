using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ADB.Models;
using Discord;
using Discord.Commands;
using JikanDotNet;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ADB.Modules
{
    [Summary("Anime Commands")]
    public class Anime : ModuleBase<SocketCommandContext>
    {
        [Command("animelist")] //command name
        public async Task Animee([Remainder] string query) //command method
        {
            Console.WriteLine("Search query: " + query);

            IJikan jikan = new Jikan();

            var services = new ServiceCollection().AddSingleton<IJikan, Jikan>().BuildServiceProvider();

            AnimeSearchResult result = await jikan.SearchAnime(query);

            if (result == null)
            {
                await Context.Channel.SendMessageAsync("Your search query returned nothing. Please check spelling and try again" +
                    "\n if the problem persists please make an issue on the bot's github (run `bro!github`)");
                return;
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithTitle(result.Results.First().Title)
                    .WithDescription(result.Results.First().Description)
                    .WithImageUrl(result.Results.First().ImageURL)
                    .AddField("MyAnimeList 🆔", result.Results.First().MalId, true)
                    .AddField("Age Rating 🔞", $"{ result.Results.First().Rated}. ", true)
                    .AddField("Episode Count 🎞️", $"{result.Results.First().Episodes}. ", true)
                    .AddField("Start Date 📅", $"{result.Results.First().StartDate}. ", true)
                    .AddField("Airing? 📺", $"{result.Results.First().Airing}. ", true)
                    .AddField("End Date 📅", $"{result.Results.First().EndDate}. ", true)
                    .AddField("Score ⭐", $"{result.Results.First().Score}.", true)
                    .AddField("Type 📽️", $"{result.Results.First().Type}.", true)
                    .AddField("Anime Members count on MAL 🧑🏻‍🤝‍🧑🏽", $"{result.Results.First().Members}.", true)
                    .WithFooter($"{result.Results.First().URL}. ")
                    .WithColor(242, 145, 0);

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                Console.WriteLine("Search was Successful");
            }
        }

        [Command("manga")]
        public async Task Manga([Remainder] string query)
        {
            Console.WriteLine("Search query: " + query);
            if (string.IsNullOrEmpty(query))
            {
                await Context.Channel.SendMessageAsync("Search Failed." +
                    "\n No query was specified");

                return;
            }
            IJikan jikan = new Jikan();

            var services = new ServiceCollection().AddSingleton<IJikan, Jikan>().BuildServiceProvider();

            MangaSearchResult result = await jikan.SearchManga(query);
            if (result == null)
            {
                await Context.Channel.SendMessageAsync("Your search query returned nothing. Please check spelling and try again" +
                    "\n if the problem persists please make an issue on the bot's github (run `bro!github`)");
                return;
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithTitle(result.Results.First().Title)
                    .WithDescription(result.Results.First().Description)
                    .WithImageUrl(result.Results.First().ImageURL)
                    .AddField("MyAnimeList 🆔", result.Results.First().MalId, true)
                    .AddField("Chapter 📘", $"{result.Results.First().Chapters}. ", true)
                    .AddField("Volumes 📗", $"{result.Results.First().Volumes}.", true)
                    .AddField("Start Date 📅", $"{result.Results.First().StartDate}. ", true)
                    .AddField("Publishing? 📚", $"{result.Results.First().Publishing}. ", true)
                    .AddField("End Date 📅", $"{result.Results.First().EndDate}. ", true)
                    .AddField("Score ⭐", $"{result.Results.First().Score}. ", true)
                    .AddField("Type 📖 ️", $"{result.Results.First().Type}. ", true)
                    .AddField("Manga Members count on MAL 🧑🏻‍🤝‍🧑🏽", $"{result.Results.First().Members}. ", true)
                    .WithFooter($"{result.Results.First().URL}. ")
                    .WithColor(157, 252, 3);

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                Console.WriteLine("Search was Successful");
            }
        }

        [Command("malcharacter")]
        public async Task MangaCharacter([Remainder] string query)
        {
            Console.WriteLine("Search query: " + query);
            IJikan jikan = new Jikan();

            var services = new ServiceCollection().AddSingleton<IJikan, Jikan>().BuildServiceProvider();

            CharacterSearchResult result = await jikan.SearchCharacter(query);

            var builder = new EmbedBuilder()
                .WithTitle(result.Results.First().Name)
                .WithImageUrl(result.Results.First().ImageURL)
                .AddField("MyAnimeList 🆔", result.Results.First().MalId, true)
                .WithFooter($"{result.Results.First().URL}. ")
                .WithColor(245, 120, 66);

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
            Console.WriteLine("Search was Successful");
        }
        [Command("mal")]
        public async Task MALUser([Remainder] string User)
        {
            try
            {
                HttpClient client = new HttpClient();
                var json = client.GetStringAsync("https://api.jikan.moe/v3/user/" + User);
                MALUser x = JsonConvert.DeserializeObject<MALUser>(json.Result);

                var MAL = new EmbedBuilder();

                EmbedFieldBuilder watched = new EmbedFieldBuilder();
                watched.IsInline = true;
                watched.Name = "Days Watched: ";
                watched.Value = x.anime_stats.days_watched;

                EmbedFieldBuilder total = new EmbedFieldBuilder();
                total.IsInline = false;
                total.Name = "Total Entries: ";
                total.Value = x.anime_stats.total_entries;

                MAL.WithTitle(x.username)
                .WithThumbnailUrl(x.image_url)
                .AddField(watched)
                .AddField(total)
                .AddField("Completed:", x.anime_stats.completed, true)
                .AddField("Watching:", x.anime_stats.watching, true)
                .AddField("Dropped:", x.anime_stats.dropped, true);

                await Context.Channel.SendMessageAsync("", false, MAL.Build());
            }
            catch (Exception)
            {
                await Context.Channel.SendMessageAsync("Couldn't find any user with that nickname!");
            }
        }

    }
}