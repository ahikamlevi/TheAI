using System.Collections.Generic;
using UnityEngine;

namespace TheAI.Data
{
    [CreateAssetMenu(fileName = "AiConfigsDatabase", menuName = "TheAI/Data/Ai Configs Database", order = 3)]
    public class AiConfigsDatabase : ScriptableObject
    {
        public List<AiConfig> Ais = new();
    }
}
