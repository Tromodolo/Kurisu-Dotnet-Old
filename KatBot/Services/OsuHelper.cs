using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;

namespace KatBot.Services
{
    public class OsuHelper
    {
        public async Task<EmbedBuilder> GetUserAsync(string user, string gamemode, IMessageChannel chnl)
        {
            int gamemodeChoice;

            switch (gamemode)
            {
                case "standard":
                case "s":
                case "o":
                    gamemodeChoice = 0;
                    break;

                case "mania":
                case "m":
                    gamemodeChoice = 3;
                    break;

                case "catch":
                case "c":
                    await chnl.SendMessageAsync(
                        "osu!catch is not a supported gamemode yet. It might not work with old maps");
                    gamemodeChoice = 2;
                    break;

                case "taiko":
                case "t":
                    await chnl.SendMessageAsync(
                        "osu!taiko is not a supported gamemode yet. It might not work with old maps");
                    gamemodeChoice = 1;
                    break;

                default:
                    await chnl.SendMessageAsync(
                        $"{gamemode} is not a valid gamemode. Please write `standard` or `mania`");
                    return null;
            }

            var osuUser = await OsuMethods.GetUserAsync(user, gamemodeChoice);

            var username = new EmbedFieldBuilder().WithIsInline(true)
                                                  .WithName("Username:")
                                                  .WithValue(osuUser.Username);

            var rank = new EmbedFieldBuilder().WithIsInline(true)
                                              .WithName("Ranking:")
                                              .WithValue($"Global: #{osuUser.GlobalRank}\nCountry: #{osuUser.RegionalRank} ({osuUser.Country})");

            var pp = new EmbedFieldBuilder().WithIsInline(true)
                                            .WithName("Performance Points:")
                                            .WithValue($"{Math.Round(osuUser.Pp)} pp");

            var accuracy = new EmbedFieldBuilder().WithIsInline(true)
                                                  .WithName("Accuracy:")
                                                  .WithValue($"{Math.Round(osuUser.Accuracy, 2)}%");

            var playCount = new EmbedFieldBuilder().WithIsInline(true)
                                                   .WithName("Play Count:")
                                                   .WithValue($"{osuUser.PlayCount}");


            var levelRound = Math.Floor(osuUser.Level);

            var level = new EmbedFieldBuilder().WithIsInline(true)
                        .WithName("Level:")
                .WithValue($"{levelRound} ({Math.Round(osuUser.Level - levelRound, 2) * 100}%)");

            var events = "";
            var eventLoop = 0;

            if (osuUser.Events.Count > 0)
                foreach (var userEvent in osuUser.Events)
                {
                    var mapDate = userEvent.Date.Value.Subtract(new TimeSpan(6, 0, 0));
                    string timeSince;
                    if ((DateTime.Now - mapDate).TotalHours < 0)
                        timeSince = $"**{Math.Round((DateTime.Now - mapDate).TotalMinutes)} minute(s) ago :**\n";
                    else if ((DateTime.Now - mapDate).TotalHours > 0 && (DateTime.Now - mapDate).TotalHours < 24)
                        timeSince = $"**{Math.Round((DateTime.Now - mapDate).TotalHours)} hour(s) ago :**\n";
                    else
                        timeSince = $"**{Math.Round((DateTime.Now - mapDate).TotalDays)} day(s) ago :**\n";

                    var html = userEvent.DisplayHtml;
                    var htmlUsername = Regex.Match(html, "(?<='>)(.*)(?=</a></b>)").ToString();
                    var htmlEvent = Regex.Match(html, "(?<=</b>)(.*)(?=<a href=)").ToString();
                    var htmlBeatmap = Regex.Match(html, "(?<==\\d'>)(.*)(?=</a>)").ToString();

                    htmlEvent = htmlEvent.Replace("<b>", "");
                    htmlEvent = htmlEvent.Replace("</b>", "");
                    htmlBeatmap = htmlBeatmap.Replace("&amp;", "&");

                    var fullmessage = $"{timeSince}{htmlUsername}{htmlEvent}{htmlBeatmap}\n";

                    events += fullmessage;

                    eventLoop++;
                    if (eventLoop == 3)
                        break;
                }
            else
                events = $"{osuUser.Username} has no recent events.";


            var recentEvents = new EmbedFieldBuilder().WithName("Recent Events:").WithValue(events);

            var embed = new EmbedBuilder().WithTitle("")
                .WithColor(Katarina.KatClr)
                .WithThumbnailUrl($"https://a.ppy.sh/{osuUser.Userid}")
                .AddField(username)
                .AddField(rank)
                .AddField(pp)
                .AddField(accuracy)
                .AddField(playCount)
                .AddField(level)
                .AddField(recentEvents);
            return embed;
        }

        public async Task<EmbedBuilder> GetUserRecentAsync(string user, string gamemode, IMessageChannel chnl)
        {
            int gamemodeChoice;

            switch (gamemode)
            {
                case "standard":
                case "s":
                case "o":
                    gamemodeChoice = 0;
                    break;

                case "mania":
                case "m":
                    gamemodeChoice = 1;
                    break;

                case "catch":
                case "c":
                    await chnl.SendMessageAsync(
                        "osu!catch is not a supported gamemode yet. It might not work with old maps");
                    gamemodeChoice = 2;
                    break;

                case "taiko":
                case "t":
                    await chnl.SendMessageAsync(
                        "osu!taiko is not a supported gamemode yet. It might not work with old maps");
                    gamemodeChoice = 3;
                    break;

                default:
                    await chnl.SendMessageAsync(
                        $"{gamemode} is not a valid gamemode. Please write `standard` or `mania`");
                    return null;
            }


            var scores = await OsuMethods.GetUserRecentAsync(user, gamemodeChoice, 1);
            var osuUser = await OsuMethods.GetUserAsync(user, gamemodeChoice);

            var score = scores[0];

            var mapInfo = new EmbedFieldBuilder();

            if (scores.Count < 1)
            {
                mapInfo = new EmbedFieldBuilder().WithName("Sad")
                    .WithValue($"There are no recent plays for {osuUser.Username}. :cry:");
            }
            else
            {
                var map = await OsuMethods.GetBeatmapAsync(score.BeatmapId, gamemodeChoice);

                if (score.Rank == "X" || score.Rank == "XH")
                    score.Rank = "SS";
                if (score.Rank == "SH")
                    score.Rank = "S";

                var mods = "+" + (Mods) score.EnabledMods;
                if (mods == "+Nomod")
                    mods = "";

                var mapDate =
                    score.Date.Subtract(new TimeSpan(6, 0,
                        0)); //This is to offset the timezone difference between the osu servers and sweden's timezone wont work if you have any other
                var timeAgo = $"{(DateTime.Now - mapDate).TotalDays} Days ago";
                var date = DateTime.Now - mapDate;

                if (date.TotalMinutes < 60)
                    timeAgo = $"{Math.Round(date.TotalMinutes, 0)} minute(s) ago";
                else if (date.TotalHours < 24)
                    timeAgo = $"{Math.Round(date.TotalHours, 0)} hour(s) ago";
                else if (date.TotalDays > 30)
                    timeAgo = $"{Math.Round((DateTime.Now - score.Date).TotalDays / 30, 0)} Month(s) ago";

                mapInfo = new EmbedFieldBuilder().WithName($"**The most recent play for {osuUser.Username}**\n")
                    .WithValue($"{map.Title} [{map.Version}] {mods}\n" + string.Format("{0, 10} {1, 14} {2, 12}",
                                   ":star:: " + Math.Round(map.DifficultyRating, 2),
                                   score.MaxCombo + "x/" + map.MaxCombo + "x",
                                   string.Empty.PadLeft(3) +
                                   GetAcc(score.Count50, score.Count100, score.Count300, score.CountMiss,
                                       score.CountGeki, score.CountKatu, gamemodeChoice) + "% " +
                                   string.Empty.PadLeft(1) + score.Rank + string.Empty.PadLeft(2) +
                                   Math.Round(score.Pp) + "pp") +
                               $"\n:calendar: {timeAgo}");
            }

            var embed = new EmbedBuilder().WithColor(Katarina.KatClr).AddField(mapInfo);


            return embed;
        }

        public async Task<EmbedBuilder> GetUserTopAsync(string user, string gamemode, IMessageChannel chnl)
        {
            int gamemodeChoice;

            switch (gamemode)
            {
                case "standard":
                case "s":
                case "o":
                    gamemodeChoice = 0;
                    break;

                case "mania":
                case "m":
                    gamemodeChoice = 3;
                    break;

                case "catch":
                case "c":
                    await chnl.SendMessageAsync(
                        "osu!catch is not a supported gamemode yet. It might not work with old maps");
                    gamemodeChoice = 2;
                    break;

                case "taiko":
                case "t":
                    await chnl.SendMessageAsync(
                        "osu!taiko is not a supported gamemode yet. It might not work with old maps");
                    gamemodeChoice = 1;
                    break;

                default:
                    await chnl.SendMessageAsync(
                        $"{gamemode} is not a valid gamemode. Please write `standard` or `mania`");
                    return null;
            }


            var scores = await OsuMethods.GetUserBestAsync(user, gamemodeChoice);
            var osuUser = await OsuMethods.GetUserAsync(user, gamemodeChoice);

            var userStats = new EmbedFooterBuilder()
                .WithText($"Total performance points for user {osuUser.Username}: {Math.Round(osuUser.Pp)}pp")
                .WithIconUrl($"https://a.ppy.sh/{osuUser.Userid}");


            var embed = new EmbedBuilder().WithTitle($"")
                .WithColor(Katarina.KatClr)
                .WithFooter(userStats);


            var scoreposition = 1;
            foreach (var score in scores)
            {
                var map = await OsuMethods.GetBeatmapAsync(score.BeatmapId, gamemodeChoice);

                if (score.Rank == "X" || score.Rank == "XH")
                    score.Rank = "SS";
                if (score.Rank == "SH")
                    score.Rank = "S";

                var mods = "+" + (Mods) score.EnabledMods;
                if (mods == "+Nomod")
                    mods = "";

                var mapDate =
                    score.Date.Subtract(new TimeSpan(6, 0,
                        0)); //This is to offset the timezone difference between the osu servers and sweden's timezone wont work if you have any other
                var timeAgo = $"{Math.Round((DateTime.Now - mapDate).TotalDays)} Days ago";
                var date = DateTime.Now - mapDate;

                if (date.TotalMinutes < 60)
                    timeAgo = $"{Math.Round(date.TotalMinutes, 0)} minute(s) ago";
                else if (date.TotalHours < 24)
                    timeAgo = $"{Math.Round(date.TotalHours, 0)} hour(s) ago";
                else if (date.TotalDays > 30)
                    timeAgo = $"{Math.Round((DateTime.Now - score.Date).TotalDays / 30, 0)} Month(s) ago";


                embed.AddField(new EmbedFieldBuilder()
                    .WithName($"**#{scoreposition}** {map.Title} [{map.Version}] {mods}")
                    .WithValue(string.Format("    {0, 10} {1, 14} {2, 12}",
                                   ":star:: " + Math.Round(map.DifficultyRating, 2),
                                   score.MaxCombo + "x/" + map.MaxCombo + "x",
                                   string.Empty.PadLeft(3) +
                                   GetAcc(score.Count50, score.Count100, score.Count300, score.CountMiss,
                                       score.CountGeki, score.CountKatu, gamemodeChoice) + "% " +
                                   string.Empty.PadLeft(1) + score.Rank + string.Empty.PadLeft(2) +
                                   Math.Round(score.Pp) + "pp") +
                               $"\n:calendar: {timeAgo}"));
                scoreposition++;
            }

            return embed;
        }


        public static double GetAcc(double count50, double count100, double count300, double countmiss,
            double countgeki, double countkatu, int mode)
        {
            var sum = 0.0;
            switch (mode)
            {
                case 0:
                    sum = (50 * count50 + 100 * count100 + 300 * count300) /
                          (countmiss + count50 + count100 + count300) / 3;
                    break;
                case 1: //TODO TAIKO
                    break;
                case 2: //TODO CTB
                    break;
                case 3: //TODO FIX MANIA ACC
                    var totalhits = count50 + count100 + count300 + countkatu + countgeki;
                    sum = (count50 * 50 + count100 * 100 + countkatu * 200 + (count300 + countgeki) * 300) /
                          (totalhits * 3);
                    break;
            }

            return Math.Round(sum, 2);
        }

        private enum Mods
        {
            Nomod = 0,
            NF = 1,
            EZ = 2,
            HD = 8,
            HR = 16,
            HDHR = 24,
            SD = 32,
            DT = 64,
            HDDT = 72,
            HRDT = 80,
            HDHRDT = 88,
            Relax = 128,
            HT = 256,
            NC = 512,
            FL = 1024,
            HDFl = 1032,
            DTFL = 1088,
            HDHRDTFL = 1112,
            Auto = 2048,
            SO = 4096,
            NFSOHDDT = 4169,
            Autopilot = 8192, // Autopilot?
            PF = 16384
        }
    }
}