using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Riptide;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MP_Player : MonoBehaviour
{
    public static Dictionary<ushort, MP_Player> List = new Dictionary<ushort, MP_Player>();
    
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    
    public bool IsReady { get; private set; }

    private string username;

    [SerializeField] private TextMeshProUGUI textAboveHead;


    private void OnDestroy()
    {
        List.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        MP_Player player;

        NetworkManager.Singleton.mpDebug($"Local ID: ${id} | Singleton ID: ${NetworkManager.Singleton.Client.Id}");
        player = Instantiate((id == NetworkManager.Singleton.Client.Id ? GameLogic.Singleton.LocalPlayerPrefab : GameLogic.Singleton.PlayerPrefab), NetworkManager.Singleton.playerLobbySpawns[id - 1].position, Quaternion.identity).GetComponent<MP_Player>();
        player.IsLocal = id == NetworkManager.Singleton.Client.Id;

        NetworkManager.Singleton.mpDebug("Is player local? " + player.IsLocal);
 

        player.Id = id;
        player.username = username;

        player.name = $"{id}_{username}";
        player.textAboveHead.text = player.name;
       
        
        List.Add(id, player);
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
