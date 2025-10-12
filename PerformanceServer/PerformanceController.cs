// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace PerformanceServer
{
    public class PerformanceRequestBody
    {
        [JsonProperty("beatmap_id")] public int BeatmapId { get; set; }
        [JsonProperty("checksum")] public string? Checksum { get; set; }
        [JsonProperty("mods")] public List<APIMod> Mods { get; set; } = [];
        [JsonProperty("is_legacy")] public bool IsLegacy { get; set; }
        [JsonProperty("accuracy")] public float Accuracy { get; set; }
        [JsonProperty("ruleset_id")] public int RulesetId { get; set; }
        [JsonProperty("combo")] public int Combo { get; set; }
        [JsonProperty("statistics")] public Dictionary<HitResult, int> Statistics { get; set; } = new();
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
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<PerformanceAttributes>> CalculatePerformance(
            [FromBody] PerformanceRequestBody body)
        {
            Ruleset ruleset = Helper.GetRuleset(body.RulesetId);
            List<Mod> mods = body.Mods.Select(m => m.ToMod(ruleset)).ToList();
            if (body.IsLegacy && !mods.Any(m => m is ModClassic))
            {
                Mod? classicMod = ruleset.CreateModFromAcronym("CL");
                if (classicMod != null)
                    mods.Add(classicMod);
            }

            ScoreInfo scoreInfo = new()
            {
                IsLegacyScore = body.IsLegacy,
                Ruleset = new RulesetInfo { OnlineID = body.RulesetId },
                BeatmapInfo = new BeatmapInfo { OnlineID = body.BeatmapId },
                Statistics = body.Statistics,
                Mods = mods.ToArray(),
                Accuracy = body.Accuracy,
                MaxCombo = body.Combo,
            };
            ProcessorWorkingBeatmap workingBeatmap;
            if (body.BeatmapFile != null)
            {
                workingBeatmap = new ProcessorWorkingBeatmap(body.BeatmapFile);
            }
            else
            {
                try
                {
                    Beatmap beatmap =
                        await ProcessorWorkingBeatmap.ReadById(body.BeatmapId, body.Checksum ?? "");
                    workingBeatmap = new ProcessorWorkingBeatmap(beatmap);
                }
                catch (InvalidOperationException)
                {
                    return StatusCode(503, "Failed to fetch beatmap from online.");
                }
            }

            DifficultyAttributes? difficultyAttributes =
                ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(scoreInfo.Mods);
            PerformanceCalculator? performanceCalculator = ruleset.CreatePerformanceCalculator();
            PerformanceAttributes? performanceAttributes =
                performanceCalculator?.Calculate(scoreInfo, difficultyAttributes);
            return performanceAttributes == null
                ? BadRequest("Failed to calculate performance attributes.")
                : Ok(performanceAttributes);
        }
    }
}