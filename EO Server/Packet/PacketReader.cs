using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EO_Server
{

    //TODO: Make sure properly read bytes (buffer is big enough)
    public class PacketReader
    {
        public int packetType;
        public int packetLength;
        public Packet packet;
        public bool constructed;
        public PacketError error;

        public int messageSize;

        private byte[] buffer;
        private int readOffset;

        public PacketReader(byte[] buffer)
        {
            this.buffer = buffer;
            packetType = -1;
            packetLength = -1;
            messageSize = 4;
            error = PacketError.NONE;
        }

        public byte ReadByte()
        {
            return buffer[readOffset++];
        }

        public short ReadInt16()
        {
            short a = BitConverter.ToInt16(buffer, readOffset);
            readOffset += 2;

            return a;
        }

        public int ReadInt32()
        {
            int a = BitConverter.ToInt32(buffer, readOffset);
            readOffset += 4;

            return a;
        }

        public long ReadInt64()
        {
            long a = BitConverter.ToInt64(buffer, readOffset);
            readOffset += 8;

            return a;
        }

        public ushort ReadUInt16()
        {
            ushort a = BitConverter.ToUInt16(buffer, readOffset);
            readOffset += 2;

            return a;
        }

        public uint ReadUInt32()
        {
            uint a = BitConverter.ToUInt32(buffer, readOffset);
            readOffset += 4;

            return a;
        }

        public ulong ReadUInt64()
        {
            ulong a = BitConverter.ToUInt64(buffer, readOffset);
            readOffset += 8;

            return a;
        }

        public string ReadString()
        {
            if ((messageSize - readOffset) >= 4)
            {
                int stringLen = ReadInt32();
                try
                {
                    string s = Encoding.ASCII.GetString(buffer, readOffset, stringLen);
                    readOffset += stringLen;
                    return s;
                }
                catch (Exception e)
                { 
                    Console.Error.WriteLine(e.ToString());
                    return null;
                }
            }

            return null;
        }

        public bool ReadBoolean()
        {
            if ((messageSize - readOffset) >= 1)
            {
                return BitConverter.ToBoolean(buffer, readOffset++);
            }

            return false;
        }

        public void ReadPacketLength()
        {
            packetLength = ReadInt32();
            messageSize += packetLength;
        }


        //TODO: Make sure there's enough bytes to read from
        //size of buffer doesnt start from offset
        public bool ReadPacket()
        {
            //Read an integer for packet type

            if((messageSize - readOffset) < 4)
            {
                error = PacketError.NO_PACKET_TYPE;
                return false;
            }
            else
            {
                int a = ReadInt32();

                //Successful
                if(a >= 0)
                {
                    packetType = a;
                }
            }
            
            switch(packetType)
            {
                //Hello packet!
                case (int) PacketType.HELLO_PACKET:

                    string s = ReadString();
                    if(s != null)
                    {
                        Console.WriteLine("Got message: " + s);
                        packet = new HelloPacket(s);
                    }
                    else
                    {
                        error = PacketError.INVALID_DATA;
                        return false;
                    }

                    break;
                //TODO: INIT GAME PACKET
                case (int) PacketType.REQUEST_ENTER_WORLD:

                    uint char_index = ReadUInt32();

                    packet = new ReqEnterWorld(char_index);
                    break;

                case (int) PacketType.REQUEST_PLAYER_DIR:

                    int direction = ReadInt32();
                    bool walk = ReadBoolean();
                    
                    packet = new RequestPlayerDir(direction, walk);
                    break;

                case (int) PacketType.REQUEST_PLAYER_ATTACK:

                    packet = new RequestPlayerAttack();
                    break;

                case (int)PacketType.REQUEST_ITEM_PICKUP:

                    ulong entityId = ReadUInt64();

                    packet = new RequestItemPickup(entityId);
                    break;

                case (int)PacketType.REQUEST_ITEM_DROP:

                    uint itemId = ReadUInt32();
                    uint quantity = ReadUInt32();
                    int x = ReadInt32();
                    int y = ReadInt32();

                    packet = new RequestItemDrop(itemId, quantity, x, y);
                    break;

                case (int)PacketType.REQUEST_ITEM_MOVE:

                    itemId = ReadUInt32();
                    uint slotX = ReadUInt32();
                    uint slotY = ReadUInt32();

                    packet = new RequestItemMove(itemId, slotX, slotY);
                    break;

                case (int)PacketType.REQUEST_ITEM_EQUIP:
                    {
                        itemId = ReadUInt32();
                        byte slotIndex = ReadByte();
                        bool equip = ReadBoolean();

                        packet = new RequestItemEquip(itemId, slotIndex, equip);
                        break;
                    }
                case (int)PacketType.REQUEST_ITEM_CONSUME:
                    {
                        itemId = ReadUInt32();

                        packet = new RequestItemConsume(itemId);
                        break;
                    }
                case (int) PacketType.SET_NET_TIME:

                    ReadInt64();
                    packet = new SetNetworkTime(0);
                    break;

                case (int)PacketType.LOGIN_AUTH:

                    string username = ReadString();
                    string password = ReadString();

                    packet = new LoginAuth(username, password);
                    break;

                case (int)PacketType.REQUEST_RES:

                    byte resType = ReadByte();

                    packet = new RequestResource(resType);
                    break;



                default:
                    error = PacketError.INVALID_PACKET_TYPE;
                    return false;
            }

            constructed = true;

            return true;
        }

        public bool ReadJSONPacket()
        {
            if ((messageSize - readOffset) < 4)
            {
                error = PacketError.NO_PACKET_TYPE;
                return false;
            }
            else
            {
                int a = ReadInt32();

                //Successful
                if (a >= 0)
                {
                    packetType = a;
                }
                else
                    return false;
            }

            string json = ReadString();
            //Console.WriteLine("Packet: " + json);
            
            switch(packetType)
            {
                case (int)PacketType.HELLO_PACKET:

                    packet = JsonConvert.DeserializeObject<HelloPacket>(json);
                    break;

                case (int)PacketType.ACCOUNT_CREATE:

                    packet = JsonConvert.DeserializeObject<AccountCreate>(json);
                    break;

                case (int)PacketType.CHARACTER_CREATE:

                    packet = JsonConvert.DeserializeObject<CharacterCreate>(json);
                    break;

                case (int)PacketType.REQUEST_ENTER_WORLD:

                    packet = packet = JsonConvert.DeserializeObject<ReqEnterWorld>(json);
                    break;

                case (int)PacketType.REQUEST_PLAYER_DIR:

                    packet = JsonConvert.DeserializeObject<RequestPlayerDir>(json);
                    break;

                case (int)PacketType.REQUEST_PLAYER_ATTACK:

                    packet = JsonConvert.DeserializeObject<RequestPlayerAttack>(json);
                    break;

                case (int)PacketType.REQUEST_ITEM_PICKUP:

                    packet = JsonConvert.DeserializeObject<RequestItemPickup>(json);
                    break;

                case (int)PacketType.REQUEST_ITEM_DROP:

                    packet = JsonConvert.DeserializeObject<RequestItemDrop>(json);
                    break;

                case (int)PacketType.REQUEST_ITEM_MOVE:

                    packet = JsonConvert.DeserializeObject<RequestItemMove>(json);
                    break;

                case (int)PacketType.REQUEST_ITEM_EQUIP:
                    {
                        packet = JsonConvert.DeserializeObject<RequestItemEquip>(json);
                        break;
                    }
                case (int)PacketType.REQUEST_ITEM_CONSUME:
                    {
                        packet = JsonConvert.DeserializeObject<RequestItemConsume>(json);
                        break;
                    }

                case (int)PacketType.REQUEST_CHEST_CLOSE:

                    packet = JsonConvert.DeserializeObject<ReqChestClose>(json);
                    break;

                case (int)PacketType.REQUEST_CHEST_TAKE:

                    packet = JsonConvert.DeserializeObject<ReqChestItemTake>(json);
                    break;

                case (int)PacketType.REQUEST_CHEST_GIVE:

                    packet = JsonConvert.DeserializeObject<ReqChestItemGive>(json);
                    break;

                case (int)PacketType.SET_NET_TIME:

                    ReadInt64();
                    packet = new SetNetworkTime(0);
                    break;

                case (int)PacketType.LOGIN_AUTH:

                    packet = JsonConvert.DeserializeObject<LoginAuth>(json);
                    break;

                case (int)PacketType.REQUEST_RES:

                    packet = JsonConvert.DeserializeObject<RequestResource>(json);
                    break;

            
                default:
                    error = PacketError.INVALID_PACKET_TYPE;
                    return false;
            }
            
            

            return true;

        }

    }


}
