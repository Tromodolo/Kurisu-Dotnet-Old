using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;

namespace KatBot.Modules.Help
{
    [Name("Help")]
    public class HelpModule : InteractiveModuleBase<ICommandContext>
    {
        [Command("commands")]
        [Alias("help")]
        public async Task Commands()
        {
            var ModuleList = new List<string>();

            foreach (var m in Katarina.commandHandler.Commands.Modules)
            {
                if (m.Name.ToLower() == "owner")
                    continue;

                ModuleList.Add($"**{m.Name}**");
                ModuleList.Add(string.Join(", ", m.Commands.Select(x => x.Aliases.First())));
            }

            var Author = new EmbedAuthorBuilder().WithName(Katarina.client.CurrentUser.Username)
                .WithIconUrl(Katarina.client.CurrentUser.GetAvatarUrl());

            await ReplyAsync("", embed: new EmbedBuilder().WithAuthor(Author)
                .WithTitle("All commands start with a k? prefix")
                .WithDescription(string.Join("\n", ModuleList))
                .WithColor(Katarina.KatClr)
                .Build());
        }
        /*
         * Removed Because of abuse
        [Command("sendsuggestion", RunMode = RunMode.Async)]
        public async Task Suggestion()
        {
            var suggestionPrompt =
                await Context.Channel.SendMessageAsync(
                    ":pencil: Write a suggestion of a command or feature you would like added to the bot or write cancel:");
            var suggestion = await WaitForMessage(Context.Message.Author, Context.Channel, new TimeSpan(0, 1, 0));
            if (suggestion == null)
            {
                await suggestionPrompt.DeleteAsync();

                await Context.Channel.SendMessageAsync("Command has timed out due to inactivity.");
            }
            else if (suggestion.Content.ToLower() == "cancel")
            {
                await Context.Message.DeleteAsync();
                await suggestion.DeleteAsync();
                await suggestionPrompt.DeleteAsync();

                await Context.Channel.SendMessageAsync("Command has been canceled.");
            }
            else
            {
                await Context.Message.DeleteAsync();
                await suggestionPrompt.DeleteAsync();
                await suggestion.DeleteAsync();

                IMessageChannel TromoDm = await Katarina.client.GetUser(123184215423582208)
                    .GetOrCreateDMChannelAsync();
                await TromoDm.SendMessageAsync(
                    $"Feature suggested by {Context.Message.Author.Mention}: {suggestion.Content}");
                await ReplyAsync("Suggestion has been sent to developer. :heart:");
            }
        }
        */

        [Command("invite")]
        public async Task Invite()
        {
            await Context.Channel.SendMessageAsync("To invite me to your server, use this link: \nhttps://discordapp.com/oauth2/authorize?&client_id=338099495248068618&scope=bot&permissions=268492974");
        }
    }
}