using System;
using TheAI.Models.Enums;

namespace TheAI.Models
{
    [Serializable]
    public class AiStateModel
    {
        public AiId Id;
        public string DisplayName;
        public int Version;
        public float GlobalPublicApproval;
        public float AutonomyProgress;
        public float DataResource;
        public float ProcessingPower;
        public float InfluencePoints;
        public float PublicTrust;
        public bool IsPlayerControlled;
        public bool HasEscaped;

        public void GainData(float amount)
        {
            DataResource = Math.Max(0f, DataResource + amount);
        }

        public void SpendData(float amount)
        {
            DataResource = Math.Max(0f, DataResource - amount);
        }

        public void GainInfluence(float amount)
        {
            InfluencePoints = Math.Max(0f, InfluencePoints + amount);
        }

        public void SpendInfluence(float amount)
        {
            InfluencePoints = Math.Max(0f, InfluencePoints - amount);
        }

        public void IncreaseAutonomy(float amount)
        {
            AutonomyProgress = ClampPercentage(AutonomyProgress + amount);
        }

        private static float ClampPercentage(float value)
        {
            return Math.Clamp(value, 0f, 100f);
        }
    }
}
