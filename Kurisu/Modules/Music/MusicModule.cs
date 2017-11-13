using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using KurisuBot.Services.EmbedExtensions;

namespace KurisuBot.Modules.Music
{
    [Name("Music")]
    public class MusicModule : InteractiveModuleBase<SocketCommandContext>
    {
        // Scroll down further for the AudioService.
        // Like, way down
        private readonly AudioService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public MusicModule(AudioService service)
        {
            _service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCmd()
        {
            try
            {
                await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
                _service.guildOptions[Context.Guild.Id].Settings.textchannel = Context.Channel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
        }


        [Command("Play", RunMode = RunMode.Async)]
        public async Task PlayMusic([Remainder] string url)
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;

            if (_settings.voiceClient != null)
                if (await new Youtube().GetYoutubeSong(url, _settings))
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + $" Song: {_settings.playList.Last().Title} has been added to the queue.");
                    await _service.HandleSongs(Context.Channel);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + $" Error occured while adding a Song to the queue, possible causes: {Environment.NewLine}- Song is too long (more than 20 minutes).{Environment.NewLine}- Song does not exist.{Environment.NewLine}- Song is already in the queue.");
                }
        }

        [Command("Queue", RunMode = RunMode.Async)]
        public async Task ShowQueue()
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;
            var songNr = 0;
            string songList = "";
            

            if (_settings.playList.Count > 0)
            {
                songList = "**Current Song:** " + _settings.currentSong.Title + "\n**Length: **`" +
                               _settings.stopwatch.Elapsed.Minutes + ":" + _settings.stopwatch.Elapsed.Seconds + "/" +
                               _settings.currentSong.Duration.Minutes + ":" + _settings.currentSong.Duration.Seconds +
                               "\n `\n**Progress:**\n" + _service.printProgress(_settings.stopwatch, _settings.currentSong.Duration) + "\n \n" ;

                songList += "**Song Queue:** \n \n";
                foreach (var song in _settings.playList)
                {
                    if (songNr == 10)
                    {
                        songList += $"**\n... and {_settings.playList.Count - 10} more songs in the queue**";

                        break;
                    }
                    songNr++;

                    songList += "**" + songNr + ".** ";
                    songList += song.Title + " ";
                    songList += "\n" + string.Empty.PadLeft(4) + "**Length: **`"
                        + song.Duration.Minutes + ":" + song.Duration.Seconds + "`" + "\n";
                }

            }     
            else if(_settings._playing)
            {
                songList = "**Current Song:** " + _settings.currentSong.Title + "\n**Length: **`" +
                               _settings.stopwatch.Elapsed.Minutes + ":" + _settings.stopwatch.Elapsed.Seconds + "/" +
                               _settings.currentSong.Duration.Minutes + ":" + _settings.currentSong.Duration.Seconds +
                               "\n `\n**Progress:**\n" + _service.printProgress(_settings.stopwatch, _settings.currentSong.Duration) + "\n \n";
                songList += "**Song Queue:** \n \n";
                songList += $" No more songs in the queue.";
            }
            else
            {
                songList += "**Song Queue:** \n \n";
                songList += $" No more songs in the queue.";
            }

            var builder = new EmbedBuilder();

            builder.Color = Kurisu.KurisuClr;
            builder.Description = songList;
            builder.ThumbnailUrl = _settings.currentSong.Thumbnail;

            await Context.Channel.SendMessageAsync("", embed: builder.Build());

        }

        [Command("skip", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Skip()
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;

            if (_settings.voiceClient != null)
            {
                _settings.cancellationToken.Cancel();
                _settings._playing = false;
                await _service.HandleSongs(Context.Channel);
                await ReplyAsync("Song skipped");
            }
        }

        [Command("shuffle", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Shuffle()
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;

            if (_settings.voiceClient != null && _settings.playList.Count > 1)
            {
                _settings.playList = _service.ShuffleList(_settings.playList);
                await ReplyAsync("Song queue shuffled");
            }
            else if(_settings.voiceClient != null && _settings.playList.Count < 2)
            {
                await ReplyAsync("You need more than 1 song in the queue to shuffle");
            }
        }
        /*
         * 
         * TODO: FIX THIS
         * 
        [Command("clear", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Clear()
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;
            if (_settings.voiceClient != null)
            {
                _settings.playList.Clear();
                _settings.cancellationToken.Cancel();

                await ReplyAsync("Playlist cleared");
            }
        }
        
        TODO: FIX THIS TOO

        [Command("loop", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Loop(string choice)
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;
            if (_settings.voiceClient != null)
            {
                if (choice.ToLower() == "song")
                {
                    _settings.loopSong = !_settings.loopSong;
                    if (_settings.loopSong)
                        await Context.Channel.SendMessageAsync(":white_check_mark: Now looping current song.");
                    else
                        await Context.Channel.SendMessageAsync(":x: No longer looping current song.");
                }
                else if (choice.ToLower() == "queue")
                {
                    _settings.loopQueue = !_settings.loopQueue;
                    if (_settings.loopQueue)
                        await Context.Channel.SendMessageAsync(":white_check_mark: Now looping queue.");
                    else
                        await Context.Channel.SendMessageAsync(":x: No longer looping queue.");
                }
                else return;
            }
        }


        */
        /*
        [Command("volume", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Shuffle()
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;

            if (_settings.voiceClient != null && _settings.playList.Count > 1)
            {
                _settings.playList = _service.ShuffleList(_settings.playList);
                await ReplyAsync("Song queue shuffled");
            }
            else if (_settings.voiceClient != null && _settings.playList.Count < 2)
            {
                await ReplyAsync("You need more than 1 song in the queue to shuffle");
            }
        }

        [Command("pause", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Shuffle()
        {
            var _settings = _service.guildOptions[Context.Guild.Id].Settings;

            if (_settings.voiceClient != null && _settings.playList.Count > 1)
            {
                _settings.playList = _service.ShuffleList(_settings.playList);
                await ReplyAsync("Song queue shuffled");
            }
            else if (_settings.voiceClient != null && _settings.playList.Count < 2)
            {
                await ReplyAsync("You need more than 1 song in the queue to shuffle");
            }
        }
        */
    }
}