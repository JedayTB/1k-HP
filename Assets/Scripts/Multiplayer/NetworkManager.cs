using System;
using System.Net;
using System.Net.Sockets;
using Riptide;
using Riptide.Transports.Udp;
using Riptide.Utils;
using UnityEngine;

public enum ServerToClient : ushort
{
    playerSpawned = 1,
}

public enum ClientToServerId : ushort
{
    name = 1,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    public Server Server { get; private set; }
    
    
    [SerializeField] private ushort port = 24561;
    [SerializeField] private ushort maxConnections = 4;
    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        
    }

    public void StartServer()
    {
        Server = new Server(new UdpServer(System.Net.IPAddress.Any));
        Debug.Log($"Server Started on IP: {GetLocalIPv4()}");
        Server.Start(port, maxConnections);
        Server.ClientDisconnected += PlayerLeft;
    }

    public void StopServer()
    {
        Server.Stop();
        Debug.Log("Game server stopped!");
    }

    private void FixedUpdate()
    {
        if(_singleton.Server != null && _singleton.Server.IsRunning) Server.Update();
    }

    private void OnApplicationQuit()
    {
        if(_singleton.Server != null && _singleton.Server.IsRunning) StopServer();
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Client.Id].gameObject);
    }
    
    // fuck c#
    string GetLocalIPv4()
    {
        string localIP = string.Empty;
        foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = address.ToString();
                break;
            }
        }
        return localIP;
    }
}
