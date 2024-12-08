using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Riptide;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MP_Player : MonoBehaviour
{
    public static Dictionary<ushort, MP_Player> list = new Dictionary<ushort, MP_Player>();
    
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    private string username;

    [SerializeField] private TextMeshProUGUI textAboveHead;


    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        MP_Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, NetworkManager.Singleton.playerLobbySpawns[id - 1].position, Quaternion.identity).GetComponent<MP_Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, NetworkManager.Singleton.playerLobbySpawns[id - 1].position, Quaternion.identity).GetComponent<MP_Player>();
            player.IsLocal = false;
        }

        player.Id = id;
        player.username = username;

        player.name = $"{id}_{username}";
        player.textAboveHead.text = player.name;
       
        
        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClient.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString());
    }

    private static void DespawnPlayer()
    {

    }
}
