using System;
using System.Linq;
using System.Threading.Tasks;
using CommonBotLibrary.Services;
using Discord;
using Discord.Commands;
using KurisuBot.Services;

namespace KurisuBot.Modules.Searches
{
    [Name("Searches")]
    public class SearchesModule : ModuleBase
    {
        [Command("google")]
        public async Task Google([Remainder] string query)
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                var google = new GoogleService();

                var results = await google.GoogleSearchAsync(query);


                var firstResult = new EmbedFieldBuilder().WithName("Top Result:")
                                                         .WithValue($"\n[{results.Items[0].Title}]({results.Items[0].DisplayLink})" +
                                                                    $"\n{results.Items[0].Snippet}\n[Read More]({results.Items[0].DisplayLink})")
                                                         .WithIsInline(false);

                var otherResults = new EmbedFieldBuilder().WithName("Also see:")
                                                          .WithValue($"\n{results.Items[1].Title}\n[Read More]({results.Items[1].DisplayLink})" +
                                                                     $"\n{results.Items[2].Title}\n[Read More]({results.Items[2].DisplayLink})")
                                                          .WithIsInline(false);

                var embed = new EmbedBuilder().WithTitle($"**Searched for: \"{query}\"**")
                                              .WithColor(Kurisu.KurisuClr);

                embed.AddField(firstResult).AddField(otherResults);

                await ReplyAsync("", embed: embed.Build());
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message);
            }
        }

        [Command("osutop", RunMode = RunMode.Async)]
        public async Task OsuTop(string username, string gamemode = "standard")
        {
            if (Kurisu.CommandTimer.ContainsKey(Context.Message.Author.Id))
            {
                var timeDelay = DateTime.Now - Kurisu.CommandTimer[Context.Message.Author.Id];
                if (timeDelay.TotalSeconds < 30 && Context.Message.Author.Id != 123184215423582208)
                {
                    await ReplyAsync(
                        $"Please wait {30 - Math.Round(timeDelay.TotalSeconds)} seconds before trying again. This shares cooldown with all osu commands.");
                    return;
                }

                Kurisu.CommandTimer.Remove(Context.Message.Author.Id);
            }
            else
            {
                Kurisu.CommandTimer.TryAdd(Context.Message.Author.Id, DateTime.Now);
            }

            await Context.Channel.TriggerTypingAsync();
            var emb = await new OsuHelper().GetUserTopAsync(username, gamemode, Context.Channel);

            if (emb == null)
                return;

            await ReplyAsync("", embed: emb.Build());
        }

        [Command("osurecent", RunMode = RunMode.Async)]
        public async Task OsuRecent(string username, string gamemode = "standard")
        {
            if (Kurisu.CommandTimer.ContainsKey(Context.Message.Author.Id))
            {
                var timeDelay = DateTime.Now - Kurisu.CommandTimer[Context.Message.Author.Id];
                if (timeDelay.TotalSeconds < 30 && Context.Message.Author.Id != 123184215423582208)
                {
                    await ReplyAsync(
                        $"Please wait {30 - Math.Round(timeDelay.TotalSeconds)} seconds before trying again. This shares cooldown with all osu commands.");
                    return;
                }

                Kurisu.CommandTimer.Remove(Context.Message.Author.Id);
            }
            else
            {
                Kurisu.CommandTimer.TryAdd(Context.Message.Author.Id, DateTime.Now);
            }

            await Context.Channel.TriggerTypingAsync();
            var emb = await new OsuHelper().GetUserRecentAsync(username, gamemode, Context.Channel);

            if (emb == null)
                return;

            await ReplyAsync("", embed: emb.Build());
        }

        [Command("osuprofile", RunMode = RunMode.Async)]
        public async Task osuProfile(string username, string gamemode = "standard")
        {
            if (Kurisu.CommandTimer.ContainsKey(Context.Message.Author.Id))
            {
                var timeDelay = DateTime.Now - Kurisu.CommandTimer[Context.Message.Author.Id];
                if (timeDelay.TotalSeconds < 30 && Context.Message.Author.Id != 123184215423582208)
                {
                    await ReplyAsync(
                        $"Please wait {30 - timeDelay.TotalSeconds} seconds before trying again. This shares cooldown with all osu commands.");
                    return;
                }

                Kurisu.CommandTimer.Remove(Context.Message.Author.Id);
            }
            else
            {
                Kurisu.CommandTimer.TryAdd(Context.Message.Author.Id, DateTime.Now);
            }

            await Context.Channel.TriggerTypingAsync();
            var emb = await new OsuHelper().GetUserAsync(username, gamemode, Context.Channel);

            if (emb == null)
                return;

            await ReplyAsync("", embed: emb.Build());
        }

        [Command("shortener")]
        [Alias("urlshortener", "shortenurl")]
        public async Task Shortener([Remainder] string url)
        {
            await Context.Channel.TriggerTypingAsync();
            var google = new GoogleService();

            var results = await google.ShortenUrlAsync(url);

            var embed = new EmbedBuilder().WithTitle("**Shortened URL:**")
                .WithUrl(results.Id)
                .WithColor(Kurisu.KurisuClr)
                .WithDescription($"\n{results.Id}")
                .WithFooter(new EmbedFooterBuilder().WithText($"Original URL: {url}"));

            await ReplyAsync("", embed: embed.Build());
        }


        [Command("urbandictionary")]
        [Alias("urban", "ud")]
        public async Task UrbanDict([Remainder] string word)
        {
            var Urban = new UrbanDictionaryService();
            var definitions = await Urban.GetDefinitionsAsync(word);
            string defLink, definition;
            if (!definitions.Any())
            {
                defLink = "";
                definition = "No definitions found";
            }
            else
            {
                defLink = definitions.First().Permalink;
                definition = definitions.First().Definition;
            }

            EmbedFieldBuilder topDefinition = new EmbedFieldBuilder().WithName("Top Definition for: \"" + word + "\"");

            if (definition != "No definitions found")
                topDefinition.Value = definition + $"\n[Read More]({defLink})";

            var embed = new EmbedBuilder().WithColor(Kurisu.KurisuClr);

            embed.AddField(topDefinition);

            await ReplyAsync("", embed: embed.Build());
        }

        [Command("myanimelist")]
        [Alias("mal", "anime")]
        public async Task MyAnimeList([Remainder] string anime)
        {
            var MAL = new MyAnimeListService(Kurisu.botData.myanimelistname, Kurisu.botData.myanimelistpass);

            var animeResult = await MAL.SearchAsync(anime);

            string animeLink = "",
                animeName = "",
                animeSynposis = "",
                animeImageUrl = "",
                episodeCount = "",
                animeStatus = "";
            var animeRating = 0.0;

            if (!animeResult.Any())
            {
                var emb = new EmbedBuilder().WithTitle($"**No results found for {anime}:**").WithColor(Kurisu.KurisuClr);
                await ReplyAsync("", embed: emb.Build());
                return;
            }
            animeLink = animeResult.First().Url;
            animeName = animeResult.First().Title;
            animeSynposis = animeResult.First().Synopsis;
            animeImageUrl = animeResult.First().Image;
            animeRating = animeResult.First().Score;
            animeStatus = animeResult.First().Status;

            if (animeResult.First().Episodes == 0 && !(animeResult.First().Status == "Not yet aired"))
                episodeCount = "Over 775";
            else
                episodeCount = animeResult.First().Episodes.ToString();

            if (animeSynposis.Length > 500)
                animeSynposis = animeSynposis.Substring(0, 500) + "...";

            var embed = new EmbedBuilder().AddField(new EmbedFieldBuilder().WithName(animeName).WithValue(animeSynposis)
                    .WithIsInline(false))
                .AddField(new EmbedFieldBuilder().WithName("Episodes:").WithValue(episodeCount).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Rating:").WithValue(animeRating).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Status:").WithValue($"{animeStatus}").WithIsInline(true))
                .WithThumbnailUrl(animeImageUrl)
                .WithColor(Kurisu.KurisuClr)
                .WithFooter(new EmbedFooterBuilder().WithText($"Url: {animeLink}"));


            await ReplyAsync("", embed: embed.Build());
        }
    }
}