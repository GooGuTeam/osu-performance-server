// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using PerformanceServer.Helpers;

namespace PerformanceServer
{
    public class DifficultyRequestBody
    {
        [JsonProperty("beatmap_id")] public int BeatmapId { get; set; }
        [JsonProperty("checksum")] public string? Checksum { get; set; }
        [JsonProperty("mods")] public List<APIMod> Mods { get; set; } = [];
        [JsonProperty("ruleset_id")] public int? RulesetId { get; set; }
        [JsonProperty("beatmap_file")] public string? BeatmapFile { get; set; }
    }

    [ApiController]
    [Route("difficulty")]
    public class BeatmapDifficultyController : ControllerBase
    {
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DifficultyAttributes>> CalculateDifficulty([FromBody] DifficultyRequestBody body)
        {
            ProcessorWorkingBeatmap workingBeatmap;
            if (body.BeatmapFile != null)
            {
                workingBeatmap = new ProcessorWorkingBeatmap(body.BeatmapFile);
            }
            else
            {
                Beatmap beatmap = await ProcessorWorkingBeatmap.ReadFromOnlineAsync(body.BeatmapId);
                workingBeatmap = new ProcessorWorkingBeatmap(beatmap);
            }

            Ruleset ruleset = RulesetHelper.GetRuleset(body.RulesetId ?? workingBeatmap.BeatmapInfo.Ruleset.OnlineID);
            Mod[] mods = body.Mods.Select(m => m.ToMod(ruleset)).ToArray();
            DifficultyAttributes? difficultyAttributes =
                ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods);

            return Ok(difficultyAttributes);
        }
    }
}