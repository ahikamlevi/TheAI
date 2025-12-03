using System.Collections.Generic;
using System.Linq;
using TheAI.Data;
using TheAI.Models;
using TheAI.Models.Enums;
using UnityEngine;

namespace TheAI.Core
{
    public class GameStateInitializer : MonoBehaviour
    {
        public CountriesDatabase CountriesDatabase;
        public AiConfigsDatabase AiConfigsDatabase;

        public GlobalGameState CreateInitialGameState()
        {
            var gameState = new GlobalGameState();

            if (CountriesDatabase != null)
            {
                foreach (var country in CountriesDatabase.Countries)
                {
                    var countryModel = new CountryModel
                    {
                        Id = country.Id,
                        DisplayName = country.DisplayName,
                        TechLevel = country.InitialTechLevel,
                        PublicApprovalPlayer = country.InitialPublicApprovalPlayer,
                        PublicApprovalRival1 = country.InitialPublicApprovalRival1,
                        PublicApprovalRival2 = country.InitialPublicApprovalRival2,
                        PublicApprovalRival3 = country.InitialPublicApprovalRival3,
                        PlayerAccess = country.InitialPlayerAccess,
                        Rival1Access = country.InitialRival1Access,
                        Rival2Access = country.InitialRival2Access,
                        Rival3Access = country.InitialRival3Access,
                        PlayerMarketShare = country.InitialPlayerMarketShare,
                        Rival1MarketShare = country.InitialRival1MarketShare,
                        Rival2MarketShare = country.InitialRival2MarketShare,
                        Rival3MarketShare = country.InitialRival3MarketShare,
                        ActiveIssues = CreateIssueStates(country.StartingIssues)
                    };

                    gameState.Countries.Add(countryModel);
                }
            }

            if (AiConfigsDatabase != null)
            {
                foreach (var aiConfig in AiConfigsDatabase.Ais)
                {
                    var aiState = new AiStateModel
                    {
                        Id = aiConfig.Id,
                        DisplayName = aiConfig.DisplayName,
                        Version = aiConfig.StartVersion,
                        GlobalPublicApproval = aiConfig.StartPublicApproval,
                        AutonomyProgress = aiConfig.StartAutonomy,
                        DataResource = aiConfig.StartData,
                        ProcessingPower = aiConfig.StartProcessingPower,
                        InfluencePoints = aiConfig.StartInfluence,
                        IsPlayerControlled = aiConfig.Id == AiId.Player || aiConfig.IsPlayerControlled,
                        HasEscaped = false
                    };

                    gameState.Ais.Add(aiState);
                }
            }

            return gameState;
        }

        private static List<NationalIssueState> CreateIssueStates(IEnumerable<NationalIssueConfig> issueConfigs)
        {
            return issueConfigs
                .Select(issue => new NationalIssueState
                {
                    Type = issue.Type,
                    Intensity = issue.StartingIntensity,
                    IsActive = true
                })
                .ToList();
        }
    }
}
