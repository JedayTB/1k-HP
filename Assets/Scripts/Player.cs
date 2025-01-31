using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using System.Linq;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> List = new Dictionary<ushort, Player>();
    
    public ushort Id { get; private set; }
    public string Username { get; private set; }
    
    public bool IsReady { get; private set; }

    private void OnDestroy()
    {
        List.Remove(Id);
    }
    
    [SerializeField] private Transform[] japanSpawnPoints;

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in List.Values)
        {
            otherPlayer.SendSpawned(id);
            
            SendExistingPlayerToNewPlayer(id, otherPlayer);
        }
        
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f,1f,0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"{id}_{username}";
        player.Id = id;
        player.Username = username;
        
        player.SendSpawned();
        List.Add(id, player);

        Debug.Log($"Player {username} (ID: {id}) spawned on the server.");
    }
    
    private static void SendExistingPlayerToNewPlayer(ushort newPlayerId, Player existingPlayer)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.playerSpawned);
        message.AddUShort(existingPlayer.Id);
        message.AddString(existingPlayer.Username);
        message.AddVector3(existingPlayer.transform.position);
        NetworkManager.Singleton.Server.Send(message, newPlayerId);
        
        Debug.Log($"Sent existing player {existingPlayer.Id} to new player {newPlayerId}");
    }

    public void SetReady(bool isReady)
    {
        IsReady = isReady;
        CheckAllReady();
    }

    private static void CheckAllReady()
    {
        if (List.Count > 0 && List.Values.All(x => x.IsReady))
        {
            Debug.Log("All players are ready! Starting countdown...");
            GameLogic.Singleton.StartCoroutine(GameLogic.Singleton.CountdownToStartGame());
        } else Debug.Log("Not all players are ready.");
    }
    
    private static void NotifyClientsAboutReadyState(ushort id, bool isReady)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.playerReady);
        message.AddUShort(id);
        message.AddBool(isReady);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    
    private void Update()
    {
        SendPositionUpdate();
    }

    private void SendPositionUpdate()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClient.playerPosition);
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    
    #region Messages

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.readyUp)]
    private static void ReadyUp(ushort fromClientId, Message message)
    {
        bool isReady = message.GetBool();

        if (List.TryGetValue(fromClientId, out Player player))
        {
            Debug.Log($"Player {fromClientId} ({player.Username}) is now {(isReady ? "READY" : "NOT READY")}");
            
            player.SetReady(isReady);
            NotifyClientsAboutReadyState(fromClientId, isReady);
            
            CheckAllReady();
        }
    }
    
    [MessageHandler((ushort)ClientToServerId.playerPosition)]
    private static void HandlePlayerPosition(ushort fromClientId, Message message)
    {
        if (List.ContainsKey(fromClientId))
        {
            Player player = List[fromClientId];
            player.transform.position = message.GetVector3();
        }
    }
    
    #endregion
}
