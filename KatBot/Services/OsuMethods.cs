using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static KatBot.Services.OsuModels;

namespace KatBot.Services
{
    public class OsuMethods
    {
        private const string RootDomain = "https://osu.ppy.sh";
        private const string GetBeatmapsUrl = "/api/get_beatmaps";
        private const string GetUserUrl = "/api/get_user";
        private const string GetScoresUrl = "/api/get_scores";
        private const string GetUserBestUrl = "/api/get_user_best";
        private const string GetUserRecentUrl = "/api/get_user_recent";
        private const string ApiKeyParameter = "?k=";
        private const string UserParameter = "&u=";
        private const string MatchParameter = "&mp=";
        private const string LimitParameter = "&limit=";
        private const string BeatmapParameter = "&b=";
        private const string ModeParameter = "&m=";


        public static async Task<List<OsuUserBestScore>> GetUserBestAsync(string userId, int gamemode, int limit = 5)
        {
            var urlRequest =
                await GetAsync(
                    $"{RootDomain}{GetUserBestUrl}{ApiKeyParameter}{Katarina.botData.osuapikey}{UserParameter}{userId}{ModeParameter}{gamemode}{LimitParameter}{limit}");
            var maps = JsonConvert.DeserializeObject<List<OsuUserBestScore>>(urlRequest);
            return maps;
        }

        public static async Task<List<OsuUserBestScore>> GetUserRecentAsync(string userId, int gamemode, int limit = 5)
        {
            var urlRequest =
                await GetAsync(
                    $"{RootDomain}{GetUserRecentUrl}{ApiKeyParameter}{Katarina.botData.osuapikey}{UserParameter}{userId}{ModeParameter}{gamemode}{LimitParameter}{limit}");
            var maps = JsonConvert.DeserializeObject<List<OsuUserBestScore>>(urlRequest);
            return maps;
        }

        public static async Task<OsuBeatMap> GetBeatmapAsync(ulong beatmapId, int gamemode)
        {
            var urlRequest =
                await GetAsync(
                    $"{RootDomain}{GetBeatmapsUrl}{ApiKeyParameter}{Katarina.botData.osuapikey}{BeatmapParameter}{beatmapId}");
            var maps = JsonConvert.DeserializeObject<List<OsuBeatMap>>(urlRequest);
            if (maps.Count > 0)
                return maps[0];
            return null;
        }

        public static async Task<OsuUser> GetUserAsync(string username, int gamemode)
        {
            var urlRequest =
                await GetAsync(
                    $"{RootDomain}{GetUserUrl}{ApiKeyParameter}{Katarina.botData.osuapikey}{UserParameter}{username}{ModeParameter}{gamemode}");
            var user = JsonConvert.DeserializeObject<List<OsuUser>>(urlRequest);
            return user[0];
        }

        private static async Task<string> GetAsync(string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(RootDomain);
                var message = await client.GetStringAsync(url);
                return message; /*
                if (message.StatusCode == HttpStatusCode.OK)
                {
                    return await message.Content.ReadAsStringAsync();
                }
                else
                {
                    //(await message.Content.ReadAsStringAsync());
                    return null;
                }*/
            }
        }
    }
}