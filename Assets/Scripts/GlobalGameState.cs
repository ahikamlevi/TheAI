using System;
using System.Collections.Generic;
using System.Linq;
using TheAI.Models.Enums;

namespace TheAI.Models
{
    [Serializable]
    public class GlobalGameState
    {
        public List<CountryModel> Countries = new();
        public List<AiStateModel> Ais = new();

        public int CurrentTurn;
        public bool IsGameOver;
        public GameOverResult GameResult;

        public AiStateModel PlayerAi => GetAi(AiId.Player);
        public List<AiStateModel> RivalAis => Ais.Where(ai => ai.Id != AiId.Player).ToList();

        public CountryModel GetCountry(CountryId id)
        {
            return Countries.FirstOrDefault(country => country.Id == id);
        }

        public AiStateModel GetAi(AiId id)
        {
            return Ais.FirstOrDefault(ai => ai.Id == id);
        }

        public void AdvanceTurn()
        {
            CurrentTurn++;
        }
    }

    [Serializable]
    public struct GameOverResult
    {
        public GameOutcome Outcome;
        public GameEndReason Reason;
    }
}
