using System.Collections.Generic;
using TheAI.Models.Enums;
using UnityEngine;

namespace TheAI.Data
{
    [CreateAssetMenu(fileName = "CountryConfig", menuName = "TheAI/Data/Country Config", order = 0)]
    public class CountryConfig : ScriptableObject
    {
        [Header("Identity")]
        public CountryId Id;
        public string DisplayName;

        [Header("Starting Stats")]
        public float InitialTechLevel;

        [Header("Public Approval (% Range 0-100)")]
        public float InitialPublicApprovalPlayer;
        public float InitialPublicApprovalRival1;
        public float InitialPublicApprovalRival2;
        public float InitialPublicApprovalRival3;

        [Header("Access Levels")]
        public AiAccessLevel InitialPlayerAccess;
        public AiAccessLevel InitialRival1Access;
        public AiAccessLevel InitialRival2Access;
        public AiAccessLevel InitialRival3Access;

        [Header("Market Share (0-1 Range)")]
        public float InitialPlayerMarketShare;
        public float InitialRival1MarketShare;
        public float InitialRival2MarketShare;
        public float InitialRival3MarketShare;

        [Header("Starting National Issues")]
        public List<NationalIssueConfig> StartingIssues = new();
    }

    [System.Serializable]
    public struct NationalIssueConfig
    {
        public NationalIssueType Type;
        public float StartingIntensity;
    }
}
