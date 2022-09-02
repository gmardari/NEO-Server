using Newtonsoft.Json;
using EO_Server;

public enum PacketError
{
    NONE,
    NO_PACKET_TYPE,
    INVALID_PACKET_TYPE,
    INVALID_DATA
}

public enum PacketType : int
{
    NONE,
    HELLO_PACKET,
    LOGIN_AUTH,
    LOGIN_RESPONSE,
    ACCOUNT_CREATE,
    ACCOUNT_CREATE_RESPONSE,
    CHARACTER_CREATE,
    CHARACTER_CREATE_RESPONSE,
    REQUEST_ENTER_WORLD,
    ENTER_WORLD_RESPONSE,
    SET_MAP,
    SET_ENTITY_DEF,
    SET_ENTITY_POS,
    SET_ENTITY_DIR,
    SET_ENTITY_WALK,
    SET_ENTITY_ATTACK,
    SET_ENTITY_PROP,
    SET_ENTITY_HEALTH,
    REQUEST_PLAYER_DIR,
    REQUEST_PLAYER_ATTACK,
    REQUEST_ITEM_PICKUP,
    REQUEST_ITEM_DROP,
    REQUEST_ITEM_MOVE,
    REQUEST_ITEM_EQUIP,
    REQUEST_ITEM_CONSUME,
    REQUEST_ENTITY_INTERACT,
    REQUEST_CHEST_CLOSE,
    REQUEST_CHEST_TAKE,
    REQUEST_CHEST_GIVE,
    REMOVE_ENTITY,
    ACK,
    SET_NET_TIME,
    REQUEST_RES,
    SET_CHARACTER_DEF,
    SET_NPC_DEF,
    SET_ITEM_DEF,
    SET_PLAYER_INV_ITEM,
    SET_CHEST_INV_ITEM,
    SET_PAPERDOLL_SLOT,
    INIT_PLAYER_VALS,
    CHEST_OPEN
}

public enum RESOURCE_TYPE : byte
{
    MAP,
    CS_CHARACTERS,
    CHEST_CONTENTS
}

public class Packet
{
    [JsonIgnore]
    public int packetType;
}

public class HelloPacket : Packet
{
    public string msg;

    public HelloPacket(string msg)
    {
        this.packetType = (int)PacketType.HELLO_PACKET;
        this.msg = msg;
    }

    public override string ToString()
    {
        return $"HelloPacket [msg:\"{msg}\"]";
    }
}


public class LoginAuth : Packet
{
    public string username;
    public string password;

    public LoginAuth(string _username, string _password)
    {
        this.packetType = (int)PacketType.LOGIN_AUTH;
        (username, password) = (_username, _password);
    }
}

public class LoginResponse : Packet
{
    public uint response;
    public string responseMsg;

    public LoginResponse(uint response, string responseMsg)
    {
        this.packetType = (int)PacketType.LOGIN_RESPONSE;
        this.response = response;
        this.responseMsg = responseMsg;
    }
}

public class AccountCreate : Packet
{
    public string username;
    public string password;
    public string realName;
    public string loc;
    public string email;

    public AccountCreate(string username, string password, string realName, string loc, string email)
    {
        this.packetType = (int)PacketType.ACCOUNT_CREATE;
        this.username = username;
        this.password = password;
        this.realName = realName;
        this.loc = loc;
        this.email = email;
    }
}

public class AccountCreateResponse : Packet
{
    public byte response;

    public AccountCreateResponse(byte response)
    {
        this.packetType = (int)PacketType.ACCOUNT_CREATE_RESPONSE;
        this.response = response;
    }
}

public class CharacterCreate : Packet
{
    public string name;
    public byte gender;
    public byte hairStyle;
    public byte hairColour;
    public byte skinColour;

    public CharacterCreate(string name, byte gender, byte hairStyle, byte hairColour, byte skinColour)
    {
        this.packetType = (int)PacketType.CHARACTER_CREATE;

        this.name = name;
        this.gender = gender;
        this.hairStyle = hairStyle;
        this.hairColour = hairColour;
        this.skinColour = skinColour;
    }
}

public class CharacterCreateResponse : Packet
{
    public byte response;

    public CharacterCreateResponse(byte response)
    {
        this.packetType = (int)PacketType.CHARACTER_CREATE_RESPONSE;
        this.response = response;
    }
}

public class ReqEnterWorld : Packet
{
    public uint char_index;

    public ReqEnterWorld(uint char_index)
    {
        this.packetType = (int)PacketType.REQUEST_ENTER_WORLD;
        this.char_index = char_index;
    }
}

public class EnterWorldResp : Packet
{
    public bool accepted;

    public EnterWorldResp(bool accepted)
    {
        this.packetType = (int)PacketType.ENTER_WORLD_RESPONSE;
        this.accepted = accepted;
    }
}

public class SetMapPacket : Packet
{
    public uint mapId;
    public ulong myCharacterId;

    public SetMapPacket(uint _mapId, ulong _myCharacterId)
    {
        this.packetType = (int)PacketType.SET_MAP;
        (mapId, myCharacterId) = (_mapId, _myCharacterId);
    }
}

public class SetEntityDef : Packet
{
    public ulong entityId;
    public uint entityType;
    public int posX;
    public int posY;

    public SetEntityDef(ulong entityId, uint entityType, int posX, int posY)
    {
        this.packetType = (int)PacketType.SET_ENTITY_DEF;
        this.entityId = entityId;
        this.entityType = entityType;
        this.posX = posX;
        this.posY = posY;
    }

    public override string ToString()
    {
        return $"Id:{entityId}, type:{entityType}, pos:({posX},{posY})";
    }
}

public class SetEntityPos : Packet
{
    public ulong entityId;
    public int posX;
    public int posY;

    public SetEntityPos(ulong entityId, int posX, int posY)
    {
        this.packetType = (int)PacketType.SET_ENTITY_POS;
        this.entityId = entityId;
        this.posX = posX;
        this.posY = posY;
    }
}

public class SetEntityDir : Packet
{
    public ulong entityId;
    public uint direction;

    public SetEntityDir(ulong entityId, uint direction)
    {
        this.packetType = (int)PacketType.SET_ENTITY_DIR;
        this.entityId = entityId;
        this.direction = direction;
    }
}

public class SetEntityWalk : Packet
{
    public ulong entityId;
    public int fromX;
    public int fromY;
    public uint direction;
    public long timeStarted;
    public byte speed;

    public SetEntityWalk(ulong entityId, int fromX, int fromY, uint direction, byte speed, long timeStarted)
    {
        this.packetType = (int)PacketType.SET_ENTITY_WALK;
        this.entityId = entityId;
        this.fromX = fromX;
        this.fromY = fromY;
        this.direction = direction;
        this.speed = speed;
        this.timeStarted = timeStarted;
    }
}

//Sets the attack state of an NPC/Mob/Player
public class SetEntityAttack : Packet
{
    public ulong entityId;
    public long timeStarted;

    public SetEntityAttack(ulong entityId, long timeStarted)
    {
        this.packetType = (int)PacketType.SET_ENTITY_ATTACK;
        this.entityId = entityId;
        this.timeStarted = timeStarted;
    }
}

//Sets a property of an entity
public class SetEntityProp : Packet
{
    public ulong entityId;
    public uint propType;
    public object propValue;

    public SetEntityProp(ulong entityId, uint propType, object propValue)
    {
        this.packetType = (int)PacketType.SET_ENTITY_PROP;
        this.entityId = entityId;
        this.propType = propType;
        this.propValue = propValue;
    }
}

public class SetEntityHealth : Packet
{
    public ulong entityId;
    public ulong health;
    public ulong maxHealth;
    public long deltaHp;

    public SetEntityHealth(ulong entityId, ulong health, ulong maxHealth, long deltaHp)
    {
        this.packetType = (int)PacketType.SET_ENTITY_HEALTH;
        this.entityId = entityId;
        this.health = health;
        this.maxHealth = maxHealth;
        this.deltaHp = deltaHp;
    }
}

public class RemoveEntity : Packet
{
    public ulong entityId;


    public RemoveEntity(ulong entityId)
    {
        this.packetType = (int)PacketType.REMOVE_ENTITY;
        this.entityId = entityId;
    }
}

public class RequestPlayerDir : Packet
{
    public int direction;
    public bool walk;

    public RequestPlayerDir(int direction, bool walk)
    {
        this.packetType = (int)PacketType.REQUEST_PLAYER_DIR;
        this.direction = direction;
        this.walk = walk;
    }
}

public class RequestPlayerAttack : Packet
{
    public RequestPlayerAttack()
    {
        this.packetType = (int)PacketType.REQUEST_PLAYER_ATTACK;
    }
}


public class RequestItemPickup : Packet
{
    public ulong entityId;

    public RequestItemPickup(ulong entityId)
    {
        this.packetType = (int)PacketType.REQUEST_ITEM_PICKUP;
        this.entityId = entityId;
    }
}

public class RequestItemDrop : Packet
{
    public uint itemId;
    public uint quantity;
    public int x;
    public int y;

    public RequestItemDrop(uint itemId, uint quantity, int x, int y)
    {
        this.packetType = (int)PacketType.REQUEST_ITEM_DROP;
        this.itemId = itemId;
        this.quantity = quantity;
        this.x = x;
        this.y = y;
    }
}

public class RequestItemMove : Packet
{
    public uint itemId;
    public uint x;
    public uint y;

    public RequestItemMove(uint itemId, uint x, uint y)
    {
        this.packetType = (int)PacketType.REQUEST_ITEM_MOVE;
        this.itemId = itemId;
        this.x = x;
        this.y = y;
    }
}

public class RequestItemEquip : Packet
{
    public uint itemId;
    public byte slotIndex;
    public bool equip;

    public RequestItemEquip(uint itemId, byte slotIndex, bool equip)
    {
        this.packetType = (int)PacketType.REQUEST_ITEM_EQUIP;
        this.itemId = itemId;
        this.slotIndex = slotIndex;
        this.equip = equip;
    }
}

public class RequestItemConsume : Packet
{
    public uint itemId;

    public RequestItemConsume(uint itemId)
    {
        this.packetType = (int)PacketType.REQUEST_ITEM_CONSUME;
        this.itemId = itemId;
    }
}

public class RequestEntityInteract : Packet
{
    public ulong entityId;
    public byte interactId;

    public RequestEntityInteract(ulong entityId, byte interactId)
    {
        this.packetType = (int)PacketType.REQUEST_ENTITY_INTERACT;
        this.entityId = entityId;
        this.interactId = interactId;
    }
}

public class ReqChestClose : Packet
{
    public ReqChestClose()
    {
        this.packetType = (int)PacketType.REQUEST_CHEST_CLOSE;
    }
}

public class ReqChestItemTake : Packet
{
    public uint slotIndex;
    public uint quantity;

    public ReqChestItemTake(uint slotIndex, uint quantity)
    {
        this.packetType = (int)PacketType.REQUEST_CHEST_TAKE;
        this.slotIndex = slotIndex;
        this.quantity = quantity;
    }
}

public class ReqChestItemGive : Packet
{
    public uint itemId;
    public uint quantity;

    public ReqChestItemGive(uint itemId, uint quantity)
    {
        this.packetType = (int)PacketType.REQUEST_CHEST_GIVE;
        this.itemId = itemId;
        this.quantity = quantity;
    }
}



public class Ack : Packet
{
    public byte resType;

    public Ack(byte _type)
    {
        this.packetType = (int)PacketType.ACK;
        resType = _type;
    }
}

public class SetNetworkTime : Packet
{
    public long netTime;

    public SetNetworkTime(long _netTime)
    {
        this.packetType = (int)PacketType.SET_NET_TIME;
        netTime = _netTime;
    }
}

public class RequestResource : Packet
{
    public byte resType;

    public RequestResource(byte _resType)
    {
        this.packetType = (int)PacketType.REQUEST_RES;
        resType = _resType;
    }
}

public class SetCharacterDef : Packet
{
    public ulong entityId;
    public int x;
    public int y;
    public uint direction;

    public ulong health;
    public ulong maxHealth;

    public CharacterDef def;

    [JsonConstructor]
    public SetCharacterDef(ulong entityId, int x, int y, uint direction, ulong health, ulong maxHealth, CharacterDef def)
    {
        this.packetType = (int)PacketType.SET_CHARACTER_DEF;
        (this.entityId, this.x, this.y, this.direction, this.health, this.maxHealth) = (entityId, x, y, direction, health, maxHealth);
        this.def = def;

    }

    //For CharacterSelect 
    public SetCharacterDef(CharacterDef def)
    {
        this.packetType = (int)PacketType.SET_CHARACTER_DEF;
        this.def = def;

    }

}

public class SetNpcDef : Packet
{
    public ulong entityId;
    public int x;
    public int y;
    public uint direction;
    public ulong health;
    public ulong maxHealth;
    public uint npcId;

    public SetNpcDef(ulong entityId, int x, int y, uint direction, ulong health, ulong maxHealth, uint npcId)
    {
        this.packetType = (int)PacketType.SET_NPC_DEF;
        this.entityId = entityId;
        this.x = x;
        this.y = y;
        this.direction = direction;
        this.health = health;
        this.maxHealth = maxHealth;
        this.npcId = npcId;
    }
}

public class SetItemDef : Packet
{
    public ulong entityId;
    public int x;
    public int y;
    public uint itemId;
    public uint quantity;

    public SetItemDef(ulong entityId, int x, int y, uint itemId, uint quantity)
    {
        this.packetType = (int)PacketType.SET_ITEM_DEF;
        this.entityId = entityId;
        this.x = x;
        this.y = y;
        this.itemId = itemId;
        this.quantity = quantity;
    }
}

public class SetPlayerInvItem : Packet
{
    public uint itemId;
    public uint quantity;
    public uint x;
    public uint y;

    public SetPlayerInvItem(uint itemId, uint quantity, uint x, uint y)
    {
        this.packetType = (int)PacketType.SET_PLAYER_INV_ITEM;
        this.itemId = itemId;
        this.quantity = quantity;
        this.x = x;
        this.y = y;
    }
}

public class SetChestInvItem : Packet
{
    public uint itemId;
    public uint quantity;
    public uint slotIndex;

    public SetChestInvItem(uint itemId, uint quantity, uint slotIndex)
    {
        this.packetType = (int)PacketType.SET_CHEST_INV_ITEM;
        this.itemId = itemId;
        this.quantity = quantity;
        this.slotIndex = slotIndex;
    }
}

public class SetPaperdollSlot : Packet
{
    public ulong entityId;
    public byte slotIndex;
    public uint itemId;
    public bool equipped;

    public SetPaperdollSlot(ulong entityId, byte slotIndex, uint itemId, bool equipped)
    {
        this.packetType = (int)PacketType.SET_PAPERDOLL_SLOT;
        this.entityId = entityId;
        this.slotIndex = slotIndex;
        this.itemId = itemId;
        this.equipped = equipped;
    }
}

public class InitPlayerVals : Packet
{
    public ulong health;
    public ulong maxHealth;
    public ulong mana;
    public ulong maxMana;
    public ulong energy;
    public ulong maxEnergy;

    public uint level;
    public ulong exp;
    public ulong expLevel; //Exp needed to be at this current level
    public ulong expTNL; //Exp till next level

    [JsonConstructor]
    public InitPlayerVals(ulong health, ulong maxHealth, ulong mana, ulong maxMana, ulong energy, ulong maxEnergy, uint level,
        ulong exp, ulong expLevel, ulong expTNL)
    {
        this.packetType = (int)PacketType.INIT_PLAYER_VALS;
        this.health = health;
        this.maxHealth = maxHealth;
        this.mana = mana;
        this.maxMana = maxMana;
        this.energy = energy;
        this.maxEnergy = maxEnergy;
        this.level = level;
        this.exp = exp;
        this.expLevel = expLevel;
        this.expTNL = expTNL;
    }

    public InitPlayerVals(CharProperties props)
    {
        this.packetType = (int)PacketType.INIT_PLAYER_VALS;
        this.health = props.health;
        this.maxHealth = props.maxHealth;
        this.mana = props.mana;
        this.maxMana = props.maxMana;
        this.energy = props.energy;
        this.maxEnergy = props.maxEnergy;
        this.level = props.level;
        this.exp = props.exp;
        this.expLevel = props.expLevel;
        this.expTNL = props.expTNL;
    }

}

public class ChestOpen : Packet
{
    public ulong entityId;

    public ChestOpen(ulong entityId)
    {
        this.packetType = (int)PacketType.CHEST_OPEN;
        this.entityId = entityId;
    }
}

