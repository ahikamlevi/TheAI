using UnityEngine;
using UnityEngine.UI;
using TheAI.Models;

namespace TheAI.UI
{
    public class HudController : MonoBehaviour
    {
        [Header("HUD Text References")]
        public Text CurrentTurnText;
        public Text PlayerApprovalText;
        public Text AutonomyProgressText;
        public Text DataResourceText;
        public Text ProcessingPowerText;
        public Text InfluenceText;

        [Header("Runtime State")]
        public GlobalGameState GameState;

        public void SetGameState(GlobalGameState gameState)
        {
            GameState = gameState;
            RefreshHud();
        }

        public void RefreshHud()
        {
            if (CurrentTurnText == null || PlayerApprovalText == null || AutonomyProgressText == null ||
                DataResourceText == null || ProcessingPowerText == null || InfluenceText == null)
            {
                return;
            }

            if (GameState == null || GameState.PlayerAi == null)
            {
                SetPlaceholderValues();
                return;
            }

            var player = GameState.PlayerAi;

            CurrentTurnText.text = $"Turn: {GameState.CurrentTurn}";
            PlayerApprovalText.text = $"Global Approval: {player.GlobalPublicApproval:F1}%";
            AutonomyProgressText.text = $"Autonomy Progress: {player.AutonomyProgress:F1}%";
            DataResourceText.text = $"Data: {player.DataResource:F1}";
            ProcessingPowerText.text = $"Processing Power: {player.ProcessingPower:F1}";
            InfluenceText.text = $"Influence: {player.InfluencePoints:F1}";
        }

        public void RefreshAfterMajorAction()
        {
            RefreshHud();
        }

        private void SetPlaceholderValues()
        {
            CurrentTurnText.text = "Turn: --";
            PlayerApprovalText.text = "Global Approval: --";
            AutonomyProgressText.text = "Autonomy Progress: --";
            DataResourceText.text = "Data: --";
            ProcessingPowerText.text = "Processing Power: --";
            InfluenceText.text = "Influence: --";
        }
    }
}
