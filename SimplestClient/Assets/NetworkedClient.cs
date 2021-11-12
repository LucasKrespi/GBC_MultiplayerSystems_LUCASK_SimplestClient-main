using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{

    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    byte error;
    bool isConnected = false;
    int ourClientID;

    GameObject gameSystemManager;

    public bool isInputEnable;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjecs = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjecs)
        {
            switch (go.name)
            {
                case "GM":
                    gameSystemManager = go;
                    break;
            }
        }

        Connect();

    }

    // Update is called once per frame
    void Update()
    {
        UpdateNetworkConnection();

    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }
    
    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "192.168.2.58", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }
    
    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }
    
    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

        string[] csv = msg.Split(',');

        int signifier = int.Parse(csv[0]);

        if (signifier == (int)GameSystemManager.ServerToClientSignifiers.LOGIN_SUCCESS)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameSystemManager.GameStates.MAIN_MENU);
        }
        else if(signifier == (int)GameSystemManager.ServerToClientSignifiers.GAME_SESSION_STARTED)
        {
            if(int.Parse(csv[1]) == (int)GameSystemManager.ServerToClientSignifiers.FIRST_PLAYER)
            {
                isInputEnable = true;
            }

            gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameSystemManager.GameStates.PLAYING_TIC_TAC_TOE);

        }
        else if (signifier == (int)GameSystemManager.ServerToClientSignifiers.OPPONENT_PLAY)
        {
            foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
            {
                if (int.Parse(csv[1]) == sp.m_iterator)
                {
                    sp.isOccupied = true;
                    sp.m_Button.GetComponentInChildren<Text>().text = "O";
                }

                isInputEnable = true;
            }
        }
        else if (signifier == (int)GameSystemManager.ServerToClientSignifiers.CHAT_MSG)
        {
            csv[0] = "";
            gameSystemManager.GetComponent<GameSystemManager>().chatSendMessage(string.Join(" ", csv), Color.black);
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }

   
}
