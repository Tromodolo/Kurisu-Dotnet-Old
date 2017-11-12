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

namespace KatBot
{
    internal class Katarina
    {
        private static DiscordSocketClient _client;
        private static CommandHandler _commands = new CommandHandler();
        public static Dictionary<ulong, DateTime> CommandTimer = new Dictionary<ulong, DateTime>();
        public static AudioService audio = new AudioService();
        public static DatabaseHandler db = new DatabaseHandler();
        public static IServiceProvider services;
        public static string encryptionpass;

        public static DiscordSocketClient client
        {
            get { return _client; }
        }

        public static CommandHandler commandHandler
        {
            get { return _commands; }
        }

        public static Dictionary<string, string> serverprefixes = new Dictionary<string, string>();
        public static BotData botData = new BotData();
        public static Color KatClr = new Color(242, 78, 46); //Color to use in embeds


        private static void Main(string[] args)
        {
                new Katarina().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info, // Specify console verbose information level.
                AlwaysDownloadUsers = true, // Start the cache off with updated information.
                MessageCacheSize = 1000 // Tell discord.net how long to store messages (per channel).
            });
            _client.Log += l // Register the console log event.
                => Task.Run(()
                    => Console.WriteLine($"{string.Format("{0:HH:mm:ss tt}", DateTime.Now)} - [{l.Severity}] {l.Source}: {l.Exception?.Message ?? l.Message}"));

            try
            {
                Console.WriteLine("Enter encryption password:");
                encryptionpass = ReadPassword();
                botData = await db.fillBotDataAsync();
                await _client.LoginAsync(TokenType.Bot, botData.bottoken);
            }
            catch
            {
                Console.WriteLine("Something went wrong with logging in. Are you sure you entered the correct password?");
                Console.ReadLine();
                Environment.Exit(0);
            }
            await _client.StartAsync();

            services = ConfigureServices();

            await _commands.Install(_client, services);

            await Task.Delay(
                2000); //add extra delay so the bot has time to connect and have a ready status before writing connected message

            Console.WriteLine($"Connected as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");
            await _client.SetGameAsync("k?commands for Command List");

            _client.RoleDeleted += _client_RoleDeleted; //In case of role deletion, remove it from database.

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

            await Task.Delay(-1);
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

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
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