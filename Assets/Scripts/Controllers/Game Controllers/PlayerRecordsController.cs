using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder.MeshOperations;

public class PlayerRecordsController : MonoBehaviour
{
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private List<TextMeshProUGUI> timeTexts;
    [SerializeField] private List<TextMeshProUGUI> scoreTexts;

    [Header("Rank Colors")]
    [SerializeField] private MedalValues medalValues;
    [SerializeField] private Color whydidtheygrindscore;
    [SerializeField] private Color authorColor;
    [SerializeField] private Color LegendColor;
    [SerializeField] private Color devColor;
    [SerializeField] private Color platinumColor;
    [SerializeField] private Color goldColor;
    [SerializeField] private Color silverColor;
    [SerializeField] private Color bronzeColor;
    [SerializeField] private Color woodColor;

    [Header("Event System")]
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GameObject initialSelected;
    [SerializeField] private GameObject initialSelectedAfterClose;

    public void OpenPlayerRecords()
    {
        highScorePanel.SetActive(true);
        eventSystem.SetSelectedGameObject(initialSelected);

        for (int i = 0; i < timeTexts.Count; i++)
        {
            float tempTime = PlayerPrefs.GetFloat($"BestTime{i + 1}");
            int tempScore = PlayerPrefs.GetInt($"HighScore{i + 1}");

            if (tempTime == 99999999f || tempTime == 0f)
            {
                timeTexts[i].text = $"#{i + 1}: --------";
            }
            else
            {
                float minutes = tempTime / 60;
                float seconds = tempTime % 60;

                timeTexts[i].text = $"#{i + 1}: {Mathf.FloorToInt(minutes):00}:{seconds:00.00}";
                timeTexts[i].color = DetermineRankColorTime(tempTime);
            }

            if (tempScore == 0)
            {
                scoreTexts[i].text = $"#{i + 1}: --------";
            }
            else
            {
                scoreTexts[i].text = $"#{i + 1}: {tempScore}";
                scoreTexts[i].color = DetermineRankColorScore(tempScore);
            }
        }
    }

    public void ClosePlayerRecords()
    {
        highScorePanel.SetActive(false);
        eventSystem.SetSelectedGameObject(initialSelectedAfterClose);
    }

    private Color DetermineRankColorTime(float time)
    {
        if (time < medalValues.AuthorLapTime)
        {
            return authorColor;
        }
        else if(time < medalValues.subtwominute){
            return LegendColor;
        }
        else if (time < medalValues.DevLapTime)
        {
            return devColor;
        }
        else if (time < medalValues.PlatinumLapTime)
        {
            return platinumColor;
        }
        else if (time < medalValues.GoldLapTime)
        {
            return goldColor;
        }
        else if (time < medalValues.SilverLapTime)
        {
            return silverColor;
        }
        else if (time < medalValues.BronzeLapTime)
        {
            return bronzeColor;
        }
        else
        {
            return woodColor;
        }
    }

    private Color DetermineRankColorScore(int score)
    {
        if(score > medalValues.WhyDidYouGrindScore_Score){
            return whydidtheygrindscore;
        }
        else if (score > medalValues.DevScore)
        {
            return devColor;
        }
        else if (score > medalValues.PlatinumScore)
        {
            return platinumColor;
        }
        else if (score > medalValues.GoldScore)
        {
            return goldColor;
        }
        else if (score > medalValues.SilverScore)
        {
            return silverColor;
        }
        else if (score > medalValues.BronzeScore)
        {
            return bronzeColor;
        }
        else
        {
            return woodColor;
        }
    }
}
