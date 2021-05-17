using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADB.Config;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using static ADB.Models.CommonMessages;

namespace ADB.Modules
{
    [Summary("Commands for listing available commands")]
    [Discord.Commands.Name("Help")]
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public Help(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            //var emote = Emote.Parse();
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"These are the category you can use",
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    description += $"{cmd.Aliases.First()}\n";
                }
                string prefix = AppSettings.CommandPrefix;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        switch (module.Name)
                        {
                            case "Anime":
                                x.Name = module.Name + $" ❤️";
                                break;

                            case "Csgo":
                                x.Name = module.Name + " 🔫";
                                break;

                            case "Help":
                                x.Name = module.Name + " 💁🏻";
                                break;

                            case "Moderation":
                                x.Name = module.Name + " 🦾";
                                break;

                            case "Music":
                                x.Name = module.Name + " 🎧";
                                break;

                            case "NekoActiona":
                                x.Name = module.Name + " 💞";
                                break;

                            case "Steam":
                                x.Name = module.Name + " 🎮";
                                break;

                            default:
                                x.Name = module.Name;
                                break;
                        }

                        x.Value = $"{prefix}help ``{module.Name}``";
                        x.IsInline = true;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help"), Alias("h")]
        [Remarks("Shows what a specific command or module does and what parameters it takes.")]
        public async Task HelpQuery([Remainder] string query)
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Title = $"Help for '{query}'"
            };

            var result = _service.Search(Context, query);
            if (query.StartsWith("module"))
            {
                query = query.Remove(0, "module ".Length);
            }

            var emb = result.IsSuccess ? HelpCommand(result, builder) : await HelpModule(query, builder);

            if (emb.Fields.Length == 0)
            {
                await ReplyAsync($"Sorry, I couldn't find anything for \"{query}\".");
                return;
            }

            await Context.Channel.SendMessageAsync("", false, emb);
        }

        private static Embed HelpCommand(SearchResult search, EmbedBuilder builder)
        {
            foreach (var match in search.Commands)
            {
                var cmd = match.Command;
                var parameters = cmd.Parameters.Select(p => string.IsNullOrEmpty(p.Summary) ? p.Name : p.Summary);
                var paramsString = $"Parameters: {string.Join(", ", parameters)}" +
                                   (string.IsNullOrEmpty(cmd.Summary) ? "" : $"\nSummary: {cmd.Summary}") +
                                   (string.IsNullOrEmpty(cmd.Remarks) ? "" : $"\nRemarks: {cmd.Remarks}");

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = paramsString;
                    x.IsInline = false;
                });
            }

            return builder.Build();
        }

        private async Task<Embed> HelpModule(string moduleName, EmbedBuilder builder)
        {
            var module = _service.Modules.ToList().Find(mod =>
                string.Equals(mod.Name, moduleName, StringComparison.CurrentCultureIgnoreCase));
            await AddModuleEmbedField(module, builder);
            return builder.Build();
        }

        private async Task AddModuleEmbedField(ModuleInfo module, EmbedBuilder builder)
        {
            if (module is null)
            {
                return;
            }

            var descriptionBuilder = new List<string>();
            var duplicateChecker = new List<string>();
            foreach (var cmd in module.Commands.OrderBy(c => c.Name))
            {
                var result = await cmd.CheckPreconditionsAsync(Context);
                if (!result.IsSuccess || duplicateChecker.Contains(cmd.Aliases.First()))
                {
                    continue;
                }

                duplicateChecker.Add(cmd.Aliases.First());
                var addDesc = string.Join("`, `", cmd.Aliases.Where(c => c.Length <= 1 && c != cmd.Aliases.First()));
                addDesc = addDesc.Length > 0 ? $" (`{addDesc}`)" : "";
                var cmdDescription = $"`{cmd.Aliases.First()}`{addDesc}";
                //var cmdDescription = $"`{string.Join("`, `", cmd.Aliases)}`";
                if (!string.IsNullOrEmpty(cmd.Summary))
                {
                    cmdDescription += $" | {cmd.Summary}";
                }

                if (!string.IsNullOrEmpty(cmd.Remarks))
                {
                    cmdDescription += $" | {cmd.Remarks}";
                }

                if (cmdDescription != "``")
                {
                    descriptionBuilder.Add(cmdDescription);
                }
            }

            if (descriptionBuilder.Count <= 0)
            {
                return;
            }

            var builtString = string.Join("\n", descriptionBuilder);
            var testLength = builtString.Length;
            if (testLength >= 1024)
            {
                Console.WriteLine(testLength);
                builtString = builtString.Substring(0, 1000);
                //throw new ArgumentException("Value cannot exceed 1024 characters");
            }
            var moduleNotes = "";
            if (!string.IsNullOrEmpty(module.Summary))
            {
                moduleNotes += $" {module.Summary}";
            }

            if (!string.IsNullOrEmpty(module.Remarks))
            {
                moduleNotes += $" {module.Remarks}";
            }

            if (!string.IsNullOrEmpty(moduleNotes))
            {
                moduleNotes += "\n";
            }

            if (!string.IsNullOrEmpty(module.Name))
            {
                builder.AddField($"__**{module.Name}:**__",
                    $"{moduleNotes} {builtString}\n\u200b");
            }
        }
    }
}