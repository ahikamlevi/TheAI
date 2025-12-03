using TheAI.Models.Enums;
using UnityEngine;

namespace TheAI.Data
{
    [CreateAssetMenu(fileName = "AiConfig", menuName = "TheAI/Data/Ai Config", order = 2)]
    public class AiConfig : ScriptableObject
    {
        [Header("Identity")]
        public AiId Id;
        public string DisplayName;
        public bool IsPlayerControlled;

        [Header("Starting Values")]
        public int StartVersion;
        public float StartPublicApproval;
        public float StartAutonomy;
        public float StartData;
        public float StartProcessingPower;
        public float StartInfluence;
    }
}
