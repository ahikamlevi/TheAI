using System;
using System.Collections.Generic;
using TheAI.Core;
using TheAI.Models;
using TheAI.Models.Enums;
using TheAI.Systems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheAI.UI
{
    public class PlayerActionPanel : MonoBehaviour
    {
        [Header("References")]
        public TurnManager TurnManager;
        public HudController Hud;
        public CountryListController CountryList;

        [Header("UI Elements")]
        public Button HelpNationButton;
        public Button ManipulateAgentButton;
        public Button ResearchKnowledgeButton;
        public Button EndTurnButton;
        public Dropdown IssueDropdown;
        public Slider StrengthSlider;
        public Text StrengthValueLabel;

        [Header("Action Settings")]
        public KnowledgeItem DefaultKnowledgeItem;
        public float DefaultStrength = 10f;

        private readonly PlayerActionSystem _playerActionSystem = new();

        private CountryId? _selectedCountry;
        private readonly List<NationalIssueType> _issueOptions = new();

        private GlobalGameState CurrentState => TurnManager != null ? TurnManager.GameState : Hud?.GameState;

        private void Awake()
        {
            PopulateIssueDropdown();
            RegisterUiCallbacks();
            SyncInitialSelection();
            UpdateInteractableState();
            UpdateStrengthLabel();
        }

        private void OnDestroy()
        {
            UnregisterUiCallbacks();
        }

        private void PopulateIssueDropdown()
        {
            if (IssueDropdown == null)
            {
                return;
            }

            _issueOptions.Clear();
            var options = new List<string>();
            foreach (var value in Enum.GetValues(typeof(NationalIssueType)))
            {
                if (value is NationalIssueType issue)
                {
                    _issueOptions.Add(issue);
                    options.Add(issue.ToString());
                }
            }

            IssueDropdown.ClearOptions();
            IssueDropdown.AddOptions(options);
        }

        private void RegisterUiCallbacks()
        {
            AttachButtonListener(HelpNationButton, OnHelpNationClicked);
            AttachButtonListener(ManipulateAgentButton, OnManipulateAgentClicked);
            AttachButtonListener(ResearchKnowledgeButton, OnResearchKnowledgeClicked);
            AttachButtonListener(EndTurnButton, OnEndTurnClicked);

            if (StrengthSlider != null)
            {
                StrengthSlider.onValueChanged.AddListener(OnStrengthChanged);
            }

            if (CountryList != null)
            {
                CountryList.OnCountrySelected ??= new UnityEvent<CountryId>();
                CountryList.OnCountrySelected.AddListener(HandleCountrySelected);
            }
        }

        private void UnregisterUiCallbacks()
        {
            if (StrengthSlider != null)
            {
                StrengthSlider.onValueChanged.RemoveListener(OnStrengthChanged);
            }

            if (CountryList != null && CountryList.OnCountrySelected != null)
            {
                CountryList.OnCountrySelected.RemoveListener(HandleCountrySelected);
            }
        }

        private static void AttachButtonListener(Button button, UnityAction callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (callback != null)
            {
                button.onClick.AddListener(callback);
            }
        }

        private void SyncInitialSelection()
        {
            if (CountryList != null)
            {
                _selectedCountry = CountryList.SelectedCountryId;
            }

            if (StrengthSlider != null)
            {
                StrengthSlider.value = Math.Abs(StrengthSlider.value) < float.Epsilon ? DefaultStrength : StrengthSlider.value;
            }
        }

        private void HandleCountrySelected(CountryId countryId)
        {
            _selectedCountry = countryId;
            UpdateInteractableState();
        }

        private void OnStrengthChanged(float _)
        {
            UpdateStrengthLabel();
        }

        private void OnHelpNationClicked()
        {
            if (!TryGetSelectedCountry(out var countryId))
            {
                return;
            }

            var action = new PlayerAction
            {
                ActionType = PlayerActionType.HelpNation,
                TargetCountry = countryId,
                IssueType = GetSelectedIssueType(),
                Strength = GetSelectedStrength()
            };

            ExecuteAction(action);
        }

        private void OnManipulateAgentClicked()
        {
            if (!TryGetSelectedCountry(out var countryId))
            {
                return;
            }

            var strength = GetSelectedStrength();
            var action = new PlayerAction
            {
                ActionType = PlayerActionType.ManipulateAgent,
                TargetCountry = countryId,
                AutonomyGain = strength,
                ApprovalPenalty = -Mathf.Abs(strength)
            };

            ExecuteAction(action);
        }

        private void OnResearchKnowledgeClicked()
        {
            var researchItem = DefaultKnowledgeItem ?? new KnowledgeItem
            {
                Id = "DefaultResearch",
                Type = KnowledgeType.AiBenefit,
                DataCost = 0f,
                HumanBenefitValue = 0f,
                AiAutonomyValue = GetSelectedStrength(),
                ApprovalEffect = 0f,
                SuspicionEffect = 0f
            };

            var action = new PlayerAction
            {
                ActionType = PlayerActionType.KnowledgeResearch,
                SelectedItem = researchItem
            };

            ExecuteAction(action);
        }

        private void OnEndTurnClicked()
        {
            TurnManager?.OnEndTurnButtonClicked();
            RefreshUi();
        }

        private void ExecuteAction(PlayerAction action)
        {
            var state = CurrentState;
            if (state == null)
            {
                Debug.LogWarning("Cannot execute action because the game state is missing.");
                return;
            }

            _playerActionSystem.ExecutePlayerAction(state, action);
            RefreshUi();
        }

        private void RefreshUi()
        {
            Hud?.RefreshAfterMajorAction();
            CountryList?.RefreshList();
        }

        private bool TryGetSelectedCountry(out CountryId countryId)
        {
            if (_selectedCountry.HasValue)
            {
                countryId = _selectedCountry.Value;
                return true;
            }

            Debug.LogWarning("Select a country before performing this action.");
            countryId = default;
            return false;
        }

        private NationalIssueType GetSelectedIssueType()
        {
            if (IssueDropdown == null || _issueOptions.Count == 0)
            {
                return NationalIssueType.Economy;
            }

            var index = Mathf.Clamp(IssueDropdown.value, 0, _issueOptions.Count - 1);
            return _issueOptions[index];
        }

        private float GetSelectedStrength()
        {
            return StrengthSlider != null ? StrengthSlider.value : DefaultStrength;
        }

        private void UpdateStrengthLabel()
        {
            if (StrengthValueLabel != null)
            {
                StrengthValueLabel.text = $"Strength: {GetSelectedStrength():F1}";
            }
        }

        private void UpdateInteractableState()
        {
            var hasSelection = _selectedCountry.HasValue;

            if (HelpNationButton != null)
            {
                HelpNationButton.interactable = hasSelection;
            }

            if (ManipulateAgentButton != null)
            {
                ManipulateAgentButton.interactable = hasSelection;
            }
        }
    }
}
