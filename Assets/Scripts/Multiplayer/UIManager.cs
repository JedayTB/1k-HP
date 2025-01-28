using System.Collections.Generic;
using Riptide;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
  private static UIManager _singleton;

  public static UIManager Singleton
  {
    get => _singleton;
    private set
    {
      if (_singleton == null)
        _singleton = value;
      else if (_singleton != value)
      {
        NetworkManager.Singleton.mpDebug($"{nameof(UIManager)} instance already exists, destroying object!");
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
  [SerializeField] private TMP_Text countdownText;

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

    NetworkManager.Singleton.mpDebug("<color=grey>[</color><color=teal>NETWORKING</color><color=grey>]</color> - Ready message sent to server");
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
    UnityEngine.SceneManagement.SceneManager.LoadScene("MP_CityLevel");
  }

  [MessageHandler((ushort)ServerToClient.countdown)]
  private static void StartCountdown(Message message)
  {
    int seconds = message.GetInt();
    NetworkManager.Singleton.mpDebug($"Countdown: {seconds} seconds remaining...");

    if (Singleton != null && Singleton.countdownText != null)
    {
      Singleton.countdownText.text = $"Game starts in {seconds} seconds!";
    }
  }

  [MessageHandler((ushort)ServerToClient.loadLevel)]
  private static void HandleLoadLevel(Message message)
  {
    string levelName = message.GetString();
    NetworkManager.Singleton.mpDebug($"Loading level: {levelName}");

    UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);

    NotifyServerSceneLoaded();
  }

  private static void NotifyServerSceneLoaded()
  {
    Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerId.sceneLoaded);
    NetworkManager.Singleton.Client.Send(message);

    NetworkManager.Singleton.mpDebug("Notified server that the scene has been loaded.");
  }

}
