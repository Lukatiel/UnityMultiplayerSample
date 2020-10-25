using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using NetworkMessages;
using NetworkObjects;
using System;
using System.Text;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine.Assertions.Must;

public class NetworkClient : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIP;
    public ushort serverPort;

    //Testing
    public NetworkObjects.NetworkPlayer networkPlayer;
    public string myAddress;
    public GameObject playerPrefab;
    //public GameObject playerGO;
    public List<string> newPlayers, droppedPlayers;
    public Dictionary<string, GameObject> currentPlayers;
    public ListOfPlayers initialSetofPlayers;
    
    void Start ()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.Parse(serverIP,serverPort);
        m_Connection = m_Driver.Connect(endpoint);


    }
    
    void SendToServer(string message){  //WHAT TO USE THE SEND THE CLIENT UPDATE TO THE SERVER
        var writer = m_Driver.BeginSend(m_Connection);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message),Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    void OnConnect(){
        Debug.Log("We are now connected to the server");

        newPlayers = new List<string>();
        initialSetofPlayers = new ListOfPlayers();
        networkPlayer = new NetworkObjects.NetworkPlayer();

        SpawnPlayer(networkPlayer.id, networkPlayer.cubPos, networkPlayer.rotation);
        


        //// Example to send a handshake message:
        //HandshakeMsg m = new HandshakeMsg();
        //m.player.id = m_Connection.InternalId.ToString();
        //SendToServer(JsonUtility.ToJson(m));
        //Testing Player update
        //PlayerUpdateMsg m = new PlayerUpdateMsg();
        //m.player.id = m_Connection.InternalId.ToString();
        //SendToServer(JsonUtility.ToJson(m));


    }
    
    public void SpawnPlayer(string id, Vector3 position, Quaternion rotation)
    {
        
        Debug.Log("Spawn in player" + newPlayers.Count);
        if (newPlayers.Count > 0)
        {
            Debug.Log("There are more than 0 players" + newPlayers.Count);
            foreach (string playerID in newPlayers)
            {
                Debug.Log("trying to instantiate a player with id" + playerID);
                currentPlayers.Add(playerID, Instantiate(playerPrefab, networkPlayer.cubPos, Quaternion.identity));
                currentPlayers[playerID].name = playerID;
            }
            newPlayers.Clear();
        }

        if (initialSetofPlayers.players.Length > 0)
        {
            Debug.Log(initialSetofPlayers);
            foreach (NetworkObjects.NetworkPlayer player in initialSetofPlayers.players)
            {
                if (player.id == myAddress)
                    continue;
                currentPlayers.Add(player.id, Instantiate(playerPrefab, player.cubPos, Quaternion.identity));
                currentPlayers[player.id].GetComponent<Renderer>().material.color = player.cubeColor;
                currentPlayers[player.id].name = player.id;
                Debug.Log("should have spawned other players");

            }

            initialSetofPlayers.players = new NetworkObjects.NetworkPlayer[0];

        }
    }

    void OnData(DataStreamReader stream){
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length,Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        NetworkHeader header = JsonUtility.FromJson<NetworkHeader>(recMsg);
        SpawnPlayer(networkPlayer.id, networkPlayer.cubPos, networkPlayer.rotation);
        switch (header.cmd){
            case Commands.HANDSHAKE:
            HandshakeMsg hsMsg = JsonUtility.FromJson<HandshakeMsg>(recMsg);
            Debug.Log("Handshake message received!");
            break;
            case Commands.PLAYER_UPDATE:
            PlayerUpdateMsg puMsg = JsonUtility.FromJson<PlayerUpdateMsg>(recMsg);
            Debug.Log("Player update message received!");
                Debug.Log(puMsg.player.cubPos);
                Debug.Log(networkPlayer.cubPos);
            break;
            case Commands.SERVER_UPDATE:
            ServerUpdateMsg suMsg = JsonUtility.FromJson<ServerUpdateMsg>(recMsg);
            Debug.Log("Server update message received!");
            break;
            default:
            Debug.Log("Unrecognized message received!");
            break;
        }
    }

    void Disconnect(){
        m_Connection.Disconnect(m_Driver);
        m_Connection = default(NetworkConnection);
    }

    void OnDisconnect(){
        Debug.Log("Client got disconnected from server");
        //Destroy current player game object
        //remove the players id
        m_Connection = default(NetworkConnection);
    }

    public void OnDestroy()
        {
            m_Driver.Dispose();
        }
   

    public  class ListOfPlayers
    {
        public NetworkObjects.NetworkPlayer[] players;

        public ListOfPlayers()
        {
            players = new NetworkObjects.NetworkPlayer[0];
        }
    }


    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }
        //Sending players position to the server
        PlayerUpdateMsg m = new PlayerUpdateMsg();
        m.player.id = m_Connection.InternalId.ToString();
        SendToServer(JsonUtility.ToJson(m));

        //Player Movement here???


        DataStreamReader stream;
        NetworkEvent.Type cmd;
        cmd = m_Connection.PopEvent(m_Driver, out stream);
        while (cmd != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                OnConnect();
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnData(stream);

            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                OnDisconnect();
            }

            cmd = m_Connection.PopEvent(m_Driver, out stream);
        }
    }
}