using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KurisuBot.Services.EmbedExtensions;
using System;

namespace KurisuBot.Modules
{
    [Name("Social")]
    public class SocialModule : ModuleBase
    {
        [Command("avatar")]
        [Summary("Shows full size image from specified user")]
        [Remarks("avatar tromodolo")]
        public async Task Avatar(SocketGuildUser User = null)
        {
            if (User == null)
            {
                var useravatar = Context.Message.Author.GetAvatarUrl(ImageFormat.Auto, 1024);
                if (useravatar.Contains(".gif"))
                {
                    var url = useravatar.Substring(0, useravatar.Length - 10);
                    await Context.Channel.SendImageEmbedAsync(url,
                        "Avatar for user " + Context.Message.Author.Username + ":");
                }
                else
                {
                    await Context.Channel.SendImageEmbedAsync(
                        Context.Message.Author.GetAvatarUrl(ImageFormat.Auto, 1024),
                        "Avatar for user " + Context.Message.Author.Username + ":");
                }
            }
            else
            {
                var useravatar = User.GetAvatarUrl(ImageFormat.Auto, 1024);
                if (useravatar.Contains(".gif"))
                {
                    var url = useravatar.Substring(0, useravatar.Length - 10);
                    await Context.Channel.SendImageEmbedAsync(url, "Avatar for user " + User.Username + ":");
                }
                else
                {
                    await Context.Channel.SendImageEmbedAsync(User.GetAvatarUrl(ImageFormat.Auto, 1024),
                        "Avatar for user " + User.Username + ":");
                }
            }
        }

        [Command("love")]
        [Summary("Calculates love between two people.")]
        [Remarks("love person1 person2")]
        public async Task Love(string person1, string person2 = null)
        {
            if (string.IsNullOrWhiteSpace(person2)) //if person2 is null, use Bot as second person WIP
            {
                await ReplyAsync("Please specify two people");
            }
            else
            {
                int person1Value = 0, person2Value = 0, randomSeed = 0;

                foreach (var s in person1.ToLower())
                    person1Value += Convert.ToInt32(s); //Copies the numerical value from the names to a variable
                foreach (var s in person2.ToLower())
                    person2Value += Convert.ToInt32(s);

                var Today = DateTime.Today;
                var change = Today.DayOfYear + Today.Year * 365;
                randomSeed = person1Value ^ person2Value ^ change; // uses bitwise operator to get a seed to use in random and add a change that depends on the date which makes it change every 24 hours.

                var rand = new Random(randomSeed);

                var lovePower = rand.Next(0, 100) + 1;

                switch (lovePower)
                {
                    case int i when i >= 1 && i <= 20:
                        await Context.Channel.SendColouredEmbedAsync($":crystal_ball: **({lovePower}%)** \n" +
                                                                     $"{printLoveBar(lovePower)}\n" +
                                                                     $"{person1} and {person2} don't seem to fit well together at all. :broken_heart:", Kurisu.KurisuClr);
                        break;
                    case int i when i >= 21 && i <= 40:
                        await Context.Channel.SendColouredEmbedAsync($":crystal_ball: **({lovePower}%)** \n" +
                                                                     $"{printLoveBar(lovePower)}\n" +
                                                                     $"{person1} and {person2} are not likely to work out.", Kurisu.KurisuClr);
                        break;
                    case int i when i >= 41 && i <= 60:
                        await Context.Channel.SendColouredEmbedAsync($":crystal_ball: **({lovePower}%)** \n" +
                                                                     $"{printLoveBar(lovePower)}\n" +
                                                                     $"{person1} and {person2} might have a chance together.", Kurisu.KurisuClr);
                        break;
                    case int i when i >= 61 && i <= 80:
                        await Context.Channel.SendColouredEmbedAsync($":crystal_ball: **({lovePower}%)** \n" +
                                                                     $"{printLoveBar(lovePower)}\n" +
                                                                     $"{person1} and {person2} fit well for each other.", Kurisu.KurisuClr);
                        break;
                    case int i when i >= 81 && i <= 100:
                        await Context.Channel.SendColouredEmbedAsync($":crystal_ball: **({lovePower}%)** \n" +
                                                                     $"{printLoveBar(lovePower)}\n" +
                                                                     $"{person1} and {person2} are perfect for each other! :heart:", Kurisu.KurisuClr);
                        break;
                        
                }
            }
        }


        [Command("pet")]
        public async Task Pet([Remainder] SocketGuildUser petTarget = null)
        {
            var petNum = new Random().Next(0, 13) + 1;

            if (petTarget == null)
                await Context.Channel.SendImageEmbedWithoutTitleAsync($"http://tromo.xyz/PetImages/{petNum}.gif", $":heart: *Pets {Context.Message.Author.Mention}*");
            else
                await Context.Channel.SendImageEmbedWithoutTitleAsync($"http://tromo.xyz/PetImages/{petNum}.gif", $":heart: *{Context.Message.Author.Mention} has petted {petTarget.Mention}*");
        }

        public string printLoveBar(int percentageNum)
        {
            double percentage = (double)percentageNum / 100;
            string progressBar = "`";

            for (int i = 1; i < percentage * 30; i++)
            {
                progressBar += "▬";
            }
            progressBar += "❤";
            for (int i = 1; i < 30 - (percentage * 30); i++)
            {
                progressBar += "▬";
            }
            progressBar += "`";
            return progressBar;
        }

    }
}