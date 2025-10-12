// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;

namespace PerformanceServer.Rulesets
{
    public interface IRulesetManager
    {
        public Ruleset GetRuleset(int rulesetId);
        public Ruleset GetRuleset(string shortName);
        public Ruleset GetRuleset(INeedsRuleset body, int defaultRulesetId = -1);
    }
}