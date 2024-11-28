using System;
using System.Collections.Generic;
using Riptide;
using UnityEngine;

public class MP_Player : MonoBehaviour
{
    public static Dictionary<ushort, MP_Player> list = new Dictionary<ushort, MP_Player>();
    
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    private string username;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        MP_Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<MP_Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<MP_Player>();
            player.IsLocal = false;
        }
        
        player.name = $"{id}_{username}";
        player.Id = id;
        player.username = username;
        
        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClient.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }
}
