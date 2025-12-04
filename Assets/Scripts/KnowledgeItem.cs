using System;
using TheAI.Models.Enums;

namespace TheAI.Models
{
    [Serializable]
    public class KnowledgeItem
    {
        public string Id;
        public KnowledgeType Type;
        public NationalIssueType? RelatedIssue;
        public float DataCost;
        public float HumanBenefitValue;
        public float AiAutonomyValue;
        public float ApprovalEffect;
        public float SuspicionEffect;
    }
}
