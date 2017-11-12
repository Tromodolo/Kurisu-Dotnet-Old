using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KatBot.Services
{
    internal class CommandHandler
    {
        private static readonly Random rand = new Random();

        private static readonly string defaultPrefix = "k?";
        private DiscordSocketClient _client;
        private CommandService _cmds;
        public IServiceProvider services;

        public CommandService Commands
        {
            get { return _cmds; }
        }

        public async Task Install(DiscordSocketClient c, IServiceProvider serv)
        {
            _client = c; // Save an instance of the discord client.
            _cmds = new CommandService(); // Create a new instance of the commandservice.                              

            services = serv;

            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly()); // Load all modules from the assembly.
            _client.MessageReceived += HandleCommand; // Register the messagereceived event to handle commands.
        }

        private async Task HandleCommand(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) // Check if the received message is from a user.
                return;

            var guild = (s.Channel as IGuildChannel)?.Guild;

            var context = new SocketCommandContext(_client, msg); // Create a new command context.

            var argPos = 0;

            // Check if the message has either a string or mention prefix.
            if (msg.HasStringPrefix(defaultPrefix, ref argPos)||
                msg.HasStringPrefix(Katarina.serverprefixes[context.Guild.Id.ToString()], ref argPos))
            {
                IResult result = null;
                // Try and execute a command with the given context.
                result = await _cmds.ExecuteAsync(context, argPos, services);
                if (result.IsSuccess)
                {
                    Console.WriteLine("\nCommand " + s.Content + " used by user " + s.Author + ":");
                    Console.WriteLine("In Server: " + guild.Name + $" ({guild.Id})");
                    Console.WriteLine("In Channel: " + s.Channel.Name + $" ({s.Channel.Id})");
                }
                else if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    //await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
    }
}