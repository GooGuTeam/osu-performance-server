// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using PerformanceServer.Helpers;

namespace PerformanceServer
{
    public class RequestBody
    {
        [JsonProperty("beatmap_id")] public int BeatmapId { get; set; }
        [JsonProperty("checksum")] public string Checksum { get; set; }
        [JsonProperty("mods")] public List<APIMod> Mods { get; set; }
        [JsonProperty("is_legacy")] public bool IsLegacy { get; set; }
        [JsonProperty("accuracy")] public float Accuracy { get; set; }
        [JsonProperty("ruleset_id")] public int RulesetId { get; set; }
        [JsonProperty("combo")] public int Combo { get; set; }
        [JsonProperty("statistics")] public Dictionary<HitResult, int> Statistics { get; set; }
        [JsonProperty("beatmap_file")] public string? BeatmapFile { get; set; }
    }

    [ApiController]
    [Route("performance")]
    public class PerformanceController : ControllerBase
    {
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PerformanceAttributes>> CalculatePerformance([FromBody] RequestBody body)
        {
            var ruleset = RulesetHelper.GetRuleset(body.RulesetId);
            var scoreInfo = new ScoreInfo
            {
                IsLegacyScore = body.IsLegacy,
                Ruleset = new RulesetInfo { OnlineID = body.RulesetId },
                BeatmapInfo = new BeatmapInfo { OnlineID = body.BeatmapId, MD5Hash = body.Checksum },
                Statistics = body.Statistics,
                Mods = body.Mods.Select(m => m.ToMod(ruleset)).ToArray(),
                Accuracy = body.Accuracy,
                Combo = body.Combo,
            };
            ProcessorWorkingBeatmap workingBeatmap;
            if (body.BeatmapFile != null)
            {
                workingBeatmap = new ProcessorWorkingBeatmap(body.BeatmapFile);
            }
            else
            {
                var beatmap = await ProcessorWorkingBeatmap.ReadFromOnlineAsync(body.BeatmapId);
                workingBeatmap = new ProcessorWorkingBeatmap(beatmap);
            }

            var difficultyAttributes = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(scoreInfo.Mods);
            var performanceCalculator = ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);
            return performanceAttributes;
        }
    }
}