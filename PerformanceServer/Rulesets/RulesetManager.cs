// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using System.Reflection;

namespace PerformanceServer.Rulesets
{
    public class RulesetManager : IRulesetManager
    {
        private readonly ILogger<RulesetManager> _logger;

        private const string RulesetLibraryPrefix = "osu.Game.Rulesets";

        private readonly Dictionary<string, Ruleset> _rulesets = new();
        private readonly Dictionary<int, Ruleset> _rulesetsById = new();

        public RulesetManager(ILogger<RulesetManager> logger)
        {
            _logger = logger;
            LoadOfficialRulesets();
            LoadFromDisk();
        }

        private void AddRuleset(Ruleset ruleset)
        {
            if (!_rulesets.TryAdd(ruleset.ShortName, ruleset))
            {
                _logger.LogWarning("Ruleset with short name {shortName} already exists, skipping.", ruleset.ShortName);
                return;
            }

            if (ruleset is not ILegacyRuleset legacyRuleset)
            {
                return;
            }

            if (!_rulesetsById.TryAdd(legacyRuleset.LegacyID, ruleset))
            {
                _logger.LogWarning("Ruleset with ID {id} already exists, skipping.", legacyRuleset.LegacyID);
            }
        }

        private void LoadOfficialRulesets()
        {
            foreach (Ruleset ruleset in (List<Ruleset>)
                     [new OsuRuleset(), new TaikoRuleset(), new CatchRuleset(), new ManiaRuleset()])
            {
                AddRuleset(ruleset);
            }

            _rulesets["catch"] = _rulesets["fruits"];
        }

        private void LoadFromDisk()
        {
            if (!Directory.Exists(AppSettings.RulesetsPath))
            {
                return;
            }

            string[] rulesets = Directory.GetFiles(AppSettings.RulesetsPath, $"{RulesetLibraryPrefix}.*.dll");

            foreach (string ruleset in rulesets.Where(f => !f.Contains(@"Tests")))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(ruleset);
                    Type? rulesetType = assembly.GetTypes()
                        .FirstOrDefault(t => t.IsSubclassOf(typeof(Ruleset)) && !t.IsAbstract);

                    if (rulesetType == null)
                    {
                        continue;
                    }

                    Ruleset instance = (Ruleset)Activator.CreateInstance(rulesetType)!;
                    _logger.LogInformation("Loading ruleset {ruleset}", ruleset);
                    AddRuleset(instance);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to load ruleset from {ruleset}: {ex}", ruleset, ex);
                }
            }
        }

        public Ruleset GetRuleset(int rulesetId)
        {
            return _rulesetsById.TryGetValue(rulesetId, out Ruleset? ruleset)
                ? ruleset
                : throw new ArgumentException("Invalid ruleset ID provided.");
        }

        public Ruleset GetRuleset(string shortName)
        {
            return _rulesets.TryGetValue(shortName, out Ruleset? ruleset)
                ? ruleset
                : throw new ArgumentException("Invalid ruleset name provided.");
        }

        public Ruleset GetRuleset(INeedsRuleset body, int defaultRulesetId = -1)
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
    }
}