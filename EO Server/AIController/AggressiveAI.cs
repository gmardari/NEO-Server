using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class AggressiveAI : I_AIController
    {
        private MobBehaviour behaviour;
        private Mob mob;
        private Character target;

        public int Behaviour { get { return (int)behaviour; } }
        public Mob Mob { get { return mob; } }

        public AggressiveAI(Mob mob)
        {
            this.mob = mob;
            behaviour = MobBehaviour.FIDGET;
        }

        public void Update()
        {
            switch(behaviour)
            {
                case MobBehaviour.FIDGET:
                    {
                        if (mob.state == NpcState.IDLE)
                        {
                            //Do fidget (patrol behaviour)
                            if (mob.fidgetInterval <= 0)
                            {
                                Random rand = new Random();
                                //Fidget time is in milliseconds
                                mob.fidgetInterval = rand.Next(mob.fidgetTime.min, mob.fidgetTime.max + 1) * 1000;
                                mob.fidgetTimer = Server.GetCurrentTime();
                            }
                            else if (Server.GetCurrentTime() - mob.fidgetTimer >= mob.fidgetInterval)
                            {
                                uint[] dirList = new uint[4];
                                uint len = 0;

                                for (uint dir = 0; dir < dirList.Length; dir++)
                                {
                                    if (mob.map.CanMoveToPos(mob.position, dir))
                                        dirList[len++] = dir;
                                }

                                if (len > 0)
                                {
                                    Random rand = new Random();
                                    uint chosenDirection = dirList[rand.Next((int)len)];
                                    mob.WalkTo(chosenDirection);

                                    mob.ResetFidget();
                                }
                                //Cannot move anywhere. Reset the figet timer
                                else
                                    mob.ResetFidget();



                            }

                        }
                        break;
                    }
                case MobBehaviour.FOLLOW:
                    {
                      
                        
                        if(target.map == null || target.map.mapId != mob.map.mapId)
                        {
                            behaviour = MobBehaviour.FIDGET;
                            target = null;
                            break; //Break early so it doesn't move
                        }
                        //Console.WriteLine(target.position);

                        if (UtilFunctions.GetDistance(mob.position, target.position) <= 1)
                        {
                            behaviour = MobBehaviour.ATTACK;
                            break; //Break early so it doesn't move
                        }

                        if(mob.state == NpcState.IDLE)
                        {
                            uint direction = 0;

                            if (target.position.y != mob.position.y)
                            {
                                int delta = target.position.y - mob.position.y;

                                if (delta > 0)
                                    direction = 3;
                                else
                                    direction = 1;
                            }
                            else if (target.position.x != mob.position.x)
                            {
                                int delta = target.position.x - mob.position.x;

                                if (delta > 0)
                                    direction = 2;
                                else
                                    direction = 0;
                            }

                            mob.WalkTo(direction);
                        }

                        

                        break;
                    }

                case MobBehaviour.ATTACK:
                    {
                        if (target.map == null || target.map.mapId != mob.map.mapId)
                        {
                            behaviour = MobBehaviour.FIDGET;
                            target = null;
                            break; //Break early so it doesn't move
                        }

                        int distance = UtilFunctions.GetDistance(mob.position, target.position);

                        if (distance > 1)
                        {
                            behaviour = MobBehaviour.FOLLOW;
                            break; //Break early so it doesn't move
                        }
                        //Mob is idle and within attack range
                        else if(mob.state == NpcState.IDLE)
                        {
                            uint direction = UtilFunctions.GetDirection(mob.position, target.position);

                            mob.SetDirection(direction);
                            mob.Attack();
                        }

                        break;
                    }
            }
        }

        public void OnTakeDamage(ulong damage, Entity attacker)
        {
            switch(behaviour)
            {
                case MobBehaviour.FIDGET:
                    {
                        if(attacker.entityType == EntityType.PLAYER)
                        {
                            if (UtilFunctions.GetDistance(mob.position, attacker.position) <= 1)
                                behaviour = MobBehaviour.ATTACK;
                            else
                                behaviour = MobBehaviour.FOLLOW;

                            target = attacker as Character;
                        }
                        break;
                    }
            }
        }
    }
}
