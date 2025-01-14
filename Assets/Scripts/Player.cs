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

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in List.Values)
            otherPlayer.SendSpawned(id);
        
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f,1f,0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"{id}_{username}";
        player.Id = id;
        player.Username = username;
        
        player.SendSpawned();
        List.Add(id, player);

        print(List.ToArray());
    }
    void printArr(IEnumerable obj)
    {
        foreach(var b in obj)
        {
            print(b);
        }
    }

    public void SetReady(bool isReady)
    {
        IsReady = isReady;
        CheckAllReady();
    }

    private void CheckAllReady()
    {
        if (List.Count > 0 && List.Values.All(x => x.IsReady))
        {
            //NetworkManager.Singleton.StartGame();
        }
    }
    
    private static void NotifyClientsAboutReadyState(ushort id, bool isReady)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.playerReady);
        message.AddUShort(id);
        message.AddBool(isReady);
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
            player.SetReady(isReady);
            NotifyClientsAboutReadyState(fromClientId, isReady);
        }
    }
    
    #endregion
}
