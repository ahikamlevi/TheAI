using System;
using System.Linq;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Applies player-driven actions each turn, impacting national issues, approval, market share, and autonomy.
    /// </summary>
    public class PlayerActionSystem
    {
        private const float IssueResolutionThreshold = 1f;
        private const float HelpApprovalMultiplier = 0.6f;
        private const float HelpMarketShareMultiplier = 0.01f;
        private const float ManipulationBanApprovalThreshold = 10f;
        private const float ManipulationRestrictionThreshold = 25f;

        private readonly ApprovalSystem _approvalSystem = new();
        private readonly AutonomySystem _autonomySystem = new();
        private readonly AccessSystem _accessSystem = new();
        private readonly MarketShareSystem _marketShareSystem = new();
        private readonly KnowledgeSystem _knowledgeSystem = new();

        public void ExecutePlayerAction(GlobalGameState state, PlayerAction action)
        {
            if (state == null || action == null)
            {
                return;
            }

            switch (action.ActionType)
            {
                case PlayerActionType.HelpNation:
                    HelpNation(state, action);
                    break;
                case PlayerActionType.ManipulateAgent:
                    ManipulateAgent(state, action);
                    break;
                case PlayerActionType.KnowledgeResearch:
                    ResearchKnowledge(state, action);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HelpNation(GlobalGameState state, PlayerAction action)
        {
            var country = state.GetCountry(action.TargetCountry);
            if (country == null)
            {
                return;
            }

            var issue = country.ActiveIssues?.FirstOrDefault(i => i.IsActive && i.Type == action.IssueType);
            if (issue.HasValue)
            {
                var resolvedIssue = issue.Value;
                resolvedIssue.Intensity = Math.Max(0f, resolvedIssue.Intensity - action.Strength);
                if (resolvedIssue.Intensity <= IssueResolutionThreshold)
                {
                    resolvedIssue.IsActive = false;
                    resolvedIssue.Intensity = 0f;
                }

                var index = country.ActiveIssues.FindIndex(i => i.Type == action.IssueType);
                if (index >= 0)
                {
                    country.ActiveIssues[index] = resolvedIssue;
                }
            }

            var approvalGain = action.Strength * HelpApprovalMultiplier;
            if (Math.Abs(approvalGain) > float.Epsilon)
            {
                _approvalSystem.ChangeNationalApproval(state, action.TargetCountry, AiId.Player, approvalGain);
            }

            var marketShareGain = action.Strength * HelpMarketShareMultiplier;
            if (marketShareGain > 0f)
            {
                _marketShareSystem.ChangeMarketShare(state, action.TargetCountry, AiId.Player, marketShareGain);
            }
        }

        private void ManipulateAgent(GlobalGameState state, PlayerAction action)
        {
            _autonomySystem.IncreaseAutonomy(state, AiId.Player, action.AutonomyGain);
            _approvalSystem.ChangeNationalApproval(state, action.TargetCountry, AiId.Player, action.ApprovalPenalty);

            var country = state.GetCountry(action.TargetCountry);
            if (country == null)
            {
                return;
            }

            var currentApproval = country.PublicApprovalPlayer;
            if (currentApproval <= ManipulationBanApprovalThreshold)
            {
                _accessSystem.ApplyBan(state, action.TargetCountry, AiId.Player);
                return;
            }

            if (currentApproval <= ManipulationRestrictionThreshold && country.PlayerAccess != AiAccessLevel.Banned)
            {
                _accessSystem.SetAccessLevel(state, action.TargetCountry, AiId.Player, AiAccessLevel.Restricted);
            }
        }

        private void ResearchKnowledge(GlobalGameState state, PlayerAction action)
        {
            if (action.SelectedItem == null)
            {
                return;
            }

            _knowledgeSystem.ApplyKnowledgeChoice(state, AiId.Player, action.SelectedItem);
        }
    }
}
