using System;
using System.Collections.Generic;
using System.Linq;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Generates and applies world events that can affect AI resources, public sentiment, and national issues.
    /// </summary>
    public class EventSystem
    {
        private const double GlobalEventChance = 0.12;
        private const double CountryEventChance = 0.28;

        private readonly ApprovalSystem _approvalSystem = new();
        private readonly MarketShareSystem _marketShareSystem = new();
        private readonly Random _random = new();

        public List<GameEvent> GenerateTurnEvents(GlobalGameState state)
        {
            var events = new List<GameEvent>();
            if (!HasValidState(state))
            {
                return events;
            }

            if (_random.NextDouble() < GlobalEventChance)
            {
                events.Add(CreateGlobalEvent(state));
            }

            if (_random.NextDouble() < CountryEventChance)
            {
                events.Add(CreateCountryEvent(state));
            }

            return events;
        }

        public void ApplyEvent(GlobalGameState state, GameEvent gameEvent)
        {
            if (!HasValidState(state) || gameEvent == null)
            {
                return;
            }

            var affectedAis = ResolveAffectedAis(state, gameEvent);
            if (affectedAis.Count == 0)
            {
                return;
            }

            foreach (var aiState in affectedAis)
            {
                ApplyAiEffects(aiState, gameEvent);

                if (gameEvent.AffectedCountry.HasValue && Math.Abs(gameEvent.ApprovalDelta) > float.Epsilon)
                {
                    _approvalSystem.ChangeNationalApproval(state, gameEvent.AffectedCountry.Value, aiState.Id, gameEvent.ApprovalDelta);
                }

                if (gameEvent.AffectedCountry.HasValue && Math.Abs(gameEvent.MarketShareDelta) > float.Epsilon)
                {
                    _marketShareSystem.ChangeMarketShare(state, gameEvent.AffectedCountry.Value, aiState.Id, gameEvent.MarketShareDelta);
                }
            }

            if (gameEvent.AffectedCountry.HasValue && gameEvent.RelatedIssue.HasValue && Math.Abs(gameEvent.IssueIntensityDelta) > float.Epsilon)
            {
                ApplyIssueEffect(state, gameEvent);
            }
        }

        private bool HasValidState(GlobalGameState state)
        {
            return state?.Countries != null && state.Countries.Count > 0 && state.Ais != null && state.Ais.Count > 0;
        }

        private GameEvent CreateGlobalEvent(GlobalGameState state)
        {
            var severity = GetRandomSeverity();
            var direction = GetRandomDirection();
            var effectType = _random.Next(3);
            var magnitude = GetSeverityMultiplier(severity);

            var targetAi = _random.NextDouble() < 0.4
                ? state.PlayerAi?.Id
                : GetRandomAi(state)?.Id;

            var gameEvent = new GameEvent
            {
                Id = $"EVT-GLOBAL-{state.CurrentTurn}-{Guid.NewGuid().ToString()[..6]}",
                Severity = severity,
                IsGlobal = true,
                TargetAi = targetAi
            };

            switch (effectType)
            {
                case 0:
                    gameEvent.TrustDelta = direction * 4f * magnitude;
                    break;
                case 1:
                    gameEvent.AutonomyDelta = direction * 2f * magnitude;
                    gameEvent.ProcessingDelta = direction * 3f * magnitude;
                    break;
                default:
                    gameEvent.DataDelta = direction * 6f * magnitude;
                    gameEvent.InfluenceDelta = direction * 2f * magnitude;
                    break;
            }

            return gameEvent;
        }

        private GameEvent CreateCountryEvent(GlobalGameState state)
        {
            var country = GetRandomCountry(state);
            var severity = GetRandomSeverity();
            var direction = GetRandomDirection();
            var effectType = _random.Next(3);
            var magnitude = GetSeverityMultiplier(severity);

            var gameEvent = new GameEvent
            {
                Id = $"EVT-COUNTRY-{country.Id}-{state.CurrentTurn}-{Guid.NewGuid().ToString()[..6]}",
                Severity = severity,
                AffectedCountry = country.Id,
                IsGlobal = false,
                TargetAi = _random.NextDouble() < 0.5 ? GetRandomAi(state)?.Id : null
            };

            switch (effectType)
            {
                case 0:
                    gameEvent.ApprovalDelta = direction * 3f * magnitude;
                    break;
                case 1:
                    gameEvent.MarketShareDelta = direction * 0.03f * magnitude;
                    break;
                default:
                    gameEvent.RelatedIssue = GetRandomIssueType(country);
                    gameEvent.IssueIntensityDelta = direction * 6f * magnitude;
                    break;
            }

            return gameEvent;
        }

        private void ApplyAiEffects(AiStateModel aiState, GameEvent gameEvent)
        {
            if (aiState == null || gameEvent == null)
            {
                return;
            }

            aiState.PublicTrust = Math.Clamp(aiState.PublicTrust + gameEvent.TrustDelta, 0f, 100f);
            aiState.AutonomyProgress = Math.Clamp(aiState.AutonomyProgress + gameEvent.AutonomyDelta, 0f, 100f);
            aiState.ProcessingPower = Math.Max(0f, aiState.ProcessingPower + gameEvent.ProcessingDelta);

            if (Math.Abs(gameEvent.DataDelta) > float.Epsilon)
            {
                if (gameEvent.DataDelta >= 0f)
                {
                    aiState.GainData(gameEvent.DataDelta);
                }
                else
                {
                    aiState.SpendData(Math.Abs(gameEvent.DataDelta));
                }
            }

            if (Math.Abs(gameEvent.InfluenceDelta) > float.Epsilon)
            {
                if (gameEvent.InfluenceDelta >= 0f)
                {
                    aiState.GainInfluence(gameEvent.InfluenceDelta);
                }
                else
                {
                    aiState.SpendInfluence(Math.Abs(gameEvent.InfluenceDelta));
                }
            }
        }

        private void ApplyIssueEffect(GlobalGameState state, GameEvent gameEvent)
        {
            var country = state.GetCountry(gameEvent.AffectedCountry!.Value);
            if (country == null)
            {
                return;
            }

            country.ActiveIssues ??= new List<NationalIssueState>();
            var issueIndex = country.ActiveIssues.FindIndex(issue => issue.Type == gameEvent.RelatedIssue);
            if (issueIndex < 0)
            {
                var startingIntensity = ClampIntensity(Math.Max(0f, gameEvent.IssueIntensityDelta));
                country.ActiveIssues.Add(new NationalIssueState
                {
                    Type = gameEvent.RelatedIssue!.Value,
                    Intensity = startingIntensity,
                    IsActive = startingIntensity > 0f
                });
                return;
            }

            var existingIssue = country.ActiveIssues[issueIndex];
            existingIssue.Intensity = ClampIntensity(existingIssue.Intensity + gameEvent.IssueIntensityDelta);
            existingIssue.IsActive = existingIssue.Intensity > 0.01f;
            country.ActiveIssues[issueIndex] = existingIssue;
        }

        private List<AiStateModel> ResolveAffectedAis(GlobalGameState state, GameEvent gameEvent)
        {
            var ais = new List<AiStateModel>();
            if (gameEvent.TargetAi.HasValue)
            {
                var ai = state.GetAi(gameEvent.TargetAi.Value);
                if (ai != null)
                {
                    ais.Add(ai);
                }
                return ais;
            }

            if (state?.Ais != null)
            {
                ais.AddRange(state.Ais.Where(ai => ai != null));
            }

            return ais;
        }

        private EventSeverity GetRandomSeverity()
        {
            var roll = _random.NextDouble();
            if (roll < 0.45)
            {
                return EventSeverity.Low;
            }

            if (roll < 0.75)
            {
                return EventSeverity.Medium;
            }

            return roll < 0.92 ? EventSeverity.High : EventSeverity.Critical;
        }

        private float GetSeverityMultiplier(EventSeverity severity)
        {
            return severity switch
            {
                EventSeverity.Low => 1f,
                EventSeverity.Medium => 1.5f,
                EventSeverity.High => 2.25f,
                EventSeverity.Critical => 3f,
                _ => 1f
            };
        }

        private float GetRandomDirection()
        {
            return _random.NextDouble() < 0.5 ? -1f : 1f;
        }

        private CountryModel GetRandomCountry(GlobalGameState state)
        {
            return state.Countries[_random.Next(state.Countries.Count)];
        }

        private AiStateModel GetRandomAi(GlobalGameState state)
        {
            return state.Ais[_random.Next(state.Ais.Count)];
        }

        private NationalIssueType GetRandomIssueType(CountryModel country)
        {
            var available = country.ActiveIssues?.Select(issue => issue.Type).ToList();
            if (available == null || available.Count == 0)
            {
                available = Enum.GetValues(typeof(NationalIssueType)).Cast<NationalIssueType>().ToList();
            }

            return available[_random.Next(available.Count)];
        }

        private static float ClampIntensity(float value)
        {
            return Math.Clamp(value, 0f, 100f);
        }
    }
}
