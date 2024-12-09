
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

    private List<(string username, ushort id)> connectedPlayers = new List<(string, ushort)>();
    
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
        connectedPlayers.Add((username, id));
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
            playerInfo += $"{player.username} ({player.id})\n";
        }
        partyMembersText.text = playerInfo.TrimEnd();
    }
}
