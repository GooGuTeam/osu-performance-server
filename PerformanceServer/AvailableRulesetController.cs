// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

using Microsoft.AspNetCore.Mvc;
using PerformanceServer.Rulesets;

namespace PerformanceServer
{
    [ApiController]
    [Route("available_rulesets")]
    public class AvailableRulesetController(IRulesetManager manager) : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string[]>> GetAvailableRulesets()
        {
            return await Task.FromResult(Ok(new Dictionary<string, string[]>
            {
                {
                    "has_performance_calculator",
                    manager.GetHasPerformCalculatorRulesets().Select(r => r.ShortName).ToArray()
                },
                {
                    "has_difficulty_calculator",
                    manager.GetHasDifficultyCalculatorRulesets().Select(r => r.ShortName).ToArray()
                },
                { "loaded_rulesets", manager.GetRulesets().Select(r => r.ShortName).ToArray() }
            }));
        }
    }
}