using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;

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
    
    public GameObject PlayerPrefab => playerPrefab;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Game Spawns")]
    public Transform[] gameSpawns;

    private void Awake()
    {
        Singleton = this;
    }
    
    public IEnumerator CountdownToStartGame()
    {
        for (int secondsLeft = 10; secondsLeft > 0; secondsLeft--)
        {
            NotifyCountdown(secondsLeft);
            Debug.Log($"Game starting in {secondsLeft} seconds...");

            yield return new WaitForSeconds(1f);
        }
        
        Debug.Log("Countdown complete! Starting the game...");
        LoadLevel();
    }
    
    private void NotifyCountdown(int secondsLeft)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.countdown);
        message.AddInt(secondsLeft);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    
    private void LoadLevel()
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.loadLevel);
        message.AddString("MP_CityLevel");
        NetworkManager.Singleton.Server.SendToAll(message);

        Debug.Log("Switching players to game level: MP_CityLevel");
        
        SyncPlayersToGameSpawnPoints();
    }
    
    public void SyncPlayersToGameSpawnPoints()
    {
        Debug.Log("Syncing all players to new spawn points...");
        Transform[] spawnPoints = GameLogic.Singleton.gameSpawns;

        int index = 0;
        foreach (Player player in Player.List.Values)
        {
            Transform spawnPoint = spawnPoints[index % spawnPoints.Length];
            player.transform.position = spawnPoint.position;
            Debug.Log($"Sent updated position for {player.Username} at {spawnPoint.position}");
            index++;
        }
    }
}
