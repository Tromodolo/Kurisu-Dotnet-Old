using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using KurisuBot.Services;
using Microsoft.EntityFrameworkCore;

namespace KurisuBot.Database
{
    public class DatabaseHandler
    {
        private readonly SqliteContext context;

        public DatabaseHandler()
        {
            context = new SqliteContext();
            context.Database.EnsureCreated();
        }

        public class SqliteContext : DbContext
        {
            public DbSet<selfroles> selfroles { get; set; }
            public DbSet<botdata> botdata { get; set; }
            public DbSet<serversettings> serversettings { get; set; }


            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Filename =./Database.db");
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                base.OnConfiguring(optionsBuilder);
            }

            protected override void OnModelCreating(ModelBuilder modelbuilder)
            {
                foreach (var relationship in modelbuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                    relationship.DeleteBehavior = DeleteBehavior.Restrict;
                base.OnModelCreating(modelbuilder);
            }
        }

        public class botdata
        {
            [Key]
            public int row { get; set; }

            public string bottoken { get; set; }

            public string osuapikey { get; set; }

            public string myanimelistname { get; set; }

            public string myanimelistpass { get; set; }

            public string googleapikey { get; set; }

            public string csecode { get; set; }
        }


        public class selfroles
        {
            [Key]
            public string roleid { get; set; }

            public string serverid { get; set; }

            [MaxLength(50)]
            public string rolename { get; set; }
        }

        public class serversettings
        {
            [Key]
            public string serverid { get; set; }

            public string commandprefix { get; set; }
        }


        public async Task<BotData> fillBotDataAsync()
        {
            var contextAsync = new SqliteContext();

            var data = await contextAsync.botdata.Where(x => x.row == 1).FirstAsync();

            await contextAsync.SaveChangesAsync();

            return new BotData
            {
                bottoken = EncryptionService.DecryptStringAES(data.bottoken, Kurisu.encryptionpass),
                csecode = EncryptionService.DecryptStringAES(data.csecode, Kurisu.encryptionpass),
                googleapikey = EncryptionService.DecryptStringAES(data.googleapikey, Kurisu.encryptionpass),
                myanimelistname = EncryptionService.DecryptStringAES(data.myanimelistname, Kurisu.encryptionpass),
                myanimelistpass = EncryptionService.DecryptStringAES(data.myanimelistpass, Kurisu.encryptionpass),
                osuapikey = EncryptionService.DecryptStringAES(data.osuapikey, Kurisu.encryptionpass)
            };
        }

        public async Task addRole(IRole role)
        {
            var contextAsync = new SqliteContext();

            await contextAsync.selfroles.AddAsync(new selfroles
            {
                serverid = EncryptionService.EncryptStringAES(role.Guild.Id.ToString(), Kurisu.encryptionpass),
                roleid = EncryptionService.EncryptStringAES(role.Id.ToString(), Kurisu.encryptionpass),
                rolename = EncryptionService.EncryptStringAES(role.Name, Kurisu.encryptionpass)
            });


            await contextAsync.SaveChangesAsync();
        }

        public async Task removeRole(IRole role)
        {
            var contextAsync = new SqliteContext();

            var res = await contextAsync.selfroles.FirstOrDefaultAsync(
                x => EncryptionService.DecryptStringAES(x.serverid, Kurisu.encryptionpass) == role.Id.ToString() &&
                     EncryptionService.DecryptStringAES(x.rolename, Kurisu.encryptionpass) == role.Name);

            contextAsync.selfroles.Remove(res);

            await contextAsync.SaveChangesAsync();
        }

        public async Task<string> getServerRole(ulong serverid)
        {
            var contextAsync = new SqliteContext();
            var roles = await contextAsync.selfroles
                .Where(x => EncryptionService.DecryptStringAES(x.serverid, Kurisu.encryptionpass) ==
                            serverid.ToString())
                .Select(x => EncryptionService.DecryptStringAES(x.rolename, Kurisu.encryptionpass)).ToListAsync();
            var fullist = string.Join(", ", roles);
            return fullist;
        }

        public async Task<bool> checkServerRole(IRole role)
        {
            var contextAsync = new SqliteContext();
            if (await contextAsync.selfroles.CountAsync(
                    x => EncryptionService.DecryptStringAES(x.serverid, Kurisu.encryptionpass) ==
                         role.Guild.Id.ToString() &&
                         EncryptionService.DecryptStringAES(x.roleid, Kurisu.encryptionpass) == role.Id.ToString()) >
                0)
                return true;
            return false;
        }

        public async Task<string> getServerPrefix(IGuild server)
        {
            var contextAsync = new SqliteContext();
            var prefix = await contextAsync.serversettings.FirstAsync(x => x.serverid == server.Id.ToString());
            return prefix.commandprefix;
        }

        public async Task<bool> checkServerPrefix(IGuild server)
        {
            var contextAsync = new SqliteContext();
            var prefix = await contextAsync.serversettings.FirstAsync(x => x.serverid == server.Id.ToString());
            if(prefix == null || prefix.commandprefix == null)
            {
                return false;
            }
            return true;
        }

        public async Task updateServerprefix(IGuild server, string prefix)
        {
            var contextAsync = new SqliteContext();
            var serverprefix = await contextAsync.serversettings.FirstAsync(x => x.serverid == server.Id.ToString());
            if(serverprefix != null)
            {
                serverprefix.commandprefix = prefix;
                contextAsync.Update(serverprefix);
            }
            
            await contextAsync.SaveChangesAsync();
        }


        public async Task setServerDefaults(IGuild server)
        {
            var contextAsync = new SqliteContext();

            await contextAsync.serversettings.AddAsync(new serversettings
            {
                serverid = server.Id.ToString(),
                commandprefix = "k?"
            });

            await contextAsync.SaveChangesAsync();
        }
    }
}