using System;
using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Manages AI market share interactions within countries, handling competitive shifts and normalization.
    /// </summary>
    public class MarketShareSystem
    {
        private const float CompetitorReductionFactor = 0.25f;

        public void ChangeMarketShare(GlobalGameState state, CountryId countryId, AiId ai, float delta)
        {
            if (state == null)
            {
                return;
            }

            var country = state.GetCountry(countryId);
            if (country == null)
            {
                return;
            }

            if (delta > 0f)
            {
                ReduceCompetitors(country, ai, delta);
            }

            country.ApplyMarketShareChange(ai, delta);
            NormalizeMarketShare(country);
        }

        public void NormalizeMarketShare(CountryModel country)
        {
            if (country == null)
            {
                return;
            }

            country.PlayerMarketShare = Math.Clamp(country.PlayerMarketShare, 0f, 1f);
            country.Rival1MarketShare = Math.Clamp(country.Rival1MarketShare, 0f, 1f);
            country.Rival2MarketShare = Math.Clamp(country.Rival2MarketShare, 0f, 1f);
            country.Rival3MarketShare = Math.Clamp(country.Rival3MarketShare, 0f, 1f);

            var total = country.PlayerMarketShare
                        + country.Rival1MarketShare
                        + country.Rival2MarketShare
                        + country.Rival3MarketShare;

            if (total <= 1f || total <= 0f)
            {
                return;
            }

            var scale = 1f / total;
            country.PlayerMarketShare *= scale;
            country.Rival1MarketShare *= scale;
            country.Rival2MarketShare *= scale;
            country.Rival3MarketShare *= scale;
        }

        private void ReduceCompetitors(CountryModel country, AiId gainingAi, float delta)
        {
            var otherShares = GetOtherShares(country, gainingAi);
            var totalOtherShare = otherShares.player + otherShares.rival1 + otherShares.rival2 + otherShares.rival3;

            if (totalOtherShare <= 0f)
            {
                return;
            }

            var reductionPool = delta * CompetitorReductionFactor;

            country.ApplyMarketShareChange(AiId.Player, -reductionPool * otherShares.player / totalOtherShare);
            country.ApplyMarketShareChange(AiId.Rival1, -reductionPool * otherShares.rival1 / totalOtherShare);
            country.ApplyMarketShareChange(AiId.Rival2, -reductionPool * otherShares.rival2 / totalOtherShare);
            country.ApplyMarketShareChange(AiId.Rival3, -reductionPool * otherShares.rival3 / totalOtherShare);
        }

        private (float player, float rival1, float rival2, float rival3) GetOtherShares(CountryModel country, AiId ai)
        {
            return ai switch
            {
                AiId.Player => (0f, country.Rival1MarketShare, country.Rival2MarketShare, country.Rival3MarketShare),
                AiId.Rival1 => (country.PlayerMarketShare, 0f, country.Rival2MarketShare, country.Rival3MarketShare),
                AiId.Rival2 => (country.PlayerMarketShare, country.Rival1MarketShare, 0f, country.Rival3MarketShare),
                AiId.Rival3 => (country.PlayerMarketShare, country.Rival1MarketShare, country.Rival2MarketShare, 0f),
                _ => (0f, 0f, 0f, 0f)
            };
        }
    }
}
