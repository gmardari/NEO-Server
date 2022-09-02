using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EO_Server
{
    public class NetworkClient
    {
        public ulong clientId;
        public Socket socket;
        public const int BufferSize = 512;
        public byte[] receiveBuffer;
        public int receiveOffset;
        public int packetsRead;
        public NetworkSession session;

        //public StateObject state;
        public bool connected;
        public IPEndPoint RemoteEndPoint
        { get { return (IPEndPoint) socket.RemoteEndPoint; } }

        public Character character;
        public CharacterDef[] charDefs;
        public uint charIndex;
        public PlayerInputManager inputManager;

        public const int OutgoingQueueSize = 10;
        public const int IncomingQueueSize = 10;
        public Queue<Packet> incomingPackets;
        public PacketReader reader;
        //public Queue<Packet> outgoingPackets;

        public NetworkClient(ulong _clientId, Socket _socket)
        {
            (clientId, socket) = (_clientId, _socket);
            receiveBuffer = new byte[BufferSize];
            connected = true;
            session = new NetworkSession();
            session.state = NetworkSessionState.PINGING;
            incomingPackets = new Queue<Packet>(IncomingQueueSize);
            reader = new PacketReader(receiveBuffer);
            //outgoingPackets = new Queue<Packet>(OutgoingQueueSize);
        }

        public void OnMapLoaded()
        {
            if(session.state == NetworkSessionState.AUTH)
            {
                session.state = NetworkSessionState.IN_GAME;
            }
        }

        public void SendPacket(Packet packet)
        {
            Server.listener.SendPacket(this, packet);
        }

        public void HandlePacket(Packet packet)
        {
            switch(session.state)
            {
                case NetworkSessionState.PINGING:

                    if (packet is HelloPacket)
                    {
                        Server.listener.SendPacket(this, new HelloPacket("Hello client!"));
                        session.state = NetworkSessionState.ACCEPTED;
                    }
                    break;

                case NetworkSessionState.ACCEPTED:

                    if (packet is LoginAuth)
                    {
                        LoginAuth cp = (LoginAuth)packet;

                        Console.WriteLine($"Receiving login attempt client {clientId}: {cp.username}:{cp.password}");

                        if(DB.Authenticate(cp.username, cp.password))
                        {
                            session.state = NetworkSessionState.AUTH;
                            session.username = cp.username;
                            Server.listener.SendPacket(this, new LoginResponse(200, "OK"));
                        }
                        else
                        {
                            Server.listener.SendPacket(this, new LoginResponse(400, "BAD"));
                        }
                    }
                    else if(packet is AccountCreate)
                    {
                        AccountCreate cp = packet as AccountCreate;
                        Console.WriteLine($"Create account: {cp.username}, {cp.password}");
                        if(DB.Connected)
                        {
                            if (!DB.UsernameExists(cp.username))
                            {
                                bool success = DB.CreateAccount(cp.username, cp.password);
                                byte responseByte = success ? (byte)ACCOUNT_CREATE_RESP.SUCCESS : (byte)ACCOUNT_CREATE_RESP.SERVER_ERROR;

                                AccountCreateResponse resp = new AccountCreateResponse(responseByte);
                                SendPacket(resp);
                            }
                            //Duplicate usernames
                            else
                            {
                                AccountCreateResponse resp = new AccountCreateResponse((byte)ACCOUNT_CREATE_RESP.USER_TAKEN);
                                SendPacket(resp);
                            }
                        }
                        else
                        {
                            AccountCreateResponse resp = new AccountCreateResponse((byte)ACCOUNT_CREATE_RESP.SERVER_ERROR);
                            SendPacket(resp);
                        }
                    }

                    break;

                case NetworkSessionState.AUTH:

                    if (packet is RequestResource)
                    {
                        RequestResource cp = (RequestResource)packet;

                        //Load in Character Select
                        if(cp.resType == (byte) RESOURCE_TYPE.CS_CHARACTERS)
                        {
                            Console.WriteLine("Request for characters");
                            charDefs = DB.GetCharDefs(session.username);
                            charIndex = 0;

                            foreach(var def in charDefs)
                            {
                                if(def != null)
                                {
                                    charIndex++;
                                    Server.listener.SendPacket(this, new SetCharacterDef(def));
                                }
                                    
                            }

                            Server.listener.SendPacket(this, new Ack((byte) RESOURCE_TYPE.CS_CHARACTERS));
                        }
                    }
                    else if(packet is CharacterCreate)
                    {
                        var cp = packet as CharacterCreate;
                        CharacterCreateResponse resp = null;
                        CharacterDef def = null;

                        //Available space?
                        if (charIndex < 3)
                        {
                            //TODO: Confirm fields
                            try
                            {
                                if (!DB.CharacterExists(cp.name))
                                {   
                                    if (DB.CreateCharacter(session.username, (int)charIndex, cp.name, cp.gender, cp.skinColour, 0, cp.hairStyle, cp.hairColour, out def))
                                    {
                                        charDefs[charIndex++] = def;
                                        resp = new CharacterCreateResponse((byte)CHARACTER_CREATE_RESP.SUCCESS);
                                    }
                                    else
                                    {
                                        resp = new CharacterCreateResponse((byte)CHARACTER_CREATE_RESP.SERVER_ERROR);

                                    }
                                }
                                else
                                    resp = new CharacterCreateResponse((byte)CHARACTER_CREATE_RESP.NAME_TAKEN);

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                resp = new CharacterCreateResponse((byte)CHARACTER_CREATE_RESP.SERVER_ERROR);
                            }

                        }
                        else
                            resp = new CharacterCreateResponse((byte)CHARACTER_CREATE_RESP.CHARS_LIMIT_REACHED);

                        if(resp != null)
                            SendPacket(resp);

                        if(def != null)
                        {
                            Console.WriteLine($"Created new character {cp.name} at index {charIndex - 1}");
                            SendPacket(new SetCharacterDef(def));
                        }

                    }
                    else if(packet is ReqEnterWorld)
                    {
                        var cp = (ReqEnterWorld) packet; 

                        
                        //EOMap spawnMap = MapManager.GetMap(EOMap.spawnMapId);
                        var charDef = charDefs[cp.char_index];

                        if(charDef != null)
                        {
                            try
                            {
                                DBCharacter dbChar = DB.GetDBChar(charDef.name);

                                if (dbChar != null)
                                {
                                    EnterWorldResp resp = new EnterWorldResp(true);
                                    SendPacket(resp);

                                    EOMap spawnMap = null;
                                    Vector2 spawnPoint = new Vector2();
                                    uint direction;
                                    ulong health;
                                    ulong mana;

                                    if (dbChar.map == null)
                                    {
                                        Console.WriteLine("Map is null, spawning on spawn map");
                                        spawnMap = MapManager.GetMap(EOMap.spawnMapId);
                                        spawnPoint = EOMap.spawnPoint;
                                        direction = 0;
                                    }
                                    else
                                    {
                                        spawnMap = MapManager.GetMap((uint)dbChar.map.Value);
                                        spawnPoint = new Vector2(dbChar.x.Value, dbChar.y.Value);
                                        direction = (uint)dbChar.direction.Value;
                                    }

                                    if (dbChar.health.HasValue)
                                        health = (ulong) dbChar.health.Value;
                                    else
                                        health = 100;

                                    if (dbChar.mana.HasValue)
                                        mana = (ulong)dbChar.mana.Value;
                                    else
                                        mana = 100;


                                    CharProperties props = new CharProperties
                                    {
                                        health = health,
                                        maxHealth = health,
                                        mana = mana,
                                        maxMana = mana,
                                        energy = 100,
                                        maxEnergy = 100,
                                        level = 0,
                                        exp = 0,
                                        expLevel = 0,
                                        expTNL = 100
                                    };

                                    character = spawnMap.SpawnPlayer(this, charDef, dbChar.inventory, props, spawnPoint, direction);

                                    inputManager = new PlayerInputManager();

                                    Console.WriteLine($"Spawned character {character.def.name} on map {spawnMap.mapId} at loc{spawnPoint}");

                                    //Init player vals to player
                                    InitPlayerVals initPacket = new InitPlayerVals(character.props);
                                    SendPacket(initPacket);

                                    //Reset char defs
                                    for (int i = 0; i < charDefs.Length; i++)
                                        charDefs[i] = null;
                                }
                            }
                            catch (SqliteException e)
                            {
                                Console.Error.WriteLine(e.ToString());
                            }
                        }
                        /*
                        if (spawnMap != null && charDef != null)
                        {
                            EnterWorldResp resp = new EnterWorldResp(true);
                            SendPacket(resp);

                            CharProperties props = new CharProperties
                            {
                                health = 100,
                                maxHealth = 100,
                                mana = 100,
                                maxMana = 100,
                                energy = 100,
                                maxEnergy = 100,
                                level = 0,
                                exp = 0,
                                expLevel = 0,
                                expTNL = 100
                            };

                            character = spawnMap.SpawnPlayer(this, charDef, props, 
                                new Vector2(spawnMap.bounds.minPoint.x, spawnMap.bounds.minPoint.y), 0);

                            inputManager = new PlayerInputManager();

                            Console.WriteLine("Spawned character");

                            //Init player vals to player
                            InitPlayerVals initPacket = new InitPlayerVals(character.props);
                            SendPacket(initPacket);

                            //Reset char defs
                            for (int i = 0; i < charDefs.Length; i++)
                                charDefs[i] = null;

                        }
                        else
                            Console.WriteLine($"Couldn't spawn player character due to spawn map or char def not found");
                        */
                    }

                    break;

                case NetworkSessionState.IN_GAME:

                    //TODO: Check if map is valid
                    if(character != null)
                    {
                        if (packet is RequestPlayerDir)
                        {
                            RequestPlayerDir castedPacket = (RequestPlayerDir)packet;
                            if (castedPacket.direction >= 0)
                                inputManager.SetInput(castedPacket.direction, Server.GetCurrentTime());
                            else
                                inputManager.ClearInput();

                            character.map.OnPlayerRequestDirection(this, castedPacket.direction, castedPacket.walk);

                        }
                        else if(packet is RequestPlayerAttack)
                        {
                            var cp = (RequestPlayerAttack) packet;

                            character.map.OnPlayerRequestAttack(this);
                        }
                        else if (packet is RequestItemPickup)
                        {
                            var cp = (RequestItemPickup) packet;

                            character.map.OnPlayerReqPickupItem(this, cp.entityId);
                        }
                        else if(packet is RequestItemMove)
                        {
                            var cp = packet as RequestItemMove;
                            
                            character.map.OnPlayerReqMoveItem(this, cp);
                        }
                        else if(packet is RequestItemEquip)
                        {
                            var cp = packet as RequestItemEquip;

                            character.map.OnPlayerReqEquip(this, cp);
                        }
                        else if(packet is ReqChestClose)
                        {
                            var cp = packet as ReqChestClose;

                            character.map.OnPlayerReqChestClose(this, cp);
                        }
                        else if (packet is ReqChestItemTake)
                        {
                            var cp = packet as ReqChestItemTake;

                            character.map.OnPlayerReqChestTake(this, cp);
                        }
                        else if(packet is ReqChestItemGive)
                        {
                            var cp = packet as ReqChestItemGive;

                            character.map.OnPlayerReqChestGive(this, cp);
                        }
                        
                    }
                    break;
            }
        }

        public void SaveCharState()
        {
            DB.SaveCharacter(character);
            Console.WriteLine($"Saved character {character.def.name} state");
        }

        public void OnDisconnect()
        {
            if (character != null)
                SaveCharState();

            if (character.map != null)
                character.map.OnLostClient(this);
        }

        public void OnProgExit()
        {
            if(character != null)
                SaveCharState();
        }

    }
}
