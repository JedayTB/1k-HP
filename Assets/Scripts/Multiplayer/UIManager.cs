
using System;
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
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField addressField;
    [SerializeField] private TMP_InputField portField;
    
    [SerializeField] private GameObject advancedOptionsUI;

    private bool advancedOpen = false;

    private void Awake()
    {
        Singleton = this;
        advancedOptionsUI.gameObject.SetActive(false);
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
    }
}
