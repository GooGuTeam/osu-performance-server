// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

namespace PerformanceServer
{
    public static class AppSettings
    {
        public static bool SaveBeatmapFiles { get; set; }
        public static string BeatmapsPath { get; set; } = "./beatmaps";
        public static string OsuFileWebUrl { get; set; } = "https://osu.ppy.sh/osu/{0}";
        public static int MaxBeatmapFileSize { get; set; } = 5 * 1024 * 1024; // 5 MB

        static AppSettings()
        {
            SaveBeatmapFiles = Environment.GetEnvironmentVariable("SAVE_BEATMAP_FILES")?.ToLower() == "true";
            BeatmapsPath = Environment.GetEnvironmentVariable("BEATMAPS_PATH") ?? BeatmapsPath;
            OsuFileWebUrl = Environment.GetEnvironmentVariable("OSU_FILE_WEB_URL") ?? OsuFileWebUrl;
            if (int.TryParse(Environment.GetEnvironmentVariable("MAX_BEATMAP_FILE_SIZE"), out int size))
            {
                MaxBeatmapFileSize = size;
            }
        }
    }
}