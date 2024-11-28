using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    public List<float> lapTimes;
    [SerializeField] private TextMeshProUGUI _timerText;
    private float _lapElapsedTime;
    private float _lapStartTime;

    private float minute;
    private float seconds;

    void Awake(){
        _lapStartTime = Time.time;
    }

    void Update()
    {
        _lapElapsedTime = Time.time - _lapStartTime;
        setText();
    }
    private void setText()
    {
        minute = _lapElapsedTime / 60;
        seconds = _lapElapsedTime % 60;
    
        _timerText.text = $"Time:{Mathf.FloorToInt(minute):00}:{seconds:00.00}";

    }
    public void endLap()
    {
        float temp = _lapElapsedTime;
        lapTimes.Add(temp);
        _lapElapsedTime = 0;
        _lapStartTime = Time.time;
    }
}
