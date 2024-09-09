using System;
using UnityEditor.Build.Content;
using UnityEngine;

public delegate void CheckFinishedLap();
public class LapChecker : MonoBehaviour
{
    [SerializeField] private lapCheckpoint[] _checkpoints;

    public CheckFinishedLap checkFinishedLap;
    public int lapCount = 0;
    public void Init()
    {
        _checkpoints = GetComponentsInChildren<lapCheckpoint>();
        checkFinishedLap = checkIfLapsPassed;

        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].Init(i, checkFinishedLap);
        }
        
    }
    void Start()
    {
        Init();
    }
    void checkIfLapsPassed()
    {
        foreach(var chkpnt in _checkpoints){
            if(chkpnt.passedCheckpoint != true) return;
        }
        
        lapCount++;
        foreach(var chkpnt in _checkpoints){
            chkpnt.passedCheckpoint = false;
        }
        print($"{lapCount}");
    }

    void OnDrawGizmos()
    {
        _checkpoints = GetComponentsInChildren<lapCheckpoint>();
        for(int i = 0; i < _checkpoints.Length; i++){
            _checkpoints[i].name = $"Checkpoint {i + 1}";
        }
    }
}
