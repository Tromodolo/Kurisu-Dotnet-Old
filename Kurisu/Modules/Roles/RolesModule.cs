using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KurisuBot.Services.EmbedExtensions;

namespace KurisuBot.Modules.Roles
{
    [Name("Roles")]
    public class RolesModule : ModuleBase
    {
        //TODO: Redo everything of this
        [Command("roles")]
        [Summary("Shows list of self-assignable roles available in a server.")]
        [Remarks("roles")]
        public async Task roles()
        {
            var results = await Kurisu.db.getServerRole(Context.Guild.Id);
            if (results.Length > 1)
                await Context.Channel.SendColouredEmbedAsync("The list of roles available in this server:", results,
                    Kurisu.KurisuClr);
            else
                await Context.Channel.SendColouredEmbedAsync("The list of roles available in this server:",
                    "There are no available roles", Kurisu.KurisuClr);
        }

        [Command("giverole")]
        [Summary("Gives you a role from one of the self-assignable ones")]
        [Remarks("giverole civilian")]
        public async Task giveRole([Remainder] IRole role)
        {
            if (await Kurisu.db.checkServerRole(role))
            {
                await (Context.Message.Author as IGuildUser).AddRoleAsync(role);
                await Context.Channel.SendConfirmAsync(
                    $"{Context.Message.Author.Mention}, you now have the role: {role.Name}");
            }
            else
            {
                await Context.Channel.SendErrorAsync("That role is not one of the self-assignable roles.");
            }
        }

        [Command("takeRole")]
        [Summary("Removes a role you have from one of the self-assignable ones")]
        [Remarks("takeRole civilian")]
        public async Task takeRole([Remainder] IRole role)
        {
            if (await Kurisu.db.checkServerRole(role))
            {
                await (Context.Message.Author as IGuildUser).RemoveRoleAsync(role);
                await Context.Channel.SendConfirmAsync(
                    $"{Context.Message.Author.Mention}, you no longer have the role : {role.Name}"); //This will run whether the role was existing to begin with or not, I'll come back to this later
            }
        }
    }
}