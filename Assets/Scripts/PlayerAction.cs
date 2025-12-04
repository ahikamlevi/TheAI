using System;
using TheAI.Models.Enums;

namespace TheAI.Models
{
    [Serializable]
    public enum PlayerActionType
    {
        HelpNation,
        ManipulateAgent,
        KnowledgeResearch
    }

    [Serializable]
    public class PlayerAction
    {
        public PlayerActionType ActionType;

        // HelpNation fields
        public CountryId TargetCountry;
        public NationalIssueType IssueType;
        public float Strength;

        // ManipulateAgent fields
        public float AutonomyGain;
        public float ApprovalPenalty;

        // KnowledgeResearch fields
        public KnowledgeItem SelectedItem;
    }
}
