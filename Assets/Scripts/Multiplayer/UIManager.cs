
using System;
using System.Collections.Generic;
using Riptide;
using Riptide.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;
    
    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }
    
    [Header("Connection")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] public TMP_InputField usernameField;
    [SerializeField] private TMP_InputField addressField;
    [SerializeField] private TMP_InputField portField;

    [Header("Information Menu")]
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private TMP_Text partyMembersText;
        
    [Header("Options")]
    [SerializeField] private GameObject advancedOptionsUI;

    private bool advancedOpen = false;

    private List<(string username, ushort id, bool isReady)> connectedPlayers = new List<(string, ushort, bool)>();
    
    private void Awake()
    {
        Singleton = this;
        advancedOptionsUI.gameObject.SetActive(false);
        lobbyUI.SetActive(false);
    }

    public void ConnectClicked()
    {
        usernameField.interactable = false;
        connectUI.SetActive(false);

        if (addressField.text.Length > 0 && portField.text.Length > 0)
        {
            NetworkManager.Singleton.Connect(addressField.text, portField.text);
        }
        else if (addressField.text.Length > 0)
        {
            NetworkManager.Singleton.Connect(addressField.text);
        }
        else if (portField.text.Length > 0)
        {
            NetworkManager.Singleton.Connect("127.0.0.1", portField.text);
        }
        else
        {
            NetworkManager.Singleton.Connect();
        }

    }

    public void AdvancedOptionsClicked()
    {
        advancedOpen = !advancedOpen;
        advancedOptionsUI.SetActive(advancedOpen);
    }

    public void BackToConnection()
    {
        usernameField.interactable = true;
        connectUI.SetActive(true);
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerId.name);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
        
        lobbyUI.SetActive(true);
    }

    public void AddPlayer(string username, ushort id)
    {
        connectedPlayers.Add((username, id, false));
        UpdatePartyMembersText();
    }
    
    public void RemovePlayer(ushort id)
    {
        connectedPlayers.RemoveAll(player => player.id == id);
        UpdatePartyMembersText();
    }
    
    private void UpdatePartyMembersText()
    {
        string playerInfo = $"Connected Players: {connectedPlayers.Count}/4\n";
        foreach (var player in connectedPlayers)
        {
            string readyStatus = player.isReady ? "[READY]" : "[UNREADY]";
            playerInfo += $"{player.username} ({player.id}) {readyStatus}\n";
        }
        partyMembersText.text = playerInfo.TrimEnd();
    }

    public void ReadyUpClicked()
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerId.readyUp);
        message.AddBool(true); // add toggling ready? maybe
        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClient.playerReady)]
    private static void HandlePlayerReady(Message message)
    {
        ushort playerId = message.GetUShort();
        bool isReady = message.GetBool();

        UIManager.Singleton.UpdateReadyState(playerId, isReady);
    }
    
    public void UpdateReadyState(ushort playerId, bool isReady)
    {
        int index = connectedPlayers.FindIndex(player => player.id == playerId);
        if (index != -1)
        {
            connectedPlayers[index] = (connectedPlayers[index].username, connectedPlayers[index].id, isReady);
            UpdatePartyMembersText();
        }
    }

    [MessageHandler((ushort)ServerToClient.startGame)]
    private static void StartGame(Message message)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MP_JapanLevel");
    }

}
