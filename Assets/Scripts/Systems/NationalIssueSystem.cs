using System;
using System.Collections.Generic;
using System.Linq;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Manages the lifecycle of national issues across countries.
    /// </summary>
    public class NationalIssueSystem
    {
        private const float IssueResolutionThreshold = 1f;
        private const float IntensityDriftPerTurn = 1.5f;
        private const float NewIssueBaseChance = 0.05f;
        private const float TechLevelIssueChanceMultiplier = 0.0025f;
        private const float HelpApprovalMultiplier = 0.6f;
        private const float HelpTrustMultiplier = 0.4f;

        private readonly ApprovalSystem _approvalSystem = new();
        private readonly Random _random = new();

        /// <summary>
        /// Updates active issues and potentially introduces new ones each turn.
        /// </summary>
        public void UpdateIssuesPerTurn(GlobalGameState state)
        {
            if (state?.Countries == null)
            {
                return;
            }

            foreach (var country in state.Countries)
            {
                if (country == null)
                {
                    continue;
                }

                DriftExistingIssues(country);
                TryCreateNewIssue(country);
            }
        }

        public void ApplyHelpEffect(GlobalGameState state, CountryId countryId, AiId ai, NationalIssueType issueType, float effort)
        {
            var country = state?.GetCountry(countryId);
            if (country == null || country.ActiveIssues == null)
            {
                return;
            }

            var issueIndex = country.ActiveIssues.FindIndex(i => i.Type == issueType);
            if (issueIndex < 0)
            {
                return;
            }

            var issue = country.ActiveIssues[issueIndex];
            if (!issue.IsActive)
            {
                return;
            }

            issue.Intensity = Math.Max(0f, issue.Intensity - effort);
            if (issue.Intensity <= IssueResolutionThreshold)
            {
                issue.IsActive = false;
                issue.Intensity = 0f;
            }

            country.ActiveIssues[issueIndex] = issue;

            var approvalGain = effort * HelpApprovalMultiplier;
            if (Math.Abs(approvalGain) > float.Epsilon)
            {
                _approvalSystem.ChangeNationalApproval(state, countryId, ai, approvalGain);
            }

            var aiState = state?.GetAi(ai);
            if (aiState != null)
            {
                var trustGain = effort * HelpTrustMultiplier;
                aiState.PublicTrust = Math.Clamp(aiState.PublicTrust + trustGain, 0f, 100f);
            }
        }

        private void DriftExistingIssues(CountryModel country)
        {
            if (country.ActiveIssues == null || country.ActiveIssues.Count == 0)
            {
                return;
            }

            for (var i = 0; i < country.ActiveIssues.Count; i++)
            {
                var issue = country.ActiveIssues[i];
                if (!issue.IsActive)
                {
                    continue;
                }

                var drift = NextFloat(-IntensityDriftPerTurn, IntensityDriftPerTurn);
                issue.Intensity = ClampIntensity(issue.Intensity + drift);

                if (issue.Intensity <= IssueResolutionThreshold)
                {
                    issue.IsActive = false;
                    issue.Intensity = 0f;
                }

                country.ActiveIssues[i] = issue;
            }
        }

        private void TryCreateNewIssue(CountryModel country)
        {
            var possibleIssues = Enum.GetValues<NationalIssueType>().ToList();
            var existingTypes = country.ActiveIssues?.Select(i => i.Type).ToHashSet() ?? new HashSet<NationalIssueType>();
            var availableIssues = possibleIssues.Where(type => !existingTypes.Contains(type)).ToList();

            if (availableIssues.Count == 0)
            {
                return;
            }

            var spawnChance = Math.Clamp(NewIssueBaseChance + country.TechLevel * TechLevelIssueChanceMultiplier, 0f, 1f);
            if (_random.NextDouble() > spawnChance)
            {
                return;
            }

            var newIssueType = availableIssues[_random.Next(availableIssues.Count)];
            var startingIntensity = NextFloat(5f, 20f);

            country.ActiveIssues ??= new List<NationalIssueState>();
            country.ActiveIssues.Add(new NationalIssueState
            {
                Type = newIssueType,
                Intensity = startingIntensity,
                IsActive = true
            });
        }

        private float NextFloat(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        private static float ClampIntensity(float value)
        {
            return Math.Clamp(value, 0f, 100f);
        }
    }
}
