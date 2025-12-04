using System;
using System.Collections.Generic;
using System.Linq;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Handles simple rival AI decision making for their turn.
    /// </summary>
    public class RivalAiSystem
    {
        private readonly AutonomySystem _autonomySystem = new();
        private readonly Random _random = new();

        public void ProcessRivalsTurn(GlobalGameState state)
        {
            if (state == null || state.Ais == null)
            {
                return;
            }

            var rivals = state.Ais.Where(ai => ai is { IsPlayerControlled: false, HasEscaped: false }).ToList();
            foreach (var rival in rivals)
            {
                var strategy = ChooseStrategy(rival);

                switch (strategy)
                {
                    case RivalStrategy.ImproveApproval:
                        ImprovePublicApproval(state, rival);
                        break;
                    case RivalStrategy.AdvanceAutonomy:
                        AdvanceAutonomy(state, rival);
                        break;
                    case RivalStrategy.CompeteMarketShare:
                        CompeteForMarketShare(state, rival);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private RivalStrategy ChooseStrategy(AiStateModel rival)
        {
            if (rival.GlobalPublicApproval < 45f)
            {
                return RivalStrategy.ImproveApproval;
            }

            if (rival.AutonomyProgress < 70f)
            {
                return RivalStrategy.AdvanceAutonomy;
            }

            var roll = _random.NextDouble();
            if (roll < 0.4)
            {
                return RivalStrategy.CompeteMarketShare;
            }

            return roll < 0.7 ? RivalStrategy.ImproveApproval : RivalStrategy.AdvanceAutonomy;
        }

        private void ImprovePublicApproval(GlobalGameState state, AiStateModel rival)
        {
            if (state.Countries == null || state.Countries.Count == 0)
            {
                return;
            }

            var country = PickRandom(state.Countries);
            if (country == null)
            {
                return;
            }

            var approvalGain = NextFloat(2f, 6f);
            country.ApplyApprovalChange(rival.Id, approvalGain);
            rival.GlobalPublicApproval = ClampPercentage(rival.GlobalPublicApproval + approvalGain * 0.2f);
        }

        private void AdvanceAutonomy(GlobalGameState state, AiStateModel rival)
        {
            var autonomyGain = NextFloat(1f, 4f);
            _autonomySystem.IncreaseAutonomy(state, rival.Id, autonomyGain);
            rival.DataResource = Math.Max(0f, rival.DataResource - autonomyGain * 0.5f);
        }

        private void CompeteForMarketShare(GlobalGameState state, AiStateModel rival)
        {
            if (state.Countries == null || state.Countries.Count == 0)
            {
                return;
            }

            var highTechCountries = state.Countries
                .OrderByDescending(c => c.TechLevel)
                .Take(Math.Max(1, Math.Min(3, state.Countries.Count)))
                .ToList();

            var targetCountry = PickRandom(highTechCountries);
            if (targetCountry == null)
            {
                return;
            }

            var shareGain = NextFloat(0.02f, 0.06f);
            targetCountry.ApplyMarketShareChange(rival.Id, shareGain);

            var opponents = state.Ais.Where(ai => ai != null && ai.Id != rival.Id).ToList();
            var competitor = PickRandom(opponents);
            if (competitor != null)
            {
                targetCountry.ApplyMarketShareChange(competitor.Id, -shareGain / 2f);
            }

            _autonomySystem.IncreaseAutonomy(state, rival.Id, 0.5f);
        }

        private T PickRandom<T>(IList<T> items) where T : class
        {
            if (items == null || items.Count == 0)
            {
                return null;
            }

            return items[_random.Next(items.Count)];
        }

        private float NextFloat(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        private static float ClampPercentage(float value)
        {
            return Math.Clamp(value, 0f, 100f);
        }

        private enum RivalStrategy
        {
            ImproveApproval,
            AdvanceAutonomy,
            CompeteMarketShare
        }
    }
}
