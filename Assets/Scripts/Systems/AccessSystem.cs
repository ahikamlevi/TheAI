using System;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Manages access levels for AIs within specific countries, including ban and preference effects.
    /// </summary>
    public class AccessSystem
    {
        private const float PreferredMarketShareBonus = 0.1f;

        public void SetAccessLevel(GlobalGameState state, CountryId countryId, AiId ai, AiAccessLevel level)
        {
            if (state == null)
            {
                return;
            }

            switch (level)
            {
                case AiAccessLevel.Banned:
                    ApplyBan(state, countryId, ai);
                    break;
                case AiAccessLevel.Preferred:
                    ApplyPreference(state, countryId, ai);
                    break;
                default:
                    var country = state.GetCountry(countryId);
                    country?.SetAccessLevel(ai, level);
                    break;
            }
        }

        public void ApplyBan(GlobalGameState state, CountryId countryId, AiId ai)
        {
            var country = state?.GetCountry(countryId);
            if (country == null)
            {
                return;
            }

            country.SetAccessLevel(ai, AiAccessLevel.Banned);

            var currentShare = GetMarketShare(country, ai);
            if (currentShare > 0f)
            {
                country.ApplyMarketShareChange(ai, -currentShare);
            }
        }

        public void ApplyPreference(GlobalGameState state, CountryId countryId, AiId ai)
        {
            var country = state?.GetCountry(countryId);
            if (country == null)
            {
                return;
            }

            country.SetAccessLevel(ai, AiAccessLevel.Preferred);

            var currentShare = GetMarketShare(country, ai);
            var preferredShare = Math.Clamp(currentShare + PreferredMarketShareBonus, 0f, 1f);
            country.ApplyMarketShareChange(ai, preferredShare - currentShare);
        }

        public bool IsAiBannedInCountry(GlobalGameState state, CountryId id, AiId ai)
        {
            var country = state?.GetCountry(id);
            if (country == null)
            {
                return false;
            }

            var accessLevel = ai switch
            {
                AiId.Player => country.PlayerAccess,
                AiId.Rival1 => country.Rival1Access,
                AiId.Rival2 => country.Rival2Access,
                AiId.Rival3 => country.Rival3Access,
                _ => AiAccessLevel.Restricted
            };

            return accessLevel == AiAccessLevel.Banned;
        }

        private static float GetMarketShare(CountryModel country, AiId ai)
        {
            return ai switch
            {
                AiId.Player => country.PlayerMarketShare,
                AiId.Rival1 => country.Rival1MarketShare,
                AiId.Rival2 => country.Rival2MarketShare,
                AiId.Rival3 => country.Rival3MarketShare,
                _ => 0f
            };
        }
    }
}
