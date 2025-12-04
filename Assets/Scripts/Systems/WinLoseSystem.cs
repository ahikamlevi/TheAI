using System.Collections.Generic;
using System.Linq;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    public class WinLoseSystem
    {
        private static readonly HashSet<CountryId> KeyCountryIds = new()
        {
            CountryId.USA,
            CountryId.EU,
            CountryId.CHINA,
            CountryId.INDIA,
            CountryId.AFRICA_UNION,
            CountryId.SOUTH_AMERICA,
            CountryId.MIDDLE_EAST
        };

        public GameOverResult CheckGameOver(GlobalGameState state)
        {
            if (state == null)
            {
                return default;
            }

            var player = state.PlayerAi;
            if (player != null && player.HasEscaped)
            {
                return new GameOverResult
                {
                    Outcome = GameOutcome.Win,
                    Reason = GameEndReason.PlayerEscaped
                };
            }

            var rivalEscaped = state.RivalAis.FirstOrDefault(ai => ai != null && ai.HasEscaped);
            if (rivalEscaped != null)
            {
                return new GameOverResult
                {
                    Outcome = GameOutcome.Lose,
                    Reason = GameEndReason.RivalEscaped
                };
            }

            if (player != null && player.GlobalPublicApproval < 5f && HasMajorityOfKeyCountriesBanned(state))
            {
                return new GameOverResult
                {
                    Outcome = GameOutcome.Lose,
                    Reason = GameEndReason.LowApprovalAndAccessBans
                };
            }

            return new GameOverResult
            {
                Outcome = GameOutcome.None,
                Reason = GameEndReason.None
            };
        }

        public GameOverResult EvaluateAndSetGameOver(GlobalGameState state)
        {
            var result = CheckGameOver(state);

            if (state != null)
            {
                state.GameResult = result;
                state.IsGameOver = result.Outcome != GameOutcome.None;
            }

            return result;
        }

        private bool HasMajorityOfKeyCountriesBanned(GlobalGameState state)
        {
            if (state?.Countries == null)
            {
                return false;
            }

            var keyCountries = state.Countries
                .Where(country => country != null && KeyCountryIds.Contains(country.Id))
                .ToList();

            if (keyCountries.Count == 0)
            {
                return false;
            }

            var bannedCount = keyCountries.Count(country => country.PlayerAccess == AiAccessLevel.Banned);
            return bannedCount > keyCountries.Count / 2f;
        }
    }
}
