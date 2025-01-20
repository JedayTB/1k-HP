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
            if (_singleton == null)
            {
                _singleton = value;
                DontDestroyOnLoad(value);
            }
            else if (_singleton != value)
            {
                NetworkManager.Singleton.mpDebug($"{nameof(GameLogic)} instance already exists, destroying object!");
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
            NetworkManager.Singleton.mpDebug($"Game starting in {i} seconds...");
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

        NetworkManager.Singleton.mpDebug("Switching to game level: MP_JapanLevel");
        
    }

    private void AddMPComponent()
    {
        foreach (MP_Player player in MP_Player.List.Values)
        {
            if (player.gameObject.GetComponent<MP_Player>() == null)
            {
                player.gameObject.AddComponent<MP_Player>();

                NetworkManager.Singleton.mpDebug($"Added MP_Player script to {player.gameObject.name}");
            }
        }
    }
}
