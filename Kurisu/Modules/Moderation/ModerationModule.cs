using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using KurisuBot.Services.EmbedExtensions;
using System.Linq;

namespace KurisuBot.Modules.Moderation
{
    [Name("Moderation")]
    public class ModerationModule : InteractiveModuleBase<ICommandContext>
    {
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans a user from the server and deletes last day of messages")]
        [Remarks("ban reddeyez")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(SocketGuildUser BanUser = null, [Remainder] string reason = null)
        {
            if (BanUser == null)
                await Context.Channel.SendErrorAsync("You need to specify a user to ban.");
            else if (BanUser == Context.Message.Author)
                await Context.Channel.SendErrorAsync("You can't ban yourself.");
            else if (BanUser.Hierarchy == int.MaxValue)
                await Context.Channel.SendErrorAsync("You can't ban the owner of a server.");
            else if (BanUser.Hierarchy >= (Context.Message.Author as SocketGuildUser).Hierarchy)
                await Context.Channel.SendErrorAsync("You can't ban someone with a role higher or equal to yours.");
            else
                try
                {
                    if (string.IsNullOrWhiteSpace(reason))
                        reason = "None";

                    var time = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Utc);

                    await Context.Channel.SendColouredEmbedAsync($"Are you sure you want to ban user: {BanUser}?",
                        $"**Send `confirm` or `cancel`**\n**Date:** {time} UTC \n**Reason:** {reason}",
                        Kurisu.KurisuClr);
                    var response = await WaitForMessage(Context.Message.Author, Context.Channel,
                        new TimeSpan(0, 0, 60));
                    if (response.Content.ToLower() == "confirm")
                    {
                        var BanDM = await BanUser.GetOrCreateDMChannelAsync();

                        await BanDM.SendErrorAsync($"You were banned from: {Context.Guild.Name}",
                            $"**Date:** {time} UTC \n" +
                            $"**Reason:** {reason}");
                        await Context.Guild.AddBanAsync(BanUser, 1);

                        await Context.Channel.SendConfirmAsync(
                            $"{Context.Message.Author.Mention}\n {BanUser} was banned from this server.");
                    }
                    else
                    {
                        await Context.Channel.SendErrorAsync($"User {BanUser} was not banned.");
                    }
                }
                catch
                {
                    await Context.Channel.SendErrorAsync(
                        $"{Context.Message.Author.Mention}\n {BanUser} could not be banned from this server.");
                }
        }

        [Command("kick", RunMode = RunMode.Async)]
        [Summary("Kicks a user from the server")]
        [Remarks("kick kbuns")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(SocketGuildUser KickUser = null, [Remainder] string reason = null)
        {
            if (KickUser == null)
                await Context.Channel.SendErrorAsync("You need to specify a user to kick.");
            else if (KickUser == Context.Message.Author)
                await Context.Channel.SendErrorAsync("You can't kick yourself.");
            else if (KickUser.Hierarchy == int.MaxValue)
                await Context.Channel.SendErrorAsync("You can't kick the owner of a server.");
            else if (KickUser.Hierarchy >= (Context.Message.Author as SocketGuildUser).Hierarchy)
                await Context.Channel.SendErrorAsync("You can't kick someone with a role higher or equal to yours.");
            else
                try
                {
                    if (string.IsNullOrWhiteSpace(reason))
                        reason = "None";

                    var time = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Utc);

                    await Context.Channel.SendColouredEmbedAsync($"Are you sure you want to kick user: {KickUser}?",
                        $"**Send `confirm` or `cancel`**\n**Date:** {time} UTC \n**Reason:** {reason}",
                        Kurisu.KurisuClr);
                    var response = await WaitForMessage(Context.Message.Author, Context.Channel,
                        new TimeSpan(0, 0, 60));
                    if (response.Content.ToLower() == "confirm")
                    {
                        var KickDM = await KickUser.GetOrCreateDMChannelAsync();

                        await KickDM.SendErrorAsync($"You were kicked from: {Context.Guild.Name}",
                            $"**Date:** {time} UTC \n" +
                            $"**Reason:** {reason}");

                        await KickUser.KickAsync();

                        await Context.Channel.SendConfirmAsync(
                            $"{Context.Message.Author.Mention}\n {KickUser} was kicked from this server.");
                    }
                    else
                    {
                        await Context.Channel.SendErrorAsync($"User {KickUser} was not kicked.");
                    }
                }
                catch
                {
                    await Context.Channel.SendErrorAsync(
                        $"{Context.Message.Author.Mention}\n {KickUser} couldn't be kicked from this server.");
                }
        }



        [Command("prune")]
        [Summary("Prunes number of messages you want, up to 100")]
        [Remarks("prune 27")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Prune(int PruneNumber = 10)
        {
            await Context.Message.DeleteAsync();

            if (PruneNumber > 100)
            {
                PruneNumber = 100;
            }

            var Messages = (await Context.Channel.GetMessagesAsync(PruneNumber + 1).Flatten().ConfigureAwait(false));
            if (Messages.FirstOrDefault()?.Id == Context.Message.Id)
                Messages = Messages.Skip(1).ToArray();
            else
                Messages = Messages.Take(PruneNumber);
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(Messages).ConfigureAwait(false);

            await Context.Channel.SendConfirmAsync($"{Context.Message.Author.Mention}\n{PruneNumber} messages were pruned");
        }


        [Command("setrole")]
        [Summary("Adds a role to self-assignable roles. Needs manage messages permissions.")]
        [Remarks("setrole civilian")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task setrole([Remainder] IRole role)
        {
            try
            {
                var authorPos = (Context.Message.Author as SocketGuildUser).Hierarchy;
                var rolePos = role.Position;
                var serverUser = await Context.Guild.GetCurrentUserAsync();
                var botPos = (serverUser as SocketGuildUser).Hierarchy;

                if (botPos <= rolePos)
                {
                    await Context.Channel.SendErrorAsync("The role needs to be below the bot in role-list.");
                }
                else if (authorPos <= rolePos)
                {
                    await Context.Channel.SendErrorAsync(
                        "You can't add a role that is equal to your own or higher to the self-assign.");
                }
                else
                {
                    await Kurisu.db.addRole(role);
                    await Context.Channel.SendConfirmAsync("Role was set as a self-assignable role.");
                }
            }
            catch
            {
                await Context.Channel.SendErrorAsync("The role is already added to self-assignable. :( ");
            }
        }

        [Command("unsetrole")]
        [Summary("Removes a role from self-assignable. Needs manage messages permissions.")]
        [Remarks("unsetrole civilian")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task unsetRole([Remainder] IRole role)
        {
            if (await Kurisu.db.checkServerRole(role))
            {
                await Kurisu.db.removeRole(role);
                await Context.Channel.SendConfirmAsync("Role was removed as a self-assignable role.");
            }
            else
            {
                await Context.Channel.SendErrorAsync("Role is not added as a self-assignable role.");
            }
        }

        [Command("setprefix")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task setprefix(string prefix)
        {
            await Kurisu.db.updateServerprefix(Context.Guild, prefix);
            await Context.Channel.SendMessageAsync("Server prefix set to: " + prefix);
        }
    }
}