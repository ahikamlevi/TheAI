using UnityEngine;
using TheAI.Models;
using TheAI.Models.Enums;
using TheAI.Systems;
using TheAI.UI;

namespace TheAI.Core
{
    public class TurnManager : MonoBehaviour
    {
        [Header("References")]
        public GameStateInitializer GameStateInitializer;
        public HudController Hud;

        private readonly RivalAiSystem _rivalAiSystem = new();
        private readonly WinLoseSystem _winLoseSystem = new();

        [Header("Runtime State")]
        public GlobalGameState GameState;

        [Header("Turn Settings")]
        public int TurnDurationMs = 1000;

        public void StartNewGame()
        {
            if (GameStateInitializer == null)
            {
                Debug.LogError("GameStateInitializer reference is missing.");
                GameState = null;
                return;
            }

            GameState = GameStateInitializer.CreateInitialGameState();

            if (Hud != null)
            {
                Hud.SetGameState(GameState);
            }

            if (GameState != null)
            {
                GameState.CurrentTurn = 0;
                GameState.IsGameOver = false;
                GameState.GameResult = new GameOverResult
                {
                    Outcome = GameOutcome.None,
                    Reason = GameEndReason.None
                };

                Hud?.RefreshHud();
            }
        }

        public void EndPlayerTurn()
        {
            if (GameState == null)
            {
                Debug.LogWarning("Cannot end turn because the game state has not been initialized.");
                return;
            }

            ProcessPlayerActions();
            ProcessRivalAiActions();
            ProcessCountryReactions();
            ProcessEvents();

            GameState.AdvanceTurn();
            EvaluateWinLossConditions();

            Hud?.RefreshHud();
        }

        public void OnEndTurnButtonClicked()
        {
            EndPlayerTurn();
        }

        public void RefreshHudAfterMajorAction()
        {
            Hud?.RefreshHud();
        }

        private void ProcessPlayerActions()
        {
            // Placeholder for processing player actions queued during the current turn.
        }

        private void ProcessRivalAiActions()
        {
            _rivalAiSystem.ProcessRivalsTurn(GameState);
        }

        private void ProcessCountryReactions()
        {
            // Placeholder for applying country reactions to AI actions.
        }

        private void ProcessEvents()
        {
            // Placeholder for resolving issues and events triggered this turn.
        }

        private void EvaluateWinLossConditions()
        {
            _winLoseSystem.EvaluateAndSetGameOver(GameState);
        }
    }
}
