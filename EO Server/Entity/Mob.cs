using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    //An entity that can be in combat
    public class Mob : Entity
    {
        public NpcDef def;
        public uint direction;
        public NpcState state;
        public I_AIController ai;
        public WalkAnim walkAnim;
        public AttackAnim attackAnim;
        public IntRange fidgetTime;

        public ulong health;
        public ulong maxHealth;

        public long fidgetTimer;
        public long fidgetInterval;

        //TODO: Edit fidget time to time set in the map file
        public Mob(EOMap _map, NpcDef _def, Vector2 _pos, uint _direction, IntRange fidgetTime) : base(_map, _pos)
        {
            (def, direction) = (_def, _direction);
            entityType = EntityType.NPC;
            state = NpcState.IDLE;
            this.ai = new AggressiveAI(this);
            this.fidgetTime = fidgetTime;
            maxHealth = (ulong) def.maxHealth;
            health = maxHealth;
        }

        public override void Update()
        {
            /*
            if (state == NpcState.IDLE)
            {
                //Do fidget (patrol behaviour)
                if (fidgetInterval <= 0)
                {
                    Random rand = new Random();
                    //Fidget time is in milliseconds
                    fidgetInterval = rand.Next(fidgetTime.min, fidgetTime.max + 1) * 1000;
                    fidgetTimer = Server.GetCurrentTime();
                }
                else if (Server.GetCurrentTime() - fidgetTimer >= fidgetInterval)
                {
                    uint[] dirList = new uint[4];
                    uint len = 0;

                    for (uint dir = 0; dir < dirList.Length; dir++)
                    {
                        if (map.CanMoveToPos(position, dir))
                            dirList[len++] = dir;
                    }

                    if (len > 0)
                    {
                        Random rand = new Random();
                        uint chosenDirection = dirList[rand.Next((int)len)];
                        WalkTo(chosenDirection);

                        ResetFidget();
                    }
                    //Cannot move anywhere. Reset the figet timer
                    else
                        ResetFidget();



                }

            }
            */
            if (state == NpcState.WALK)
            {
                //Shifting positions during walk anim
                if (!walkAnim.posShifted && walkAnim.HalftimeReached())
                {
                    SetPosition(EOMap.PositionInDirection(walkAnim.from, walkAnim.direction), true);
                    walkAnim.posShifted = true;
                }

                //Finish walk animation
                if (walkAnim.TimeExpired())
                {
                    walkAnim.Clear();
                    SetState(NpcState.IDLE);
                }
            }
            else if (state == NpcState.ATTACK)
            {
                if (attackAnim.TimeExpired())
                {
                    SetState(NpcState.IDLE);
                    attackAnim.Clear();
                }
            }

            if (ai != null)
                ai.Update();
        }

        //Map is valid as this will be called from EOMap
        public void WalkTo(uint direction)
        {
            Vector2 newPos = EOMap.PositionInDirection(position, direction);

            SetDirection(direction);
            walkAnim = new WalkAnim(position, direction, Server.GetCurrentTime());
            SetState(NpcState.WALK);

            //Update clients
            SetEntityWalk packet = new SetEntityWalk(entityId, position.x,
                position.y, direction, 1, walkAnim.timeStarted);

            map.SendPacketToClients(packet);

        }

        public void Attack()
        {
            if (state != NpcState.IDLE)
                return;

            attackAnim = new AttackAnim(direction, Server.GetCurrentTime());
            SetState(NpcState.ATTACK);

            Cell tile = map.GetCell(EOMap.PositionInDirection(position, direction));

            if (tile != null)
            {
                if (tile.entities.Count > 0)
                {
                    for (var node = tile.entities.First; node != null; node = node.Next)
                    {
                        Entity ent = node.Value;

                        if (ent is Character character)
                        {
                            CombatManager.EntityAttackEntity(this, character);
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
                newHealth = checked(health - damage);
            }
            catch(OverflowException)
            {
                newHealth = 0;
            }

            SetHealth(newHealth);
            
            if(valid)
            {
                if(ai != null)
                    ai.OnTakeDamage(damage, attacker);
            }
        }

        //Should not be used for combat
        //TODO: Converting both to longs are bad!
        public void SetHealth(ulong newHealth)
        {
            ulong oldHealth = health;
            health = newHealth;

            if (health == 0)
            {
                map.RemoveEntity(this);
            }
            else
            {
                long deltaHealth = (long)newHealth - (long)oldHealth;

                health = newHealth;
                SetEntityHealth packet = new SetEntityHealth(entityId, health, maxHealth, deltaHealth);
                map.SendPacketToClients(packet);
            }
        }
        public void SetDirection(uint direction)
        {
            this.direction = direction;

            SetEntityDir packet = new SetEntityDir(entityId, direction);

            map.SendPacketToClients(packet);
        }

        public void SetState(NpcState state)
        {
            this.state = state;
            ResetFidget();
        }

        public void ResetFidget()
        {
            fidgetInterval = 0;
            fidgetTimer = 0;
        }

    }
}
