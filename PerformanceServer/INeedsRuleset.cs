// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace PerformanceServer
{
    public interface INeedsRuleset
    {
        [JsonProperty("ruleset_id")] int? RulesetId { get; set; }
        [JsonProperty("ruleset")] string? RulesetName { get; set; }
    }
}