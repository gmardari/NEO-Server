using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class Character : Entity
    {
        public NetworkClient client;

        public uint direction;
        public CharacterState state;
        public CharProperties props;
        public CharacterDef def;
        public PlayerInventory inv;

        public WalkAnim walkAnim;
        public AttackAnim attackAnim;

        public Chest chest_open;     //A chest that the player has opened and is listening to updates

        public bool CanMove 
        { 
            get 
            {
                return (chest_open == null);
            } 
        }


        public Character(NetworkClient _client, EOMap _map, CharacterDef _def, CharProperties _props, Vector2 _pos, uint _direction) : base(_map, _pos)
        {
            (client, map, def, direction, props) = (_client, _map, _def, _direction, _props);

            entityType = EntityType.PLAYER;
            state = CharacterState.IDLE;
            inv = new PlayerInventory(this);
        }


        public override void Update()
        {
            //Console.WriteLine("Session: " + client.session.state.ToString());
            if (state == CharacterState.WALK)
            {
                //Shifting positions during walk anim
                if (!walkAnim.posShifted && walkAnim.HalftimeReached())
                {
                    SetPosition(EOMap.PositionInDirection(walkAnim.from, walkAnim.direction), true);
                    walkAnim.posShifted = true;
                }

                if (walkAnim.TimeExpired())
                {
                    walkAnim.Clear();
                    SetState(CharacterState.IDLE);
                }
            }
            else if (state == CharacterState.ATTACK)
            {
                if (attackAnim.TimeExpired())
                {
                    SetState(CharacterState.IDLE);
                    attackAnim.Clear();
                }
            }
        }

        public override void SetMap(EOMap map)
        {
            base.SetMap(map);
            //Cancel any animations
            switch(state)
            {
                case CharacterState.WALK:
                    {
                        walkAnim.Clear();
                        SetState(CharacterState.IDLE);
                        break;
                    }
                case CharacterState.ATTACK:
                    {
                        SetState(CharacterState.IDLE);
                        attackAnim.Clear();
                        break;
                    }
            }
        }

        public void WalkTo(uint direction)
        {
            Vector2 newPos = EOMap.PositionInDirection(position, direction);

            SetDirection(direction, true);
            walkAnim = new WalkAnim(position, direction, Server.GetCurrentTime());
            SetState(CharacterState.WALK);

            //Update clients
            SetEntityWalk packet = new SetEntityWalk(entityId, position.x,
                position.y, direction, 1, walkAnim.timeStarted);

            map.SendPacketToClients(packet);

        }

        public void Attack()
        {
            attackAnim = new AttackAnim(direction, Server.GetCurrentTime());
            SetState(CharacterState.ATTACK);

            Cell tile = map.GetCell(EOMap.PositionInDirection(position, direction));

            if (tile != null)
            {
                if (tile.entities.Count > 0)
                {
                    for (var node = tile.entities.First; node != null; node = node.Next)
                    {
                        Entity ent = node.Value;

                        if (ent is Mob)
                        {
                            Mob mob = (Mob)ent;

                            CombatManager.EntityAttackEntity(this, mob);
                            //mob.SetHealth(mob.health - 1);
                            break;
                        }
                    }
                }
            }

            SetEntityAttack packet = new SetEntityAttack(entityId, attackAnim.timeStarted);

            map.SendPacketToClients(packet);
        }

        //Should be called for combat between entities. SetHealth for other situations
        public void TakeDamage(ulong damage, Entity attacker)
        {
            ulong newHealth;
            try
            {
                newHealth = checked(props.health - damage);
            }
            catch (OverflowException)
            {
                newHealth = 0;
            }

            SetHealth(newHealth);

        }

        //Should not be used for combat
        //TODO: Converting both to longs are bad!
        public void SetHealth(ulong newHealth)
        {
            ulong oldHealth = props.health;
            props.health = newHealth;

            if (props.health == 0)
            {
                map.RemoveEntity(this);
            }
            else
            {
                long deltaHealth = (long)newHealth - (long)oldHealth;

                SetEntityHealth packet = new SetEntityHealth(entityId, props.health, props.maxHealth, deltaHealth);
                map.SendPacketToClients(packet);
            }
        }

        public void SetPaperdoll(PaperdollSlot slot, uint val)
        {
            bool equip = val > 0;

            def.doll.Set(slot, val);
            SetPaperdollSlot p = new SetPaperdollSlot(entityId, (byte)slot, val, equip);
            client.SendPacket(p);
            /*
            switch (slot)
            {
                case PaperdollSlot.ARMOR:
                    {
                        def.doll.armor = val;
                        SetPaperdollSlot p = new SetPaperdollSlot(entityId, (byte)PaperdollSlot.ARMOR, val, equip);
                        client.SendPacket(p);
                        break;
                    }
            }
            */
        }

        public void SetDirection(uint direction, bool net_sync)
        {
            this.direction = direction;

            if (net_sync)
            {
                SetEntityDir packet = new SetEntityDir(entityId, direction);

                map.SendPacketToClients(packet);
            }
        }

        public void SetState(CharacterState state)
        {
            this.state = state;
        }

    }

    //Non-appearance properties of a player character. CharacterDef is more concerned with the appearance
    public struct CharProperties
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

        public CharProperties(ulong health, ulong maxHealth, ulong mana, ulong maxMana, ulong energy, ulong maxEnergy, uint level,
            ulong exp, ulong expLevel, ulong expTNL)
        {
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

        public CharProperties(InitPlayerVals packet)
        {
            this.health = packet.health;
            this.maxHealth = packet.maxHealth;
            this.mana = packet.mana;
            this.maxMana = packet.maxMana;
            this.energy = packet.energy;
            this.maxEnergy = packet.maxEnergy;
            this.level = packet.level;
            this.exp = packet.exp;
            this.expLevel = packet.expLevel;
            this.expTNL = packet.expTNL;
        }
    }
}
