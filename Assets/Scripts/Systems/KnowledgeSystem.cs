using System;
using System.Linq;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Applies knowledge choices selected by the player or rivals, updating resources and reputation.
    /// </summary>
    public class KnowledgeSystem
    {
        private readonly AutonomySystem _autonomySystem = new();
        private readonly ApprovalSystem _approvalSystem = new();

        public void ApplyKnowledgeChoice(GlobalGameState state, AiId ai, KnowledgeItem item)
        {
            if (state == null || item == null)
            {
                return;
            }

            var aiState = state.GetAi(ai);
            if (aiState == null)
            {
                return;
            }

            aiState.SpendData(item.DataCost);

            _autonomySystem.IncreaseAutonomy(state, ai, item.AiAutonomyValue);

            ApplyApprovalChanges(state, ai, item);
            AdjustTrust(aiState, item);
        }

        private void ApplyApprovalChanges(GlobalGameState state, AiId ai, KnowledgeItem item)
        {
            if (state.Countries == null || state.Countries.Count == 0)
            {
                return;
            }

            var approvalDelta = item.ApprovalEffect;
            if (item.Type == KnowledgeType.HumanBenefit)
            {
                approvalDelta += item.HumanBenefitValue;
            }

            if (Math.Abs(approvalDelta) < float.Epsilon)
            {
                return;
            }

            foreach (var country in state.Countries)
            {
                if (country == null)
                {
                    continue;
                }

                if (item.RelatedIssue.HasValue)
                {
                    var matchesIssue = country.ActiveIssues?.Any(issue => issue.IsActive && issue.Type == item.RelatedIssue.Value) == true;
                    if (!matchesIssue)
                    {
                        continue;
                    }
                }

                country.ApplyApprovalChange(ai, approvalDelta);
            }

            _approvalSystem.RecalculateGlobalApproval(state, ai);
        }

        private static void AdjustTrust(AiStateModel aiState, KnowledgeItem item)
        {
            var trustDelta = item.HumanBenefitValue - item.SuspicionEffect;
            aiState.PublicTrust = Math.Clamp(aiState.PublicTrust + trustDelta, 0f, 100f);
        }
    }
}
