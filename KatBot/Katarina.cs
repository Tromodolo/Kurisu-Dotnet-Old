using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.WebSocket;
using KatBot.Database;
using KatBot.Modules.Music;
using KatBot.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Timers;
using System.Diagnostics;
using Console = Colorful.Console;

namespace KatBot
{
    internal class Katarina
    {
        private static DiscordSocketClient _client;
        private static CommandHandler _commands = new CommandHandler();
        private static Stopwatch runTime = new Stopwatch();

        public static string encryptionpass;
        public static Dictionary<ulong, DateTime> CommandTimer = new Dictionary<ulong, DateTime>();
        public static DatabaseHandler db = new DatabaseHandler();
        public static IServiceProvider services;
        public static Dictionary<string, string> serverprefixes = new Dictionary<string, string>();
        public static BotData botData = new BotData();
        public static int CommandUsage { get; set; }

        public static Color KatClr = new Color(242, 78, 46); //Color to use in embeds 
        public static System.Drawing.Color DefaultColor = System.Drawing.Color.FromArgb(28, 255, 145);
        public static System.Drawing.Color ConfirmColor = System.Drawing.Color.FromArgb(28, 255, 73);
        public static System.Drawing.Color ErrorColor = System.Drawing.Color.FromArgb(255, 43, 28);
        public static System.Drawing.Color KurisuColor = System.Drawing.Color.FromArgb(252, 127, 118);


        public static DiscordSocketClient client
        {
            get { return _client; }
        }

        public static CommandHandler commandHandler
        {
            get { return _commands; }
        }

        private static void Main(string[] args)
        {
                new Katarina().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true, // Start the cache off with updated information.
                MessageCacheSize = 1000 // Tell discord.net how long to store messages (per channel).
            });

            try
            {
                runTime.Start();
                Console.WriteLine("Enter encryption password:", DefaultColor);
                encryptionpass = ReadPassword();
                botData = await db.fillBotDataAsync();
                await _client.LoginAsync(TokenType.Bot, botData.bottoken);
            }
            catch
            {
                Console.WriteLine("Something went wrong with logging in. Are you sure you entered the correct password?", ErrorColor);
                Console.ReadLine();
                Environment.Exit(0);
            }
            await _client.StartAsync();

            services = ConfigureServices();
            await _commands.Install(_client, services);

            //add extra delay so the bot has time to connect and have a ready status before writing connected message
            await Task.Delay(2000); 

            Console.WriteLine($"Connected as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}", KurisuColor);

            await _client.SetGameAsync("k?commands for Command List");

            //Register client events
            _client.RoleDeleted += _client_RoleDeleted; //In case of role deletion, remove it from database.
            _client.LeftGuild += _client_LeftGuild;
            _client.JoinedGuild += _client_JoinedGuild;

            foreach(IGuild guild in _client.Guilds)
            {
                if(!await db.checkServerPrefix(guild))
                {
                    await db.setServerDefaults(guild);
                    serverprefixes.Add(guild.Id.ToString(), "k?");
                }
                else
                {
                    serverprefixes.Add(guild.Id.ToString(), await db.getServerPrefix(guild));
                }
            }

            Timer statsTimer = new Timer();
            statsTimer.Interval = 1000;
            statsTimer.Elapsed += StatsTimer_Elapsed;
            statsTimer.Start();

            await Task.Delay(-1);
        }

        private void StatsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.SetCursorPosition(0, 3);
            Console.WriteLine("Guilds:        " + _client.Guilds.Count, DefaultColor);
            Console.WriteLine("Users:         " + _client.Guilds.SelectMany(x => x.Users).Count().ToString(), DefaultColor);
            Console.WriteLine("Uptime:        " + runTime.Elapsed.ToString("hh\\:mm\\:ss"), DefaultColor);
            Console.WriteLine("Commands Run:  " + CommandUsage, DefaultColor);
        }

        private Task _client_JoinedGuild(SocketGuild arg)
        {
            Console.WriteLine("Joined server: " + arg.Name, ConfirmColor);
            return Task.CompletedTask;
        }

        private Task _client_LeftGuild(SocketGuild arg)
        {
            Console.WriteLine("Left server: " + arg.Name, ErrorColor);
            return Task.CompletedTask;
        }

        private async Task _client_RoleDeleted(SocketRole arg)
        {
            if (await db.checkServerRole(arg))
                await db.removeRole(arg);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(db)
                .AddSingleton(new AudioService())
                .AddSingleton(new InteractiveService(_client))
                .BuildServiceProvider();
        }

        public static string ReadPassword()
        {
            var password = "";
            var info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        var pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    }
}