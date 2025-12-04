using System.Linq;
using TheAI.Models;
using TheAI.Models.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace TheAI.UI
{
    public class CountryRowView : MonoBehaviour
    {
        [Header("UI References")]
        public Text CountryNameText;
        public Text ApprovalText;
        public Text MarketShareText;
        public Text AccessLevelText;
        public Image IssueIndicator;
        public Image SelectionHighlight;
        public Button SelectButton;

        [Header("Appearance")]
        public Color ActiveIssueColor = Color.red;
        public Color NoIssueColor = Color.gray;

        public void Bind(CountryModel country)
        {
            if (country == null)
            {
                return;
            }

            if (CountryNameText != null)
            {
                CountryNameText.text = country.DisplayName;
            }

            if (ApprovalText != null)
            {
                ApprovalText.text = $"Approval: {country.PublicApprovalPlayer:F1}%";
            }

            if (MarketShareText != null)
            {
                MarketShareText.text = $"Market Share: {country.PlayerMarketShare * 100f:F1}%";
            }

            if (AccessLevelText != null)
            {
                AccessLevelText.text = $"Access: {FormatAccessLevel(country.PlayerAccess)}";
            }

            UpdateIssueIndicator(country);
        }

        public void SetSelected(bool isSelected)
        {
            if (SelectionHighlight != null)
            {
                SelectionHighlight.enabled = isSelected;
            }
        }

        public void SetOnClickListener(UnityEngine.Events.UnityAction callback)
        {
            if (SelectButton == null)
            {
                return;
            }

            SelectButton.onClick.RemoveAllListeners();
            if (callback != null)
            {
                SelectButton.onClick.AddListener(callback);
            }
        }

        private void UpdateIssueIndicator(CountryModel country)
        {
            if (IssueIndicator == null)
            {
                return;
            }

            var hasActiveIssues = country.ActiveIssues != null && country.ActiveIssues.Any(issue => issue.IsActive);
            IssueIndicator.color = hasActiveIssues ? ActiveIssueColor : NoIssueColor;
        }

        private static string FormatAccessLevel(AiAccessLevel access)
        {
            return access switch
            {
                AiAccessLevel.Preferred => "Preferred",
                AiAccessLevel.Allowed => "Allowed",
                AiAccessLevel.Restricted => "Restricted",
                AiAccessLevel.Banned => "Banned",
                _ => "Unknown"
            };
        }
    }
}
