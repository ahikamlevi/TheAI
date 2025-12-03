using System.Collections.Generic;
using UnityEngine;

namespace TheAI.Data
{
    [CreateAssetMenu(fileName = "CountriesDatabase", menuName = "TheAI/Data/Countries Database", order = 1)]
    public class CountriesDatabase : ScriptableObject
    {
        public List<CountryConfig> Countries = new();
    }
}
