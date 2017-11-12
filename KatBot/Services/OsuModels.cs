using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KatBot.Services
{
    public class OsuModels
    {
        public class OsuBeatMap
        {
            [JsonProperty("beatmapset_id")]
            public ulong BeatmapsetId { get; set; }

            [JsonProperty("beatmap_id")]
            public ulong BeatmapId { get; set; }

            [JsonProperty("approved")]
            private string Approved { get; set; }

            [JsonProperty("total_length")]
            public int TotalLength { get; set; }

            [JsonProperty("hit_length")]
            public int HitLength { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("file_md5")]
            public string FileMD5 { get; set; }

            [JsonProperty("diff_size")]
            public double CS { get; set; }

            [JsonProperty("diff_overall")]
            public double OD { get; set; }

            [JsonProperty("diff_approach")]
            public double AR { get; set; }

            [JsonProperty("diff_drain")]
            public double HP { get; set; }

            [JsonProperty("mode")]
            public int Mode { get; set; }

            [JsonProperty("approved_date")]
            public DateTime? ApprovedDate { get; set; }

            [JsonProperty("last_update")]
            public DateTime? LastUpdate { get; set; }

            [JsonProperty("artist")]
            public string Artist { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("creator")]
            public string Creator { get; set; }

            [JsonProperty("bpm")]
            public double BPM { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }

            [JsonProperty("tags")]
            public string Tags { get; set; }

            [JsonProperty("genre_id")]
            public int GenreId { get; set; }

            [JsonProperty("language_id")]
            public int LanguageId { get; set; }

            [JsonProperty("favourite_count")]
            public int FavouriteCount { get; set; }

            [JsonProperty("playcount")]
            public int PlayCount { get; set; }

            [JsonProperty("passcount")]
            public int PassCount { get; set; }

            [JsonProperty("max_combo")]
            public int? MaxCombo { get; set; }

            [JsonProperty("difficultyrating")]
            public double DifficultyRating { get; set; }
        }

        public class OsuScore
        {
            [JsonProperty("score")]
            public int Score { get; set; }

            [JsonProperty("username")]
            public string UserName { get; set; }

            [JsonProperty("count300")]
            public int Count300 { get; set; }

            [JsonProperty("count100")]
            public int Count100 { get; set; }

            [JsonProperty("count50")]
            public int Count50 { get; set; }

            [JsonProperty("countmiss")]
            public int CountMiss { get; set; }

            [JsonProperty("countkatu")]
            public int CountKatu { get; set; }

            [JsonProperty("countgeki")]
            public int CountGeki { get; set; }

            [JsonProperty("perfect")]
            public int Perfect { get; set; }

            [JsonProperty("enabled_mods")]
            public ulong EnabledMods { get; set; }

            [JsonProperty("user_id")]
            public ulong UserId { get; set; }

            [JsonProperty("date")]
            public DateTime Date { get; set; }

            [JsonProperty("rank")]
            public string Rank { get; set; }

            [JsonProperty("pp")]
            public double Pp { get; set; }
        }

        public class OsuUserBestScore
        {
            [JsonProperty("beatmap_id")]
            public ulong BeatmapId { get; set; }

            [JsonProperty("score")]
            public int Score { get; set; }

            [JsonProperty("maxcombo")]
            public int MaxCombo { get; set; }

            [JsonProperty("count300")]
            public int Count300 { get; set; }

            [JsonProperty("count100")]
            public int Count100 { get; set; }

            [JsonProperty("count50")]
            public int Count50 { get; set; }

            [JsonProperty("countmiss")]
            public int CountMiss { get; set; }

            [JsonProperty("countkatu")]
            public int CountKatu { get; set; }

            [JsonProperty("countgeki")]
            public int CountGeki { get; set; }

            [JsonProperty("perfect")]
            public int Perfect { get; set; }

            [JsonProperty("enabled_mods")]
            public ulong EnabledMods { get; set; }

            [JsonProperty("user_id")]
            public ulong UserId { get; set; }

            [JsonProperty("date")]
            public DateTime Date { get; set; }

            [JsonProperty("rank")]
            public string Rank { get; set; }

            [JsonProperty("pp")]
            public double Pp { get; set; }
        }

        public class OsuUserRecentScore
        {
            [JsonProperty("beatmap_id")]
            public ulong BeatmapId { get; set; }

            [JsonProperty("score")]
            public int Score { get; set; }

            [JsonProperty("maxcombo")]
            public int MaxCombo { get; set; }

            [JsonProperty("count300")]
            public int Count300 { get; set; }

            [JsonProperty("count100")]
            public int Count100 { get; set; }

            [JsonProperty("count50")]
            public int Count50 { get; set; }

            [JsonProperty("countmiss")]
            public int CountMiss { get; set; }

            [JsonProperty("countkatu")]
            public int CountKatu { get; set; }

            [JsonProperty("countgeki")]
            public int CountGeki { get; set; }

            [JsonProperty("perfect")]
            public int Perfect { get; set; }

            [JsonProperty("enabled_mods")]
            public ulong EnabledMods { get; set; }

            [JsonProperty("user_id")]
            public ulong UserId { get; set; }

            [JsonProperty("date")]
            public DateTime Date { get; set; }

            [JsonProperty("rank")]
            public string Rank { get; set; }

            [JsonProperty("pp")]
            public double Pp { get; set; }
        }

        public class OsuUser
        {
            [JsonProperty("user_id")]
            public long Userid { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("playcount")]
            public uint PlayCount { get; set; }

            [JsonProperty("accuracy")]
            public double Accuracy { get; set; }

            [JsonProperty("pp_rank")]
            public uint GlobalRank { get; set; }

            [JsonProperty("pp_raw")]
            public float Pp { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("pp_country_rank")]
            public uint RegionalRank { get; set; }

            [JsonProperty("level")]
            public float Level { get; set; }

            [JsonProperty("total_score")]
            public ulong TotalScore { get; set; }

            [JsonProperty("ranked_score")]
            public ulong RankedScore { get; set; }

            [JsonProperty("count300")]
            public uint Count300 { get; set; }

            [JsonProperty("count100")]
            public uint Count100 { get; set; }

            [JsonProperty("count50")]
            public uint Count50 { get; set; }

            [JsonProperty("count_rank_ss")]
            public int SSRank { get; set; }

            [JsonProperty("count_rank_s")]
            public int SRank { get; set; }

            [JsonProperty("count_rank_a")]
            public int ARank { get; set; }

            [JsonProperty("events")]
            public List<Events> Events { get; set; }
        }

        public class Events
        {
            [JsonProperty("display_html")]
            public string DisplayHtml { get; set; }

            [JsonProperty("beatmap_id")]
            public ulong BeatmapId { get; set; }

            [JsonProperty("beatmapset_id")]
            public ulong BeatmapsetId { get; set; }

            [JsonProperty("date")]
            public DateTime? Date { get; set; }

            [JsonProperty("epicfactor")]
            public ushort Epicfactor { get; set; }
        }
    }
}