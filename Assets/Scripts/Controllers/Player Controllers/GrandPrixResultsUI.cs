using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GrandPrixResultsUI : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> StageNames = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> _stageResults = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI _overallRankText;

    private int _overallRank;

    void Start()
    {
        for (int i = 0; i <= 3; i++)
        {
            int placement = GrandPrixManager.RacePlacements[i];

            if (placement == 0)
            {
                _stageResults[i].text = "NA";
            }
            else
            {
                _stageResults[i].text = placement.ToString();
                _overallRank += placement;
            }
        }

        _overallRankText.text = (_overallRank/GrandPrixManager.GrandPrixLength).ToString();
    }
}
