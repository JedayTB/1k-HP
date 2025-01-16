using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;
    
    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;
    
    public IEnumerator CountdownToStartGame()
    {
        for (int i = 10; i > 0; i--)
        {
            NotifyCountdown(i);
            Debug.Log($"Game starting in {i} seconds...");
            yield return new WaitForSeconds(1f);
        }
        
        LoadLevel();
    }

    private void NotifyCountdown(int secondsRemaining)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.countdown);
        message.AddInt(secondsRemaining);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void LoadLevel()
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.loadLevel);
        message.AddString("MP_JapanLevel");
        NetworkManager.Singleton.Client.Send(message);

        Debug.Log("Switching to game level: MP_JapanLevel");
        
    }
}
