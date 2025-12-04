using System.Collections.Generic;
using TheAI.Models;
using TheAI.Models.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace TheAI.UI
{
    public class CountryListController : MonoBehaviour
    {
        [Header("References")]
        public GameObject CountryRowPrefab;
        public Transform ContentRoot;

        [Header("Runtime State")]
        public GlobalGameState GameState;
        public CountryId? SelectedCountryId;

        public UnityEvent<CountryId> OnCountrySelected;

        private readonly List<CountryRowView> _spawnedRows = new();
        private readonly Dictionary<CountryId, CountryRowView> _rowLookup = new();

        public void SetGameState(GlobalGameState gameState)
        {
            GameState = gameState;
            RefreshList();
        }

        public void RefreshList()
        {
            ClearExistingRows();

            if (CountryRowPrefab == null || ContentRoot == null || GameState == null)
            {
                return;
            }

            foreach (var country in GameState.Countries)
            {
                var instance = Instantiate(CountryRowPrefab, ContentRoot);
                var rowView = instance.GetComponent<CountryRowView>();

                if (rowView == null)
                {
                    continue;
                }

                rowView.Bind(country);

                var countryId = country.Id;
                rowView.SetOnClickListener(() => SelectCountry(countryId));
                rowView.SetSelected(SelectedCountryId.HasValue && SelectedCountryId.Value == countryId);

                _spawnedRows.Add(rowView);
                _rowLookup[countryId] = rowView;
            }
        }

        public void SelectCountry(CountryId countryId)
        {
            SelectedCountryId = countryId;
            UpdateSelectionHighlights();
            OnCountrySelected?.Invoke(countryId);
        }

        private void UpdateSelectionHighlights()
        {
            foreach (var row in _spawnedRows)
            {
                if (row == null)
                {
                    continue;
                }

                row.SetSelected(false);
            }

            if (!SelectedCountryId.HasValue)
            {
                return;
            }

            if (_rowLookup.TryGetValue(SelectedCountryId.Value, out var selectedRow) && selectedRow != null)
            {
                selectedRow.SetSelected(true);
            }
        }

        private void ClearExistingRows()
        {
            foreach (var row in _spawnedRows)
            {
                if (row != null)
                {
                    Destroy(row.gameObject);
                }
            }

            _spawnedRows.Clear();
            _rowLookup.Clear();
        }
    }
}
