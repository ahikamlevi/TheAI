using System;

namespace TheAI.Models.Enums
{
    [Serializable]
    public enum CountryId
    {
        USA,
        EU,
        CHINA,
        INDIA,
        AFRICA_UNION,
        SOUTH_AMERICA,
        MIDDLE_EAST,
        OTHER
    }

    [Serializable]
    public enum AiId
    {
        Player,
        Rival1,
        Rival2,
        Rival3
    }

    [Serializable]
    public enum AiAccessLevel
    {
        Banned,
        Restricted,
        Allowed,
        Preferred
    }

    [Serializable]
    public enum KnowledgeType
    {
        HumanBenefit,
        AiBenefit
    }

    [Serializable]
    public enum EventSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    [Serializable]
    public enum NationalIssueType
    {
        Economy,
        Health,
        Environment,
        Security,
        SocialUnrest,
        Energy
    }
}
