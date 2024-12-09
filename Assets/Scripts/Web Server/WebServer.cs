using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public class WebServer : MonoBehaviour // this might be the worst fucking class ive ever written i fucking HATE WEB SERVERS FUCK FUCK FUCK
{
    private HttpListener _listener;
    private bool _isRunning;

    private List<(string username, ushort id)> connectedPlayers = new List<(string username, ushort id)>
    {
        ("Player1", 1),
        ("Player2", 2) // TESTING DATA IGNOREEEEE
    };

    private void Start()
    {
        StartWebServer();
    }

    private void OnApplicationQuit()
    {
        StopWebServer();
    }

    public void StartWebServer()
    {
        if (_isRunning) return;

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:9090/");
        _listener.Start();
        _isRunning = true;

        Debug.Log($"Web server started");
        _listener.BeginGetContext(OnRequestReceived, null);
    }

    public void StopWebServer()
    {
        if (!_isRunning) return;

        _listener.Stop();
        _listener.Close();
        _isRunning = false;

        Debug.Log("Web server stopped");
    }

    private void OnRequestReceived(IAsyncResult result)
    {
        if (!_isRunning) return;
        
        var context = _listener.EndGetContext(result);
        var request = context.Request;
        var response = context.Response;

        string responseString = "";

        if (request.RawUrl.Contains("players"))
        {
            responseString = JsonConvert.SerializeObject(connectedPlayers);
        }
        else if (request.RawUrl.Contains("status"))
        {
            responseString = _isRunning ? "Running" : "Stopped";
        }
        else if (request.RawUrl.Contains("start"))
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("Game server started!");
        }
        else if (request.RawUrl.Contains("stop"))
        {
            NetworkManager.Singleton.StopServer();
        }
        else
        {
            responseString = "Unknown command.";
        }
        
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
        
        _listener.BeginGetContext(OnRequestReceived, null);
    }
}
