using System;
using TheAI.Models.Enums;

namespace TheAI.Models
{
    [Serializable]
    public class GameEvent
    {
        public string Id;
        public EventSeverity Severity;
        public CountryId? AffectedCountry;
        public NationalIssueType? RelatedIssue;
        public bool IsGlobal;
        public AiId? TargetAi;

        public float ApprovalDelta;
        public float MarketShareDelta;
        public float IssueIntensityDelta;
        public float TrustDelta;
        public float AutonomyDelta;
        public float DataDelta;
        public float ProcessingDelta;
        public float InfluenceDelta;
    }
}
