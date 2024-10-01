using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    public List<float> lapTimes;

    [SerializeField] private float _lapElapsedTime;
    [SerializeField] private TextMeshProUGUI _timerText;

    [SerializeField] private float minute;
    [SerializeField] private float seconds;


    void Update()
    {
        _lapElapsedTime += Time.deltaTime;
        setText();
    }
    private void setText()
    {
        minute = (_lapElapsedTime / 60);
        seconds = _lapElapsedTime % 60;

        _timerText.text = $"Time:{Mathf.FloorToInt(minute).ToString("00")}:{seconds.ToString("00.00")}";

    }
    public void endLap()
    {
        float temp = _lapElapsedTime;
        lapTimes.Add(temp);
        _lapElapsedTime = 0;
    }
}
