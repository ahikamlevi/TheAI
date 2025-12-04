using TheAI.Models;
using TheAI.Models.Enums;

namespace TheAI.Systems
{
    /// <summary>
    /// Centralizes changes to national and global approval values.
    /// </summary>
    public class ApprovalSystem
    {
        public void ChangeNationalApproval(GlobalGameState state, CountryId countryId, AiId ai, float delta)
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

            country.ApplyApprovalChange(ai, delta);
            RecalculateGlobalApproval(state, ai);
        }

        public void RecalculateGlobalApproval(GlobalGameState state, AiId ai)
        {
            if (state?.Countries == null || state.Countries.Count == 0)
            {
                return;
            }

            var aiState = state.GetAi(ai);
            if (aiState == null)
            {
                return;
            }

            float totalApproval = 0f;
            int countryCount = 0;

            foreach (var country in state.Countries)
            {
                if (country == null)
                {
                    continue;
                }

                var approval = ai switch
                {
                    AiId.Player => country.PublicApprovalPlayer,
                    AiId.Rival1 => country.PublicApprovalRival1,
                    AiId.Rival2 => country.PublicApprovalRival2,
                    AiId.Rival3 => country.PublicApprovalRival3,
                    _ => 0f
                };

                totalApproval += approval;
                countryCount++;
            }

            if (countryCount == 0)
            {
                return;
            }

            aiState.GlobalPublicApproval = totalApproval / countryCount;
        }
    }
}
