using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using KatBot.Services.EmbedExtensions;

namespace KatBot.Modules.Fun
{
    [Name("Fun")]
    public class FunModule : ModuleBase
    {
        [Command("emote")]
        [Alias("serveremote", "emoji")]
        [Summary("Sends full size image of an emote.")]
        [Remarks("emote meguButt")]
        public async Task Emote(string emote)
        {
            var reg = new Regex("\\:(\\d.*?[0-9])\\>", RegexOptions.IgnoreCase);        //using regex to match the id between the : and > in the emote code
            var m = reg.Match(emote);                                                   //dont ask how regex works because i dont know
            if (m.Success)                                                              //black magic happens
            {
                var int1 = m.Groups[1].ToString();
                var imageurl = "https://cdn.discordapp.com/emojis/" + int1 + ".png";

                await Context.Channel.SendImageEmbedAsync(imageurl, $"**{emote}**");
            }
        }

        [Command("echo")]
        [Alias("say")]
        [Summary("Echoes a message.")]
        [Remarks("echo hi")]
        public async Task Echo([Remainder] string message)
        {
            if (Context.Message.MentionedRoleIds.Count > 0
            ) // this is to avoid @everyone being mentioned using the bot's permissions
                return;
            await ReplyAsync($"📣 `" + message + "`");
        }

        [Command("ping")]
        [Summary("Pong!")]
        [Remarks("ping")]
        public async Task Ping()
        {
            var now = DateTimeOffset.Now;
            var msg = await ReplyAsync(":ping_pong: Pong!");
            var mslag = now - Context.Message.Timestamp;
            await msg.ModifyAsync(x => x.Content = ":ping_pong: Pong! `" + mslag.Milliseconds + "ms`");
        }

        [Command("random")]
        [Alias("rand", "rng")]
        [Summary("Returns random number from 0 to number specified, or 100 if unspecified.")]
        [Remarks("random 57")]
        public async Task Random(int maxNumber = 100)
        {
            var rand = new Random();
            var result = rand.Next(0, maxNumber);
            await ReplyAsync($":game_die: {result}");
        }

        /*      
         *      This is broken because server PC doesnt have CUDA GPU.
         *      
        [Command("upscale")]
        public async Task BiggerImage([Remainder] string imageurl = null)
        {
            if (imageurl == null)
                return;

            var http = new HttpClient();

            try
            {
                using (var stream = await http.GetStreamAsync(imageurl))
                {
                    System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                    img.Save("waifu2x/data/in.png");
                    await startWaifu2x();
                    await Context.Channel.SendFileAsync("waifu2x/data/out.png");
                }
            }
            catch(Exception e)
            {
                await Context.Channel.SendErrorAsync("fucks:" + e.Message);
            }
        }

        private async Task<string> startWaifu2x()
        {
            using (Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "run.bat",
                    //Arguments = $"-i data/in.png -o data/out.png",
                    WorkingDirectory = "waifu2x",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
            })
            {
                process.Start();
                var str = await process.StandardOutput.ReadToEndAsync();
                var err = await process.StandardError.ReadToEndAsync();
                if (!string.IsNullOrEmpty(err))
                    Console.WriteLine(err);
                return str;
            }
        }
        */
    }
}