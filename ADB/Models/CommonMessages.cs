using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ADB.Models
{
    public static class CommonMessages
    {
        public const string OK_MSG = "Ok!";
        public const string OK_EMOJI_UNICODE = "✅";
        public const string ERROR_EMOJI_UNICODE = "❌";
        public static IEmote ERROR_EMOJI { get; } = ERROR_EMOJI ??= new Emoji(ERROR_EMOJI_UNICODE);
        public static IEmote OK_EMOJI { get; } = OK_EMOJI ??= new Emoji(OK_EMOJI_UNICODE);

        public static async Task ReplyOkOrFail<T>(this Task<T> task, string commandName, SocketCommandContext context, string? msg = null, ILogger? logger = null, bool notifyErrorToUser = false)
        {
            if (task.IsFaulted && !task.IsCompletedSuccessfully)
            {
                logger?.LogError(task.Exception, $"Command name: {commandName}, Command text: {context.Message.Content}");
                if (notifyErrorToUser)
                {
                    await context.Channel.SendMessageAsync($"An error has occurred with `{commandName}`. Notify this to a bot maintainer.");
                }

                return;
            }
            if (string.IsNullOrWhiteSpace(msg))
            {
                await context.Message.AddReactionAsync(OK_EMOJI);
            }
            else
            {
                await context.Message.Channel.SendMessageAsync(msg);
            }
        }

        public static string DependencyFail(string moduleName, string functionName)
        {
            return $"Dependency injection failed. Please report to bot maintainer. Module: {moduleName} - Function: {functionName}";
        }
    }
}
