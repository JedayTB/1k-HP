using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Riptide;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        GameObject playerPrefab = SceneManager.GetActiveScene().name == "MP_JapanLevel" ? GameLogic.Singleton.LocalPlayerPrefab : GameLogic.Singleton.PlayerPrefab;

        MP_Player player = Instantiate(playerPrefab, position, Quaternion.identity).GetComponent<MP_Player>();
        
        player.Id = id;
        player.username = username;

        player.name = $"{id}_{username}";

        NetworkManager.Singleton.mpDebug($"Spawned {username} (ID: {id}) at {position} on client.");

        if (player.GetComponent<MP_Player>())
        {
            NetworkManager.Singleton.mpDebug($"Player {username} has a MP_Player");
        } else NetworkManager.Singleton.mpDebug($"Player {username} has NO MP_Player");

        if(!List.ContainsKey(id)) List.Add(id, player);
    }
    
    private void FixedUpdate()
    {
        if (IsLocal)
        {
            SendPositionUpdate();
        }
    }

    private void SendPositionUpdate()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ClientToServerId.playerPosition);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClient.playerSpawned)]
    private static void HandlePlayerSpawned(Message message)
    {
        ushort playerId = message.GetUShort();
        string username = message.GetString();
        Vector3 position = message.GetVector3();

        NetworkManager.Singleton.mpDebug($"Received spawn message for Player {username} (ID: {playerId}) at {position}");

        if (List.ContainsKey(playerId))
        {
            List[playerId].transform.position = position;
            NetworkManager.Singleton.mpDebug($"Updated existing player {username} position.");
        }
        else
        {
            Spawn(playerId, username, position);
            NetworkManager.Singleton.mpDebug($"Spawned new player {username} at {position}");
        }
    }
    
    [MessageHandler((ushort)ServerToClient.playerPosition)]
    private static void HandlePlayerPosition(Message message) 
    {
        ushort playerId = message.GetUShort();
        Vector3 newPosition = message.GetVector3();

        if (List.ContainsKey(playerId))
        {
            MP_Player player = List[playerId];

            if (!player.IsLocal)
            {
                player.transform.position = Vector3.Lerp(player.transform.position, newPosition, Time.deltaTime * 10f);
            }
        }
    }
}
