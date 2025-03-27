using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerRecordsController : MonoBehaviour
{
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private List<TextMeshProUGUI> timeTexts;
    [SerializeField] private List<TextMeshProUGUI> scoreTexts;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            for (int i = 0; i < timeTexts.Count+1; i++)
            {
                PlayerPrefs.SetFloat($"BestTime{i}", i*i);
                PlayerPrefs.SetInt($"HighScore{i}", i*i);
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            OpenPlayerRecords();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ClosePlayerRecords();
        }
    }

    public void OpenPlayerRecords()
    {
        highScorePanel.SetActive(true);

        for (int i = 0; i < timeTexts.Count; i++)
        {
            timeTexts[i].text = $"#{i + 1}: {PlayerPrefs.GetFloat($"BestTime{i + 1}")}";
            scoreTexts[i].text = $"#{i + 1}: {PlayerPrefs.GetInt($"HighScore{i + 1}")}";
        }
    }

    public void ClosePlayerRecords()
    {
        highScorePanel.SetActive(false);
    }
}
