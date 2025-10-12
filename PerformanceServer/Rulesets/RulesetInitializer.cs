// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

namespace PerformanceServer.Rulesets
{
    public class RulesetInitializer(IRulesetManager rulesetManager, ILogger<RulesetInitializer> logger)
        : IHostedService
    {
        private readonly IRulesetManager _rulesetManager = rulesetManager;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Initialized all rulesets");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}