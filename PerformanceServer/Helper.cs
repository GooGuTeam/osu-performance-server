// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using System.Security.Cryptography;

namespace PerformanceServer
{
    public static class Helper
    {
        public static Ruleset GetRuleset(INeedsRuleset body, int defaultRulesetId = -1)
        {
            Ruleset ruleset;
            if (!string.IsNullOrEmpty(body.RulesetName))
            {
                ruleset = GetRuleset(body.RulesetName);
            }
            else if (body.RulesetId != null)
            {
                ruleset = GetRuleset(body.RulesetId.Value);
            }
            else if (defaultRulesetId >= -1)
            {
                ruleset = GetRuleset(defaultRulesetId);
            }
            else
            {
                throw new ArgumentException("No ruleset provided.");
            }

            return ruleset;
        }

        public static Ruleset GetRuleset(int rulesetId)
        {
            return rulesetId switch
            {
                0 => GetRuleset("osu"),
                1 => GetRuleset("taiko"),
                2 => GetRuleset("fruits"),
                3 => GetRuleset("mania"),
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }

        public static Ruleset GetRuleset(string shortName)
        {
            return shortName switch
            {
                "osu" => new OsuRuleset(),
                "taiko" => new TaikoRuleset(),
                "fruits" or "catch" => new CatchRuleset(),
                "mania" => new ManiaRuleset(),
                _ => throw new ArgumentException("Invalid ruleset name provided.")
            };
        }

        public static string ComputeMd5(string input)
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}