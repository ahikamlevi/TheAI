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
        private const float HelpMarketShareMultiplier = 0.01f;
        private const float ManipulationBanApprovalThreshold = 10f;
        private const float ManipulationRestrictionThreshold = 25f;

        private readonly ApprovalSystem _approvalSystem = new();
        private readonly AutonomySystem _autonomySystem = new();
        private readonly AccessSystem _accessSystem = new();
        private readonly MarketShareSystem _marketShareSystem = new();
        private readonly KnowledgeSystem _knowledgeSystem = new();
        private readonly NationalIssueSystem _nationalIssueSystem = new();

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

            _nationalIssueSystem.ApplyHelpEffect(state, action.TargetCountry, AiId.Player, action.IssueType, action.Strength);

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
