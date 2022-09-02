using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public struct Vector2
    {
        public int x;
        public int y;

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator *(Vector2 a, int m)
        {
            return new Vector2(a.x * m, a.y * m);
        }

        public static Vector2 operator /(Vector2 a, int m)
        {
            return new Vector2(a.x / m, a.y / m);
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return (a.x == b.x && a.y == b.y);
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return (a.x != b.x || a.y != b.y);
        }

        public override bool Equals(object obj)
        {
            return Equals((Vector2) obj);
        }

        public bool Equals(Vector2 other)
        {
            return (this.x == other.x && this.y == other.y);
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }

    public struct Orientation
    {
        public uint mapId;
        public Vector2 position;
        public uint direction;

         
    }

    public struct IntRange
    {
        public int min;
        public int max;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

    }

    public struct LongRange
    {
        public long min;
        public long max;

        public LongRange(long min, long max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public enum EntityType : uint
    {
        PLAYER,
        NPC,
        ITEM,
        WARP,
        CHEST,
        NPC_SPAWNER,
        ITEM_SPAWNER
    }

    public enum EntityProperty : uint
    {
        HEALTH, //ulong
        MANA,   //ulong
        ENERGY, //ulong
        NAME,   //variable string
        CLASS   //variable string
    }

    public enum CharacterState
    {
        IDLE,
        WALK,
        ATTACK,
        SPELL_CAST,
        DYING
    }

    public class CharacterDef
    {
        public string name;
        public byte gender;
        public byte race;
        public byte skinColour;
        public byte hairStyle;
        public byte hairColour;
        public Paperdoll doll;

        public CharacterDef() { }

        public CharacterDef(SetCharacterDef p)
        {
            (name, gender, race, skinColour, hairStyle, hairColour) = (p.def.name, p.def.gender, p.def.race,
                p.def.skinColour, p.def.hairStyle, p.def.hairColour);
            doll = p.def.doll;
            /*
            doll = new Paperdoll
            {
                hat = p.def.hat,
                armor = p.def.armor,
                boots = p.def.shoes,
                back = p.def.back,
                weapon = p.def.weapon
            };
            */
        }

    }

    public struct NpcDef
    {
        public string name;
        public int npcId;
        public long maxHealth;
        public int npcType;
        public int gfxId;

        public NpcDef(int _npcId, NpcDataEntry entry)
        {
            name = entry.Name;
            npcId = _npcId;
            maxHealth = entry.MaxHealth;
            npcType = entry.NpcType;
            gfxId = entry.GfxId;
        }
    }

    public enum NpcState
    {
        IDLE,
        WALK,
        ATTACK,
        SPELL_CAST,
        DYING
    }


    public enum NpcType
    {
        NONE,
        MOB_PASSIVE,
        MOB_AGGRESSIVE
    }

    public struct WalkAnim
    {
        public Vector2 from;
        public uint direction;
        public bool valid;
        //The time needed for one full walk animation
        private const long timeStep = 750;
        public long timeStarted;
        public bool posShifted;

        public WalkAnim(Vector2 from, uint direction, long timeStarted)
        {
            this.from = from;
            this.direction = direction;
            this.valid = true;
            this.posShifted = false;
            //TODO: Change
            //this.timeStarted = NetworkManager.Singleton.ServerTime.TimeAsFloat;
            this.timeStarted = timeStarted;
            
        }

        public float GetAlpha()
        {
            long timeDelta = Server.GetCurrentTime() - timeStarted; 
            return (timeDelta / timeStep);
        }

        public float GetCappedAlpha()
        {
            return Math.Min(GetAlpha(), 1.0f);
        }

        public bool HalftimeReached()
        {
            if((Server.GetCurrentTime() - timeStarted) >= (timeStep / 2))
            {
                return true;
            }

            return false;
        }

        public bool TimeExpired()
        {
            if ((Server.GetCurrentTime() - timeStarted) >= timeStep)
                return true;

            return false;
        }

        public void Clear()
        {
            valid = false;
        }

        public override string ToString()
        {
            return $"From: {from}, Dir: {direction}, Time: {timeStarted}, Valid: {valid}";

        }
    }

    public struct AttackAnim
    {
        public uint direction;
        public bool valid;
        public long timeStarted;

        public
            const long timeStep = 500;

        public AttackAnim(uint direction, long timeStarted)
        {
            this.direction = direction;
            this.valid = true;
            this.timeStarted = timeStarted;
        }

        public float GetAlpha()
        {
            long timeDelta = Server.GetCurrentTime() - timeStarted;
            return (timeDelta / timeStep);
        }

        public float GetCappedAlpha()
        {
            return Math.Min(GetAlpha(), 1.0f);
        }

        public bool TimeExpired()
        {
            if ((Server.GetCurrentTime() - timeStarted) >= timeStep)
                return true;

            return false;
        }

        public void Clear()
        {
            valid = false;
        }

        public override string ToString()
        {
            return $"Dir: {direction}, Time: {timeStarted}, Valid: {valid}";

        }
    }


    public enum NetworkSessionState
    {
        PINGING,
        ACCEPTED,
        AUTH,
        IN_GAME
    }

    public struct NetworkSession
    {
        public string username;
        public string char_name;
        public uint char_id;

        public NetworkSessionState state;
    }

    public enum ACCOUNT_CREATE_RESP : byte
    {
        SUCCESS,
        USER_INVALID,
        USER_TAKEN,
        PASS_INVALID,
        REALNAME_INVALID,
        LOCATION_INVALID,
        EMAIL_INVALID,
        SERVER_ERROR
    }

    public enum CHARACTER_CREATE_RESP : byte
    {
        SUCCESS,
        NAME_TAKEN,
        CHARS_LIMIT_REACHED,
        GENDER_INVALID,
        HAIRSTYLE_INVALID,
        HAIRCOLOUR_INVALID,
        SKINCOLOUR_INVALID,
        SERVER_ERROR
    }
}
