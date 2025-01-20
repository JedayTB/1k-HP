using System;
using Riptide;
using Riptide.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ServerToClient : ushort
{
    playerSpawned = 1,
    playerReady = 2,
    startGame = 3,
    countdown = 4,
    loadLevel = 5,
}

public enum ClientToServerId : ushort
{
    name = 1,
    readyUp = 2,
    sceneLoaded = 3,
}

public class NetworkManager : MonoBehaviour
{
     private static NetworkManager _singleton;
    
        public static NetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                {
                    _singleton = value;
                    DontDestroyOnLoad(value);
                }
                else if (_singleton != value)
                {
                    _singleton.mpDebug($"{nameof(NetworkManager)} instance already exists, destroying object!");
                    Destroy(value);
                }
            }
        }
        
        public Client Client { get; private set; }

        [SerializeField] public Transform[] playerLobbySpawns;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            RiptideLogger.Initialize(_singleton.mpDebug,  _singleton.mpDebug, Debug.LogWarning, Debug.LogError, false);
            
            Client = new Client();
            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientDisconnected += PlayerLeft;
            Client.Disconnected += DidDisconnect;
        }

        private void FixedUpdate()
        {
            Client.Update();
        }

        private void OnApplicationQuit()
        {
            Client.Disconnect();
        }

        public void Connect(string address = "127.0.0.1", string port = "24561")
        {
            Client.Connect($"{address}:{port}");
        }

        private void DidConnect(object sender, EventArgs e)
        {
            string username = UIManager.Singleton.usernameField.text;
            ushort id = Client.Id;
            UIManager.Singleton.AddPlayer(username, id);
            
            UIManager.Singleton.SendName();
        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            UIManager.Singleton.BackToConnection();
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            UIManager.Singleton.RemovePlayer(e.Id);
            Destroy(MP_Player.List[e.Id].gameObject);
        }

        private void DidDisconnect(object sender, EventArgs e)
        {
            UIManager.Singleton.BackToConnection();
        }

        public void mpDebug(string message)
        {
            Debug.Log($"<color=grey>[</color><color=teal>NETWORKING</color><color=grey>]</color> - {message}");
        }
        
        /*[MessageHandler((ushort)ServerToClient.playerSpawned)]
        private static void HandlePlayerSpawned(Message message)
        {
            ushort playerId = message.GetUShort();
            Vector3 spawnPosition = message.GetVector3();
            
            GameObject playerPrefab = UIManager.Singleton.GetPlayerPrefab();
            GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            _singleton.mpDebug($"Spawned player {playerId} at {spawnPosition} using prefab for {SceneManager.GetActiveScene().name}");
            
            if (SceneManager.GetActiveScene().name == "MP_JapanLevel")
            {
                playerObject.AddComponent<MP_Player>();
            }
        }*/
}
