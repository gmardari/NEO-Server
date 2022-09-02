using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EO_Server
{
    //TODO: Don't constantly re-allocate memory!
    //TODO: Send in Network Byte Order and put back to Host Byte Order
    //TODO: When writing strings, don't go over BufferSize
    public class PacketWriter
    {
        public byte[] buffer;
        public int offset;
        public const int BufferSize = 512;

        public PacketWriter()
        {
            buffer = new byte[BufferSize];
            offset = 0;
        }

        public bool CanWrite(int size)
        {
            if ((BufferSize - offset) >= size)
                return true;

            return false;
        }

        public void ApplyLengthPadding()
        {
            offset += 4;
        }

        public void ApplyPadding(int byteLength)
        {
            offset += byteLength;
        }

        //Used when Length padding applied
        public void WritePacketLength()
        {
            uint packetLength = (uint)(offset - 4);
            Array.Copy(BitConverter.GetBytes(packetLength), 0, buffer, 0, 4);
        }

        public void WriteByte(byte a)
        {
            if(CanWrite(1))
            {
                buffer[offset++] = a;
            }
        }

        public void WriteInt16(short a)
        {
            if (CanWrite(2))
            {
                Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 2);
                offset += 2;
            }
        }

        public void WriteInt32(int a)
        {
            if (CanWrite(4))
            {
                Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 4);
                offset += 4;
            }
        }

        public void WriteInt64(long a)
        {
            if (CanWrite(8))
            {
                Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 8);
                offset += 8;
            }
        }
        
        public void WriteUInt16(ushort a)
        {
            if (CanWrite(2))
            {
                Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 2);
                offset += 2;
            }
        }

        public void WriteUInt32(uint a)
        {
            if (CanWrite(4))
            {
                Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 4);
                offset += 4;
            }
        }

        public void WriteUInt64(ulong a)
        {
            if (CanWrite(8))
            {
                Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 8);
                offset += 8;
            }
        }

        public void WriteString(string s)
        {
            byte[] stringData = Encoding.ASCII.GetBytes(s);
            int len = 4 + s.Length;

            if (CanWrite(len))
            {
                Array.Copy(BitConverter.GetBytes(stringData.Length), 0, buffer, offset, 4);
                Array.Copy(stringData, 0, buffer, offset + 4, stringData.Length);
                offset += len;
            }
        }

        public void WriteBoolean(bool b)
        {
            if (CanWrite(1))
            {
                buffer[offset++] = BitConverter.GetBytes(b)[0];
            }

        }
        /*
        public void WritePacket(Packet packet)
        {
            if(typeof(HelloPacket).IsInstanceOfType(packet))
            {
                HelloPacket helloPacket = (HelloPacket) packet;
                ApplyLengthPadding();

                WriteInt32((int) PacketType.HELLO_PACKET);
                WriteString(helloPacket.msg);

                WritePacketLength();
            }
            else if(typeof(SetMapPacket).IsInstanceOfType(packet))
            {
                SetMapPacket mapPacket = (SetMapPacket) packet;
                ApplyLengthPadding();

                WriteInt32((int) PacketType.SET_MAP);
                WriteUInt32(mapPacket.mapId);
                WriteUInt64(mapPacket.myCharacterId);

                WritePacketLength();
            }
            else if(typeof(SetEntityDef).IsInstanceOfType(packet))
            {
                SetEntityDef castedPacket = (SetEntityDef) packet;
                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_ENTITY_DEF);
                WriteUInt64(castedPacket.entityId); //ulong
                WriteUInt32(castedPacket.entityType); //uint
                WriteInt32(castedPacket.posX);
                WriteInt32(castedPacket.posY);

                WritePacketLength();
            }
            else if (packet is SetEntityPos)
            {
                SetEntityPos castedPacket = (SetEntityPos) packet;
                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_ENTITY_POS);
                WriteUInt64(castedPacket.entityId);
                WriteInt32(castedPacket.posX);
                WriteInt32(castedPacket.posY);

                WritePacketLength();
            }
            else if (packet is SetEntityDir)
            {
                SetEntityDir castedPacket = (SetEntityDir) packet;
                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_ENTITY_DIR);
                WriteUInt64(castedPacket.entityId);
                WriteUInt32(castedPacket.direction);

                WritePacketLength();
            }
            else if (packet is SetEntityWalk)
            {
                SetEntityWalk castedPacket = (SetEntityWalk) packet;
                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_ENTITY_WALK);
                WriteUInt64(castedPacket.entityId);
                WriteInt32(castedPacket.fromX);
                WriteInt32(castedPacket.fromY);
                WriteUInt32(castedPacket.direction);
                WriteByte(castedPacket.speed);
                WriteInt64(castedPacket.timeStarted);

                WritePacketLength();
            }
            else if (packet is SetEntityAttack)
            {
                SetEntityAttack cp = (SetEntityAttack) packet;
                ApplyLengthPadding();

                WriteInt32((int) PacketType.SET_ENTITY_ATTACK);
                WriteUInt64(cp.entityId);
                WriteInt64(cp.timeStarted);

                WritePacketLength();
            }
            else if (packet is SetEntityProp)
            {
                SetEntityProp cp = (SetEntityProp) packet;
                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_ENTITY_PROP);
                WriteUInt64(cp.entityId);
                WriteUInt32(cp.propType);

                switch (cp.propType)
                {
                    case (uint) EntityProperty.HEALTH:
                        //Using Convert.ToUInt64 in case we accidentaly provided a long instead of ulong
                        WriteUInt64(Convert.ToUInt64(cp.propValue));
                        break;

                    case (uint)EntityProperty.NAME:
                        WriteString((String) cp.propValue);
                        break;

                }

                WritePacketLength();
            }
            else if (packet is SetEntityHealth)
            {
                SetEntityHealth cp = (SetEntityHealth) packet;
                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_ENTITY_HEALTH);
                WriteUInt64(cp.entityId);
                WriteUInt64(cp.health);
                WriteUInt64(cp.maxHealth);
                WriteInt64(cp.deltaHp);

                WritePacketLength();
            }
            else if (typeof(RemoveEntity).IsInstanceOfType(packet))
            {
                RemoveEntity castedPacket = (RemoveEntity) packet;
                ApplyLengthPadding();

                WriteInt32((int)PacketType.REMOVE_ENTITY);
                WriteUInt64(castedPacket.entityId);

                WritePacketLength();
            }
            else if (typeof(Ack).IsInstanceOfType(packet))
            {
                Ack castedPacket = (Ack)packet;
                ApplyLengthPadding();

                WriteInt32((int) PacketType.ACK);
                WriteByte(castedPacket.resType);

                WritePacketLength();
            }
            else if (typeof(SetNetworkTime).IsInstanceOfType(packet))
            {
                SetNetworkTime cp = (SetNetworkTime) packet;

                ApplyLengthPadding();
                WriteInt32((int)PacketType.SET_NET_TIME);
                WriteInt64(cp.netTime);
                WritePacketLength();
            }
            else if(typeof(LoginResponse).IsInstanceOfType(packet))
            {
                LoginResponse cp = (LoginResponse) packet;

                ApplyLengthPadding();
                WriteInt32((int)PacketType.LOGIN_RESPONSE);
                WriteUInt32(cp.response);
                WriteString(cp.responseMsg);
                WritePacketLength();
            }
            else if(packet is SetCharacterDef)
            {
                SetCharacterDef cp = (SetCharacterDef)packet;

                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_CHARACTER_DEF);
                WriteUInt64(cp.entityId);
                WriteInt32(cp.x);
                WriteInt32(cp.y);
                WriteUInt32(cp.direction);
                WriteUInt64(cp.health);
                WriteUInt64(cp.maxHealth);
                WriteString(cp.name);
                WriteUInt16(cp.gender);
                WriteUInt16(cp.race);
                WriteUInt16(cp.hairStyle);
                WriteUInt32(cp.armor);
                WriteUInt32(cp.hat);
                WriteUInt32(cp.shoes);
                WriteUInt32(cp.back);
                WriteUInt32(cp.weapon);

                WritePacketLength();
            }
            else if(typeof(SetNpcDef).IsInstanceOfType(packet))
            {
                SetNpcDef cp = (SetNpcDef)packet;

                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_NPC_DEF);
                WriteUInt64(cp.entityId);
                WriteInt32(cp.x);
                WriteInt32(cp.y);
                WriteUInt32(cp.direction);
                WriteUInt64(cp.health);
                WriteUInt64(cp.maxHealth);
                WriteUInt32(cp.npcId);

                WritePacketLength();
            }
            else if (packet is SetItemDef)
            { 
                SetItemDef cp = (SetItemDef) packet;

                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_ITEM_DEF);
                WriteUInt64(cp.entityId);
                WriteInt32(cp.x);
                WriteInt32(cp.y);
                WriteUInt32(cp.itemId);
                WriteUInt32(cp.quantity);

                WritePacketLength();
            }
            else if (packet is SetPlayerInvItem)
            {
                var cp = packet as SetPlayerInvItem;

                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_PLAYER_INV_ITEM);
                WriteUInt32(cp.itemId);
                WriteUInt32(cp.quantity);
                WriteUInt32(cp.x);
                WriteUInt32(cp.y);

                WritePacketLength();
            }
            else if(packet is SetPaperdollSlot)
            {
                var cp = packet as SetPaperdollSlot;

                ApplyLengthPadding();

                WriteInt32((int)PacketType.SET_PAPERDOLL_SLOT);
                WriteUInt64(cp.entityId);
                WriteByte(cp.slotIndex);
                WriteUInt32(cp.itemId);
                WriteBoolean(cp.equipped);

                WritePacketLength();
            }
            else if(packet is InitPlayerVals)
            {
                var cp = packet as InitPlayerVals;

                ApplyLengthPadding();

                WriteInt32((int)PacketType.INIT_PLAYER_VALS);
                WriteUInt64(cp.health);
                WriteUInt64(cp.maxHealth);
                WriteUInt64(cp.mana);
                WriteUInt64(cp.maxMana);
                WriteUInt64(cp.energy);
                WriteUInt64(cp.maxEnergy);

                WriteUInt32(cp.level);
                WriteUInt64(cp.exp);
                WriteUInt64(cp.expLevel);
                WriteUInt64(cp.expTNL);

                WritePacketLength();
            }
        }
        */
        public void WriteJSONPacket(Packet packet)
        {
            ApplyLengthPadding();

            string json = JsonConvert.SerializeObject(packet);
            WriteInt32(packet.packetType);
            WriteString(json);
            WritePacketLength();
        }
    }
}
