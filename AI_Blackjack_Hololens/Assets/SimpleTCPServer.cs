using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class SimpleTCPServer : MonoBehaviour
{
    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;
    private bool isRunning = false;

    // Port to listen on
    public int port = 8052;

    // Reference to the TextMeshPro object in the scene
    public TextMeshProUGUI displayText;

    // Start is called before the first frame update
    void Start()
    {
        // Start TCP listener thread
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncomingConnections));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (connectedTcpClient != null && connectedTcpClient.Available > 0)
        {
            // Get the network stream
            NetworkStream stream = connectedTcpClient.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                // Convert bytes to string
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Update TextMeshPro object instead of logging
                UpdateDisplayText(message);
            }
        }
    }

    // Start listening for incoming client connections
    private void ListenForIncomingConnections()
    {
        try
        {
            // Create a TCP listener
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            isRunning = true;

            while (isRunning)
            {
                // Check for a new client connection
                connectedTcpClient = tcpListener.AcceptTcpClient();
                UpdateDisplayText("Client connected.");
            }
        }
        catch (SocketException socketException)
        {
            UpdateDisplayText("SocketException: " + socketException.ToString());
        }
    }

    // Method to update the TextMeshPro object
    private void UpdateDisplayText(string message)
    {
        if (displayText != null)
        {
            displayText.text = message;
        }
    }

    // On application quit, close all connections
    private void OnApplicationQuit()
    {
        isRunning = false;
        tcpListener.Stop();
        tcpListenerThread.Abort();
    }
}
