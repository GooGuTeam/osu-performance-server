// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Skinning;

namespace PerformanceServer
{
    public class ProcessorWorkingBeatmap(Beatmap beatmap) : WorkingBeatmap(beatmap.BeatmapInfo, null)
    {
        public ProcessorWorkingBeatmap(string content) : this(ReadFromOsuFile(content))
        {
            SaveToLocalFile(BeatmapInfo.OnlineID, content);
        }

        private static string BeatmapPath(int beatmapId) => Path.Combine(AppSettings.BeatmapsPath, $"{beatmapId}.osu");


        private static Beatmap ReadFromOsuFile(string beatmapFile)
        {
            MemoryStream stream = new();
            StreamWriter writer = new(stream);
            writer.Write(beatmapFile);
            writer.Flush();
            stream.Position = 0;

            using LineBufferedReader reader = new(stream);
            Decoder<Beatmap> decoder = Decoder.GetDecoder<Beatmap>(reader);
            return decoder.Decode(reader);
        }

        public static Beatmap ReadFromLocalFile(string filePath, string checkSum = "")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Beatmap file not found.", filePath);
            string beatmapFile = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(checkSum))
            {
                return ReadFromOsuFile(beatmapFile);
            }

            string fileCheckSum = Helper.ComputeMd5(beatmapFile);
            if (!string.Equals(fileCheckSum, checkSum, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Checksum does not match.");
            return ReadFromOsuFile(beatmapFile);
        }

        private static async Task<string?> ReadContentFromUrl(string url)
        {
            using var http = new HttpClient();
            HttpResponseMessage response = await http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public static void SaveToLocalFile(int beatmapId, string content, bool overwrite = false)
        {
            string path = BeatmapPath(beatmapId);
            try
            {
                if (!Directory.Exists(AppSettings.BeatmapsPath))
                    Directory.CreateDirectory(AppSettings.BeatmapsPath);
                if (File.Exists(path))
                {
                    string beatmapFile = File.ReadAllText(path);
                    if (string.Equals(Helper.ComputeMd5(beatmapFile), Helper.ComputeMd5(content),
                            StringComparison.OrdinalIgnoreCase) && !overwrite)
                    {
                        return;
                    }
                }

                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save beatmap file: {ex.Message}");
            }
        }

        public static async Task<Beatmap> ReadFromOnlineAsync(int beatmapId)
        {
            string? response = await ReadContentFromUrl(string.Format(AppSettings.OsuFileWebUrl, beatmapId));
            Beatmap beatmap = response == null
                ? throw new InvalidOperationException("Failed to fetch beatmap from online.")
                : ReadFromOsuFile(response);
            if (!AppSettings.SaveBeatmapFiles ||
                (!string.IsNullOrEmpty(response) && response.Length > AppSettings.MaxBeatmapFileSize))
            {
                return beatmap;
            }

            try
            {
                SaveToLocalFile(beatmapId, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save beatmap file: {ex.Message}");
            }

            return response == null
                ? throw new InvalidOperationException("Failed to fetch beatmap from online.")
                : ReadFromOsuFile(response);
        }

        public static async Task<Beatmap> ReadById(int beatmapId, string checkSum = "")
        {
            try
            {
                return ReadFromLocalFile(BeatmapPath(beatmapId), checkSum);
            }
            catch (Exception ex)
            {
                if (ex is not (FileNotFoundException or InvalidOperationException))
                    throw;
                return await ReadFromOnlineAsync(beatmapId);
            }
        }

        protected override IBeatmap GetBeatmap() => beatmap;
        public override Texture GetBackground() => null!;
        protected override Track GetBeatmapTrack() => null!;
        protected override ISkin GetSkin() => null!;
        public override Stream GetStream(string storagePath) => null!;
    }
}