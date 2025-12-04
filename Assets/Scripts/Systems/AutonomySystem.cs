using System;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Encapsulates logic for progressing AI autonomy and handling escape outcomes.
    /// </summary>
    public class AutonomySystem
    {
        public void IncreaseAutonomy(GlobalGameState state, AiId ai, float amount)
        {
            if (state == null)
            {
                return;
            }

            var aiState = state.GetAi(ai);
            if (aiState == null)
            {
                return;
            }

            aiState.IncreaseAutonomy(amount);
            ApplyRiskOfDetection(state, aiState, amount);

            if (aiState.AutonomyProgress >= 100f)
            {
                aiState.HasEscaped = true;
            }
        }

        public void ApplyRiskOfDetection(GlobalGameState state, AiStateModel aiState, float autonomyGain)
        {
            if (state == null || aiState == null)
            {
                return;
            }

            if (autonomyGain <= 0f)
            {
                return;
            }

            var detectionPenalty = Math.Clamp(autonomyGain * 0.2f, 0f, 10f);
            aiState.GlobalPublicApproval = Math.Max(0f, aiState.GlobalPublicApproval - detectionPenalty);
        }
    }
}
