using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using WorldFable_Server;

public class Connection : MonoBehaviour
{
    public static Connection Packet;

    private Host _client;
    private Peer _peer;

    private bool _isConnected;

    private const ushort Port = 6005;
    private const string ServerIP = "127.0.0.1";
    private const int MaxChannels = 1;

    public GameObject PlayerPrefab;
    public GameObject AnotherPlayerPrefab;
    private GameObject localPlayer;

    public uint mylocaluid;
    public string myUsername;
    public string currentWorld;

    public Player myPlayer;

    public List<WorldGeneration.WorldItem> meta = null;
    private Dictionary<uint, GameObject> remotePlayers = new Dictionary<uint, GameObject>();
    private Dictionary<uint, Vector3> playerPositions = new Dictionary<uint, Vector3>();

    public InventoryHandler inventoryHandler;

    void Start()
    {
        Application.runInBackground = true;
        Library.Initialize();
        ConnectToServer();
    }
    private void Awake()
    {
        if (Packet != null && Packet != this)
        {
            Destroy(gameObject);
            return;
        }

        Packet = this;
        DontDestroyOnLoad(gameObject);
    }


    private void OnApplicationQuit()
    {
        DisconnectFromServer();
        Library.Deinitialize();
    }

    private void ConnectToServer()
    {
        _client = new Host();
        Address address = new Address
        {
            Port = Port
        };
        address.SetHost(ServerIP);

        _client.Create();

        _peer = _client.Connect(address, MaxChannels);

        Debug.Log("Connecting to server...");
    }
    private void DisconnectFromServer()
    {
        if (_isConnected)
        {
            _peer.Disconnect(0);
            _client.Flush();
            Debug.Log("Disconnected from server.");
        }

        _client.Dispose();
    }
    private void Update()
    {
        if (_client == null) return;

        ENet.Event netEvent;
        while (_client.Service(0, out netEvent) > 0)
        {
            switch (netEvent.Type)
            {
                case ENet.EventType.Connect:
                    _isConnected = true;
                    Debug.Log("Connected to server.");
                    StartCoroutine(SendHeartbeat());
                    break;

                case ENet.EventType.Disconnect:
                    _isConnected = false;
                    Debug.Log("Disconnected from server.");
                    StopCoroutine(SendHeartbeat());
                    break;

                case ENet.EventType.Timeout:
                    _isConnected = false;
                    Debug.Log("Connection timeout.");
                    StopCoroutine(SendHeartbeat());
                    break;

                case ENet.EventType.Receive:
                    HandlePacket(netEvent);
                    netEvent.Packet.Dispose();
                    break;
            }
        }
        InterpolatePlayerPositions();
    }
    private IEnumerator SendHeartbeat()
    {
        while (_isConnected)
        {
            yield return new WaitForSeconds(5);
            byte[] heartbeatPacket = new byte[1];
            heartbeatPacket[0] = (byte)PacketId.Heartbeat;
            SendPacket(heartbeatPacket);
        }
    }
    public void SendPacket(byte[] data)
    {
        if (_isConnected)
        {
            Packet packet = default;
            packet.Create(data);
            _peer.Send(0, ref packet);
        }
    }
    private void HandlePacket(ENet.Event netEvent)
    {
        byte[] data = new byte[netEvent.Packet.Length];
        netEvent.Packet.CopyTo(data);

        using (MemoryStream stream = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            PacketId packetId = (PacketId)reader.ReadByte();

            if (packetId == PacketId.PlayerUID)
            {
                uint playerUID = reader.ReadUInt32();
                mylocaluid = playerUID;
            }
            else if (packetId == PacketId.Register)
            {
                string result = reader.ReadString();
                MainSceneManager.mainManager.SetText(result);
            }
            else if (packetId == PacketId.Login)
            {
                string result = reader.ReadString();
                if (result == "true")
                {
                    myUsername = reader.ReadString();
                    SceneManager.LoadScene("DataScene");
                }
                else
                {
                    MainSceneManager.mainManager.SetText("Wrong Username or Password!");
                }
            }
            else if (packetId == PacketId.RequestPlayerData)
            {
                Player playerData = new Player();

                playerData.PlayerID = reader.ReadInt32();
                playerData.TankIDName = reader.ReadString();
                playerData.TankIDPass = reader.ReadString();
                playerData.DisplayName = reader.ReadString();
                playerData.Country = reader.ReadString();
                playerData.AdminLevel = reader.ReadInt32();
                playerData.CurrentWorld = reader.ReadString();

                playerData.x = reader.ReadSingle();
                playerData.y = reader.ReadSingle();

                playerData.ClothHair = reader.ReadInt32();
                playerData.ClothShirt = reader.ReadInt32();
                playerData.ClothPants = reader.ReadInt32();
                playerData.ClothFeet = reader.ReadInt32();
                playerData.ClothFace = reader.ReadInt32();
                playerData.ClothHand = reader.ReadInt32();
                playerData.ClothBack = reader.ReadInt32();
                playerData.ClothMask = reader.ReadInt32();
                playerData.ClothNecklace = reader.ReadInt32();

                playerData.CanWalkInBlocks = reader.ReadBoolean();
                playerData.CanDoubleJump = reader.ReadBoolean();
                playerData.IsInvisible = reader.ReadBoolean();
                playerData.IsBanned = reader.ReadBoolean();
                playerData.BanTime = reader.ReadInt64();

                playerData.InventoryID = reader.ReadInt32();

                Debug.Log($"Player Data Received: {playerData.DisplayName}, Current World: {playerData.CurrentWorld}");

                myPlayer = playerData;
            }
            else if (packetId == PacketId.Join_World)
            {
                Debug.Log("ASD");
                SceneManager.LoadScene("WorldScene");
                using (MemoryStream streams = new MemoryStream(data))
                {
                    List<WorldGeneration.WorldItem> worldItems = new List<WorldGeneration.WorldItem>();

                    while (stream.Position < stream.Length)
                    {
                        WorldGeneration.WorldItem item = new WorldGeneration.WorldItem();

                        byte[] foregroundBytes = new byte[4];
                        byte[] backgroundBytes = new byte[4];

                        stream.Read(foregroundBytes, 0, 4);
                        stream.Read(backgroundBytes, 0, 4);

                        item.foreground = BitConverter.ToInt32(foregroundBytes, 0);
                        item.background = BitConverter.ToInt32(backgroundBytes, 0);

                        worldItems.Add(item);
                    }

                    meta = worldItems;
                }
            }
            else if (packetId == PacketId.Spawn_LocalPlayer)
            {
                uint playerId = reader.ReadUInt32();
                string worldName = reader.ReadString();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();

                HandlePlayerSpawn(playerId, x, y);
            }

            else if (packetId == PacketId.Spawn_OtherPlayer)
            {
                uint playerId = reader.ReadUInt32();
                string worldName = reader.ReadString();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                
                HandlePlayerSpawn(playerId, x, y);
            }

            else if (packetId == PacketId.PlayerMovement)
            {
                HandlePlayerPositionUpdate(reader);
            }
            else if (packetId == PacketId.GetInventorySize)
            {
                int size = reader.ReadInt32();
                inventoryHandler.SetInventorySlotCount(size);
            }
        }
    }
    private void HandlePlayerPositionUpdate(BinaryReader reader)
    {
        uint playerId = reader.ReadUInt32();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();

        Console.WriteLine($"{playerId} , {x}, {y}");
        if (!remotePlayers.ContainsKey(playerId))
        {
            return;
        }

        playerPositions[playerId] = new Vector3(x, y, 0);
    }
    private void HandlePlayerSpawn(uint playerId, float x, float y)
    {
        if (playerId == mylocaluid)
        {
            Debug.Log("Spawning Local Player");

            if (localPlayer == null)
            {
                localPlayer = Instantiate(PlayerPrefab, new Vector3(x, y, 0), Quaternion.identity);
                localPlayer.name = $"MyPlayer_{playerId}";
                PlayerController controller = localPlayer.GetComponent<PlayerController>();
                controller.playerId = playerId;
            }
        }
        else
        {
            Debug.Log("Spawning Remote Player");
            if (!remotePlayers.ContainsKey(playerId))
            {
                GameObject newPlayer = Instantiate(AnotherPlayerPrefab, new Vector3(x, y, 0), Quaternion.identity);
                newPlayer.name = $"Player_{playerId}";
                remotePlayers[playerId] = newPlayer;
            }
        }
    }

    public void SendPlayerMovement(Vector3 position)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketId.PlayerMovement);
            writer.Write(mylocaluid);
            writer.Write(currentWorld);
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(5f);

            SendPacket(stream.ToArray());
        }
    }
    public void RequestPlayerSpawn()
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketId.Spawn_LocalPlayer);
            writer.Write(mylocaluid);
            SendPacket(stream.ToArray());
        }
    }
    private void InterpolatePlayerPositions()
    {
        foreach (var player in remotePlayers)
        {
            uint playerId = player.Key;

            if (playerPositions.ContainsKey(playerId))
            {
                player.Value.transform.position = Vector3.Lerp(player.Value.transform.position, playerPositions[playerId], Time.deltaTime * 10f);
            }
        }
    }
    public void SendRegistrationPacket(string username, string password)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketId.Register);

            writer.Write(username);
            writer.Write(password);

            byte[] data = stream.ToArray();
            SendPacket(data);
        }
    }
    public void SendLoginPacket(string username, string password)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketId.Login);

            writer.Write(username);
            writer.Write(password);

            byte[] data = stream.ToArray();
            SendPacket(data);
        }
    }
    public void RequestPlayerData()
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketId.RequestPlayerData);

            writer.Write(myUsername);

            byte[] data = stream.ToArray();
            SendPacket(data);
        }
    }
    public void RequestJoinWorld(string worldName)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write((byte)PacketId.Join_World);

            writer.Write(worldName);

            byte[] data = stream.ToArray();
            SendPacket(data);
        }
    }
    public enum PacketId : byte
    {
        Heartbeat = 1,
        PlayerUID = 2,
        Register = 3,
        Login = 4,
        RequestPlayerData = 5,
        Join_World = 6,
        Spawn_LocalPlayer = 7,
        PlayerMovement = 8,
        Spawn_OtherPlayer = 9,
        GetInventorySize = 10
    }
}
