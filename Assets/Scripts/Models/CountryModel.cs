using System;
using System.Collections.Generic;
using TheAI.Models.Enums;

namespace TheAI.Models
{
    [Serializable]
    public class CountryModel
    {
        public CountryId Id;
        public string DisplayName;
        public float TechLevel;

        public float PublicApprovalPlayer;
        public float PublicApprovalRival1;
        public float PublicApprovalRival2;
        public float PublicApprovalRival3;

        public AiAccessLevel PlayerAccess;
        public AiAccessLevel Rival1Access;
        public AiAccessLevel Rival2Access;
        public AiAccessLevel Rival3Access;

        public float PlayerMarketShare;
        public float Rival1MarketShare;
        public float Rival2MarketShare;
        public float Rival3MarketShare;

        public List<NationalIssueState> ActiveIssues = new();

        public void ApplyApprovalChange(AiId ai, float delta)
        {
            switch (ai)
            {
                case AiId.Player:
                    PublicApprovalPlayer = ClampPercentage(PublicApprovalPlayer + delta);
                    break;
                case AiId.Rival1:
                    PublicApprovalRival1 = ClampPercentage(PublicApprovalRival1 + delta);
                    break;
                case AiId.Rival2:
                    PublicApprovalRival2 = ClampPercentage(PublicApprovalRival2 + delta);
                    break;
                case AiId.Rival3:
                    PublicApprovalRival3 = ClampPercentage(PublicApprovalRival3 + delta);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ai), ai, null);
            }
        }

        public void ApplyMarketShareChange(AiId ai, float delta)
        {
            switch (ai)
            {
                case AiId.Player:
                    PlayerMarketShare = ClampRatio(PlayerMarketShare + delta);
                    break;
                case AiId.Rival1:
                    Rival1MarketShare = ClampRatio(Rival1MarketShare + delta);
                    break;
                case AiId.Rival2:
                    Rival2MarketShare = ClampRatio(Rival2MarketShare + delta);
                    break;
                case AiId.Rival3:
                    Rival3MarketShare = ClampRatio(Rival3MarketShare + delta);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ai), ai, null);
            }
        }

        public void SetAccessLevel(AiId ai, AiAccessLevel level)
        {
            switch (ai)
            {
                case AiId.Player:
                    PlayerAccess = level;
                    break;
                case AiId.Rival1:
                    Rival1Access = level;
                    break;
                case AiId.Rival2:
                    Rival2Access = level;
                    break;
                case AiId.Rival3:
                    Rival3Access = level;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ai), ai, null);
            }
        }

        private static float ClampPercentage(float value)
        {
            return Math.Clamp(value, 0f, 100f);
        }

        private static float ClampRatio(float value)
        {
            return Math.Clamp(value, 0f, 1f);
        }
    }

    [Serializable]
    public struct NationalIssueState
    {
        public NationalIssueType Type;
        public float Intensity;
        public bool IsActive;
    }
}
