using System.Collections.Generic;
using System.IO;
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
        public CountryListController CountryList;
        public PlayerActionPanel PlayerActions;

        private readonly PlayerActionSystem _playerActionSystem = new();
        private readonly RivalAiSystem _rivalAiSystem = new();
        private readonly NationalIssueSystem _nationalIssueSystem = new();
        private readonly EventSystem _eventSystem = new();
        private readonly ApprovalSystem _approvalSystem = new();
        private readonly WinLoseSystem _winLoseSystem = new();
        private readonly SaveLoadService _saveLoadService = new();
        private readonly List<PlayerActionEntry> _queuedPlayerActions = new();

        [Header("Runtime State")]
        public GlobalGameState GameState;

        [Header("Turn Settings")]
        public int TurnDurationMs = 1000;

        [Header("Save/Load Settings")]
        public string SaveFileName = "savegame.json";

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

            if (CountryList != null)
            {
                CountryList.SetGameState(GameState);
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
                _queuedPlayerActions.Clear();
            }
        }

        public void OnSaveGameButtonClicked()
        {
            var savePath = GetSaveFilePath();
            _saveLoadService.SaveGame(GameState, savePath);
        }

        public void OnLoadGameButtonClicked()
        {
            var savePath = GetSaveFilePath();
            var loadedState = _saveLoadService.LoadGame(savePath);
            if (loadedState == null)
            {
                return;
            }

            GameState = loadedState;
            Hud?.SetGameState(GameState);
            CountryList?.SetGameState(GameState);
            PlayerActions?.RefreshAfterGameStateChanged();
            _queuedPlayerActions.Clear();
        }

        public void EndPlayerTurn()
        {
            if (GameState == null)
            {
                Debug.LogWarning("Cannot end turn because the game state has not been initialized.");
                return;
            }

            if (GameState.IsGameOver)
            {
                Debug.LogWarning("Game is already over. No further turns can be played.");
                return;
            }

            ProcessPlayerActions();
            ProcessRivalAiActions();
            UpdateNationalIssues();
            ProcessEvents();
            RecalculateGlobalApproval();
            EvaluateWinLossConditions();

            GameState.AdvanceTurn();

            Hud?.RefreshHud();
            CountryList?.RefreshList();
        }

        public void OnEndTurnButtonClicked()
        {
            EndPlayerTurn();
        }

        public void RefreshHudAfterMajorAction()
        {
            Hud?.RefreshHud();
        }

        public void QueuePlayerAction(PlayerAction action, bool executeImmediately = true)
        {
            if (action == null)
            {
                return;
            }

            if (GameState == null)
            {
                Debug.LogWarning("Cannot queue action because the game state has not been initialized.");
                return;
            }

            _queuedPlayerActions.Add(new PlayerActionEntry
            {
                Action = action,
                Executed = executeImmediately
            });

            if (executeImmediately)
            {
                _playerActionSystem.ExecutePlayerAction(GameState, action);
            }
        }

        private void ProcessPlayerActions()
        {
            if (_queuedPlayerActions.Count == 0)
            {
                return;
            }

            foreach (var entry in _queuedPlayerActions)
            {
                if (entry.Executed)
                {
                    continue;
                }

                _playerActionSystem.ExecutePlayerAction(GameState, entry.Action);
            }

            _queuedPlayerActions.Clear();
        }

        private void ProcessRivalAiActions()
        {
            _rivalAiSystem.ProcessRivalsTurn(GameState);
        }

        private void UpdateNationalIssues()
        {
            _nationalIssueSystem.UpdateIssuesPerTurn(GameState);
        }

        private void ProcessEvents()
        {
            var events = _eventSystem.GenerateTurnEvents(GameState);
            foreach (var gameEvent in events)
            {
                _eventSystem.ApplyEvent(GameState, gameEvent);
            }
        }

        private void RecalculateGlobalApproval()
        {
            if (GameState?.Ais == null)
            {
                return;
            }

            foreach (var ai in GameState.Ais)
            {
                if (ai == null)
                {
                    continue;
                }

                _approvalSystem.RecalculateGlobalApproval(GameState, ai.Id);
            }
        }

        private void EvaluateWinLossConditions()
        {
            _winLoseSystem.EvaluateAndSetGameOver(GameState);
        }

        private string GetSaveFilePath()
        {
            var fileName = string.IsNullOrWhiteSpace(SaveFileName) ? "savegame.json" : SaveFileName;
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        private struct PlayerActionEntry
        {
            public PlayerAction Action;
            public bool Executed;
        }
    }
}
