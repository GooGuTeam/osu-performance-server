// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.


using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Skinning;

namespace PerformanceServer
{
    public class ProcessorWorkingBeatmap(Beatmap beatmap) : WorkingBeatmap(beatmap.BeatmapInfo, null)
    {
        public ProcessorWorkingBeatmap(string beatmapFile) : this(ReadFromOsuFile(beatmapFile)) { }
        
        private const string OsuUrl = "https://osu.ppy.sh"; 

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

        public static async Task<Beatmap> ReadFromOnlineAsync(int beatmapId, string webUrl = OsuUrl)
        {
            var request = new WebRequest($"{webUrl}/osu/{beatmapId}");
            await request.PerformAsync();
            string? response = request.GetResponseString();
            if (response == null)
            {
                throw new Exception("Failed to fetch beatmap from online.");
            }

            return ReadFromOsuFile(response);
        }

        protected override IBeatmap GetBeatmap() => beatmap;
        public override Texture GetBackground() => null!;
        protected override Track GetBeatmapTrack() => null!;
        protected override ISkin GetSkin() => null!;
        public override Stream GetStream(string storagePath) => null!;
    }
}