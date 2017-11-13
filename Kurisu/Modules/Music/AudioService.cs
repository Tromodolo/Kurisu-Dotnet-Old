using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using System.Text.RegularExpressions;


namespace KurisuBot.Modules.Music
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        private CancellationTokenSource _source;

        public ConcurrentDictionary<ulong, GuildSettings> guildOptions = new ConcurrentDictionary<ulong, GuildSettings>();


        public string printProgress(Stopwatch stopwatch, TimeSpan songLength)
        {
            double progress = stopwatch.Elapsed.TotalMinutes / songLength.TotalMinutes;
            string progressBar = "`";

            for(int i = 1; i < progress * 30; i++)
            {
                progressBar += "▬";
            }
            progressBar += "🔘";
            for (int i = 1; i <  30 - (progress * 30); i++)
            {
                progressBar += "▬";
            }
            progressBar += "`";
            return progressBar;
        }

        public async Task HandleSongs(IMessageChannel channel)
        {
            var _settings = guildOptions[(channel as IGuildChannel).Guild.Id].Settings;
            if (_settings.voiceClient != null && _settings.playList.Any() && !_settings._playing)
            {
                _settings._playing = true;
                var song = _settings.playList.First();

                /*
                if(_settings.loopSong)
                    _settings.playList.Add(_settings.currentSong);
                if (_settings.loopQueue)
                {
                    _settings.playList.RemoveAt(0);
                    _settings.playList.Add(_settings.currentSong);
                }
                else*/

                _settings.playList.RemoveAt(0);

                _settings.stopwatch.Restart();

                await channel.SendMessageAsync($":musical_note: **Now playing:** {song.Title}");
                _settings.currentSong = song;
                await SendAudioAsync((channel as IGuildChannel).Guild, channel, song.Url);
            }
        }


        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
                return;
            if (target.Guild.Id != guild.Id)
                return;

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
                guildOptions.TryAdd(guild.Id, new GuildSettings
                {
                    Song = null,
                    Settings = new Settings
                    {
                        currentSong = null,
                        playList = new List<Song>(),
                        voiceClient = audioClient
                    }
                });
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                guildOptions.TryRemove(guild.Id, out GuildSettings settings);

                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string songUrl)
        {
            var _settings = guildOptions[guild.Id].Settings;
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
                var output = CreateStream(songUrl).StandardOutput.BaseStream;

                if (!_settings.cancellationToken.IsCancellationRequested) { 
                    _source = _settings.cancellationToken;
                }
                else
                {
                    _settings.cancellationToken = new CancellationTokenSource();
                    _source = _settings.cancellationToken;
                }

                //await channel.SendMessageAsync(songUrl);
                // You can change the bitrate of the outgoing stream with an additional argument to CreatePCMStream().
                // If not specified, the default bitrate is 96*1024.
                var stream = client.CreatePCMStream(AudioApplication.Music, bufferMillis: 500);
                await output.CopyToAsync(stream, 81920, _source.Token);

                await stream.FlushAsync().ConfigureAwait(false);

                if (!_settings.playList.Any())
                {
                    _source.Dispose();
                    _source = null;
                    _settings.currentSong = null;
                    _settings._playing = false;
                }
                else
                {
                    _source.Cancel();
                    _settings.currentSong = null;
                    _settings._playing = false;
                    await HandleSongs(channel);
                }
            }
        }

        private Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments =
                    $"-reconnect 1 -reconnect_streamed 1 -reconnect_delay_max 5 -err_detect ignore_err -i {path} -filter:a \"volume=0.03\" -f s16le -ar 48000 -vn -ac 2 pipe:1 -loglevel fatal",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            return Process.Start(ffmpeg);
        }

        public List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }
    }

    public class GuildSettings
    {
        public Settings Settings { get; set; }
        public Song Song { get; set; }
    }

    public class Settings
    {
        public Stopwatch stopwatch = new Stopwatch();
        public CancellationTokenSource cancellationToken = new CancellationTokenSource();
        public bool _playing;
        public Song currentSong;
        public List<Song> playList = new List<Song>();
        public IMessageChannel textchannel = null;
        public IAudioClient voiceClient;
        public bool loopSong = false;
        public bool loopQueue = false;
    }

    public class Song
    {
        public TimeSpan Duration;
        public string Title;
        public string Url;
        public string Thumbnail;
    }

    public class Youtube
    {
        public async Task<Song> Download(string url)
        {
            string[] data;
            Regex reg = new Regex("(youtube\\.com|youtu\\.be)\\/(watch)\\?(v=).*");
            if (reg.IsMatch(url))
            {
                data = (await GetVideoAsync(url)).Split('\n');
            }
            else
            {
                data = (await GetSearchResultAsync(url)).Split('\n');
            }
            if (data.Length < 6)
                return null;

            if (!TimeSpan.TryParseExact(data[4],
                new[] {"ss", "m\\:ss", "mm\\:ss", "h\\:mm\\:ss", "hh\\:mm\\:ss", "hhh\\:mm\\:ss"},
                CultureInfo.InvariantCulture, out var time))
                time = TimeSpan.FromHours(24);

            if (time.TotalMinutes > 20)
                return null;

            return new Song
            {
                Title = data[0],
                Duration = time,
                Url = data[2],
                Thumbnail = data[3]
            };
        }

        public async Task<bool> GetYoutubeSong(string url, Settings settings)
        {
            var song = await Download(url);
            if (song == null)
                return false;
            if (!settings.playList.Contains(song))
            {
                settings.playList.Add(song);
                return true;
            }
            return false;
        }


        public async Task<string> GetVideoAsync(string url)
        {
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "youtube-dl",
                    Arguments = $"-f bestaudio -e --get-url --get-id --get-thumbnail --get-duration --no-check-certificate {url}",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
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

        public async Task<string> GetSearchResultAsync(string url)
        {
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "youtube-dl",
                    Arguments = $"-f bestaudio -e --get-url --get-id --get-thumbnail --get-duration --no-check-certificate \"ytsearch:{url}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
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
    }
}